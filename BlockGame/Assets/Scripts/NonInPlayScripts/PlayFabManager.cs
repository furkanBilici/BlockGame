using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Collections.Generic;
using System;
using PlayFab.CloudScriptModels;
using Newtonsoft.Json.Linq;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string LEADERBOARD_STATISTIC_NAME = "global_highest_score";
    public bool IsLoggedIn=false;  
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        IsLoggedIn = false;
        StartGuestSession();
    }
    void StartGuestSession()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("PlayFab'e baþarýlý bir þekilde baðlandý.");
        IsLoggedIn = true;
        // Burada LootLocker kodunda yaptýðýn gibi konum bilgisini çekebilirsin.
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.CheckAndUpdateLocation();
        }
    }
    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("PlayFab'e baðlanýlamadý: " + error.GenerateErrorReport());
    }

    public void SubmitScore(int scoreToSubmit)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName=LEADERBOARD_STATISTIC_NAME,
                    Value = scoreToSubmit   
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request,OnUpdateSuccess,OnUpdateFailure);   
    }

    private void OnUpdateSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Skor baþarýyla lider panosuna gönderildi!");
    }

    private void OnUpdateFailure(PlayFabError error)
    {
        Debug.LogError("Skor gönderilirken hata oluþtu: " + error.GenerateErrorReport());
    }

    public void SetPLayerName(string playerName, Action<bool> onComplete)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = playerName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, (result) =>
        {
            Debug.Log("Ýsim baþarýyla eklendi: " + result.DisplayName);
            PlayerPrefs.SetString(PLAYER_NAME_KEY, result.DisplayName);
            PlayerPrefs.Save();
            onComplete?.Invoke(true);
            PlayerPrefs.SetInt("NameSavedOnPlayFab", 1);
            PlayerPrefs.Save();
        }, (error) =>
        {
            Debug.LogError("Ýsim eklenemedi: " + error.GenerateErrorReport());
            PlayerPrefs.SetInt("NameSavedOnPlayFab", 0);
            PlayerPrefs.Save();
            onComplete?.Invoke(false);
        });
    }

    public void CheckIfPlayerExists(string nameToCheck, Action<bool> onComplete)
    {
        var request= new ExecuteCloudScriptRequest
        {
            FunctionName="CheckIfDisplayNameExists",
            FunctionParameter = new { displayName = nameToCheck }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            if (result.Error != null)
            {
                Debug.LogError("CloudScript çaðrýlýrken hata: " + result.Error.Message);
                onComplete?.Invoke(false);
                return;
            }

            try
            {
                var rawResult = result.FunctionResult;
                var json = JObject.FromObject(rawResult);
                bool isNameTaken = json["isNameTaken"]?.Value<bool>() ?? false;
                onComplete?.Invoke(isNameTaken);
            }
            catch (Exception ex)
            {
                Debug.LogError("FunctionResult iþlenirken hata: " + ex.Message);
                onComplete?.Invoke(false);
            }
        },
        (error) =>
        {
            Debug.LogError("Cloud Script isteði baþarýsýz " + error.GenerateErrorReport());
            onComplete?.Invoke(false);
        });

    }
    public void SavePlayerLocation(string countryCode, string city)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            {"CountryCode", countryCode},
            {"City", city}
        },
            Permission = UserDataPermission.Public // Herkese açýk yapabiliriz.
        };

        PlayFabClientAPI.UpdateUserData(request, (result) =>
        {
            Debug.Log("Konum bilgileri baþarýyla kaydedildi.");
        }, (error) =>
        {
            Debug.LogError("Konum bilgileri kaydedilirken hata: " + error.GenerateErrorReport());
        });
    }
    public void GetFilteredLeaderboard(int listType, string countryCode, string city, Action<List<PlayerLeaderboardEntry>> onComplete)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetFilteredLeaderboard",
            FunctionParameter = new
            {
                listType = listType,
                playerCountryCode = countryCode,
                city = city
            }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            if (result.Error != null)
            {
                Debug.LogError("Cloud Script çaðrýlýrken hata: " + result.Error.Message);
                onComplete?.Invoke(null);
                return;
            }

            if (result.FunctionResult == null)
            {
                Debug.LogError("Cloud Script'ten geçersiz sonuç döndürüldü.");
                onComplete?.Invoke(null);
                return;
            }
            try
            {
                var jsonResult = JObject.FromObject(result.FunctionResult);
                var leaderboardArray = jsonResult["leaderboard"];

                var filteredLeaderboard = leaderboardArray.ToObject<List<PlayerLeaderboardEntry>>();
                onComplete?.Invoke(filteredLeaderboard);
            }
            catch (Exception ex)
            {
                Debug.LogError("Cloud Script'ten dönen sonuç ayrýþtýrýlamadý: " + ex.Message);
                onComplete?.Invoke(null);
            }
        },
        (error) =>
        {
            Debug.LogError("Cloud Script isteði baþarýsýz: " + error.GenerateErrorReport());
            onComplete?.Invoke(null);
        });
    }
}
