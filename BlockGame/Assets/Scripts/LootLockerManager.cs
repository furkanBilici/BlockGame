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
            LootLockerSDKManager.GetPlayerName((response) =>
            {
                if (response.success && !string.IsNullOrEmpty(response.name))
                {
                    playerName = response.name;
                    StartCoroutine(SubmitScoreRoutine(playerName, scoreToSubmit));
                }
                else
                {
                    string guestName = "Player" + Random.Range(1000, 9999);
                    SetPlayerName(guestName, () =>
                    {
                        StartCoroutine(SubmitScoreRoutine(guestName, scoreToSubmit));
                    });
                }
            });
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
}
