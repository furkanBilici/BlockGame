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
    private const string LEADERBOARD_STATISTIC_NAME = "global_high_scores";
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
                    StatisticName="global_highest_score",
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

    public void SetPLayerName(string playerName, Action onComplete = null)
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
            onComplete?.Invoke();
        }, (error) =>
        {
            Debug.LogError("Ýsim eklenemedi: " + error.GenerateErrorReport());
            onComplete?.Invoke();
        });
    }

    public void CheckIfPlayerExists(string nameToCheck, Action<bool> onComplete)
    {
        var request= new ExecuteCloudScriptRequest
        {
            FunctionName="CheckIfDisplayNmaeExists",
            FunctionParameter = new { displayName = nameToCheck }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            if (result.Error != null)
            {
                Debug.LogError("CloudScript çaðrýlýrkn hata " + result.Error);
                onComplete?.Invoke(false);
                return;
            }
            var jsonResult = (JObject)result.FunctionResult;
            bool isNameTaken = (bool)jsonResult["isNameTaken"];
            if (isNameTaken)
            {
                Debug.Log("isim alýnmýþ");
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.Log("isim alýnmamýþ kullanýlabilir");
                onComplete?.Invoke(false);
            }
        },
        (error) =>
        {
            Debug.LogError("Cloud Script isteði baþarýsýz " + error.GenerateErrorReport());
            onComplete?.Invoke(false);
        }
        );
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

            var jsonResult = (JObject)result.FunctionResult;
            var leaderboardArray = jsonResult["leaderboard"];

            // Lider tablosunu PlayerLeaderboardEntry listesine çevir.
            var filteredLeaderboard = leaderboardArray.ToObject<List<PlayerLeaderboardEntry>>();
            onComplete?.Invoke(filteredLeaderboard);
        },
        (error) =>
        {
            Debug.LogError("Cloud Script isteði baþarýsýz: " + error.GenerateErrorReport());
            onComplete?.Invoke(null);
        });
    }
}
