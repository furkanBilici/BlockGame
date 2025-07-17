using LootLocker.Requests;
using UnityEngine;

public class LootLockerManager : MonoBehaviour
{
    public static LootLockerManager Instance { get; private set; }
    private const string LEADERBOARD_KEY = "global_high_scores";
    private const string PLAYER_NAME_KEY = "PlayerName";


    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);

    }
    private void Start()
    {
        StartGuestSession();
    }
    public void StartGuestSession()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("lootlocker basarili sekilde baglandi");
            }
            else
            {
                Debug.Log("lootlocker baglanamadi");
            }
        }
        );
    }

    public void SubmitScore(int scoreToSubmit)
    {
        string playerName;

        string savedName = PlayerPrefs.GetString(PLAYER_NAME_KEY, null);
        if (!string.IsNullOrEmpty(savedName))
        {
            playerName = savedName;
            StartCoroutine(SubmitScoreRoutine(playerName, scoreToSubmit));
        }
        else
        {
            Debug.Log("oyuncu yok");
            return;
        }
    }
    private System.Collections.IEnumerator SubmitScoreRoutine(string playerName, int scoreToSubmit)
    {
        bool done = false;
        LootLockerSDKManager.SubmitScore(playerName, scoreToSubmit, LEADERBOARD_KEY, (response) =>
        {
            if (response.success)
            {
                Debug.Log("skor gönderildi " + scoreToSubmit);
            }
            else
            {
                Debug.LogError("skor gönderilemedi ");
            }
            done = true;
        }
        );
        yield return new WaitWhile(() => done == false);
    }
    public void SetPlayerName(string playerName, System.Action onComplete=null)
    {
        LootLockerSDKManager.SetPlayerName(playerName, (response) =>
        {
            if (response.success)
            {
                PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName); 
                PlayerPrefs.Save(); 
                Debug.Log("isim eklendi " + playerName);
            }
            else
            {
                Debug.LogError("isim eklenemedi ");
            }
            onComplete?.Invoke(); 
        });
    }

    public void CheckIfPlayerExists(string nameToCheck, System.Action<bool> onComplete)
    {
        LootLockerSDKManager.GetScoreList(LEADERBOARD_KEY, 1000, 0, (response) =>
        {
            if (response.success)
            {
                bool nameFound = false;
                foreach (var member in response.items)
                {
                    if (string.Equals(member.player.name, nameToCheck, System.StringComparison.OrdinalIgnoreCase))
                    {
                        nameFound = true;
                        break;
                    }
                }
                onComplete?.Invoke(nameFound);
            }
            else
            {
                Debug.LogError("Liderlik tablosu çekilirken hata: " + response.text);
                onComplete?.Invoke(true);
            }
        });
    }
}
