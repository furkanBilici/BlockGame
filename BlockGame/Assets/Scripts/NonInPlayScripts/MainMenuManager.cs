using LootLocker.Requests;
using System;
using System.Collections;
using TMPro;

using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject HighScore;
    public GameObject quitPanel;
    public GameObject Text;
    public GameObject settingPanel;
    public GameObject modePanel;
    public GameObject customModePanel;
    public Button sound;
    public Button music;
    bool modePanelBool;
    bool customModePanelBool;

    public Button sizeButton;
    public Button difficultyButton;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (quitPanel.activeSelf)
            {
                quitPanel.SetActive(false);
            }
            else
            {
                quitPanel.SetActive(true);
            }
        }
        TextAnimation();
    }
    private void Start()
    {
        CheckLanguage();
        if (AdsManager.Instance != null) AdsManager.Instance.LoadBannerAd();
        if (AudioManager.Instance != null) AudioManager.Instance.StopAllMusic();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("MenuMusic");
        modePanelBool = modePanel.activeSelf;
        SoundButtonControl();
        MusicButtonControl();
        HighScore.SetActive(true);
        if (PlayerPrefs.GetInt("HighestScore", 0) > 0)
        {
            if (PlayerPrefs.GetString("Locale") == english.Identifier.Code)
                HighScore.GetComponent<TextMeshProUGUI>().text = "HIGH SCORE: " + PlayerPrefs.GetInt("HighestScore", 0);
            else HighScore.GetComponent<TextMeshProUGUI>().text = "YÜKSEK SKOR: " + PlayerPrefs.GetInt("HighestScore", 0);
        }


        boardScale = PlayerPrefs.GetInt("boardScale", 1);
        difficulty = PlayerPrefs.GetInt("difficulty", 0);
        ButtonColorTextControllerForCustomGame(boardScale, sizeButton);
        ButtonColorTextControllerForCustomGame(difficulty, difficultyButton);
        if (PlayFabManager.Instance != null) StartCoroutine (CheckForPlayerName());
        
    }
    public void StartGame()
    {
        SceneManager.LoadScene(2);
    }
    public void QuitGame()//oyundan çýkýþ butonu (2 butonda kullanýlacak)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("OutGame");
        Application.Quit();
    }
    public void BackToMenu()
    {
        quitPanel.SetActive(false);
        settingPanel.SetActive(false);
    }
    public Vector3 minScale = Vector3.one;
    public Vector3 maxScale = new Vector3(1.2f, 1.2f, 1f);
    public float speed = 2f;
    private bool growing = true;

    void TextAnimation()
    {
        Text.transform.localScale = Vector3.Lerp(
            Text.transform.localScale,
            growing ? maxScale : minScale,
            Time.deltaTime * speed
        );
        if (Vector3.Distance(Text.transform.localScale, maxScale) < 0.01f)
            growing = false;
        else if (Vector3.Distance(Text.transform.localScale, minScale) < 0.01f)
            growing = true;
    }
    public void Settings()
    {
        settingPanel.SetActive(true);
    }
    public void CloseSound()
    {
        AudioManager.Instance.closeSounds = !AudioManager.Instance.closeSounds;
        SoundButtonControl();
    }
    void SoundButtonControl()
    {
        if (!AudioManager.Instance.closeSounds)
        {
            PlayerPrefs.SetInt("SoundClosed", 1);
            sound.GetComponentInChildren<TextMeshProUGUI>().text = "ON";
            sound.image.color = Color.green;
        }
        else
        {
            PlayerPrefs.SetInt("SoundClosed", 0);
            sound.GetComponentInChildren<TextMeshProUGUI>().text = "OFF";
            sound.image.color = Color.red;
        }
        PlayerPrefs.Save();
    }

    public void CloseMusic()
    {
        AudioManager.Instance.closeMusics = !AudioManager.Instance.closeMusics;
        MusicButtonControl();
    }

    void MusicButtonControl()
    {
        if (!AudioManager.Instance.closeMusics)
        {
            PlayerPrefs.SetInt("MusicClosed", 1);
            music.GetComponentInChildren<TextMeshProUGUI>().text = "ON";
            if (AudioManager.Instance != null) { AudioManager.Instance.PlayMusic("MenuMusic"); }
            music.image.color = Color.green;
        }
        else
        {
            PlayerPrefs.SetInt("MusicClosed", 0);
            music.GetComponentInChildren<TextMeshProUGUI>().text = "OFF";
            music.image.color = Color.red;
        }
        PlayerPrefs.Save();
    }


    public void ModePanelControl()
    {
        if (modePanelBool)
        {
            modePanelBool = false;
            modePanel.SetActive(false);
        }
        else
        {
            modePanelBool = true;
            modePanel.SetActive(true);
        }
    }
    [Header("TimedMode")]
    [SerializeField] float time = 25f;
    [SerializeField] int neededScore = 20;
    [SerializeField] GameObject timedPanel;
    public Button level;

    public void TimedPanelController()
    {
        if (timedPanel.activeSelf)
        {
            timedPanel.SetActive(false);
        }
        else
        {
            timedPanel.SetActive(true);
            level.GetComponentInChildren<TextMeshProUGUI>().text = "LEVEL " + (PlayerPrefs.GetInt("CompletedLevels", 0) + 1);
        }
    }
    public void StartTimedGame()
    {
        int level = PlayerPrefs.GetInt("CompletedLevels", 0) + 1;
        PlayerPrefs.SetInt("TimedGameScore", (int)(neededScore * level * MathF.Sqrt(level)));
        PlayerPrefs.SetFloat("TimedGameTime", time * level);
        SceneManager.LoadScene(3);
    }


    [Header("CustomMode")]
    [SerializeField] int boardScale = 0;//0=7x7,1=8x8,2=9x9
    [SerializeField] int difficulty = 0;//0=easy, 1 normal, 2=hard
    public void CustomModePanel()
    {
        if (customModePanelBool)
        {
            customModePanelBool = false;
            customModePanel.SetActive(false);
        }
        else
        {
            customModePanelBool = true;
            customModePanel.SetActive(true);
        }
    }


    public void BoardScale()
    {
        if (boardScale >= 2)
        {
            boardScale = 0;
            PlayerPrefs.SetInt("boardScale", 0);
        }
        else
        {
            boardScale++;
            PlayerPrefs.SetInt("boardScale", boardScale);
            if (boardScale == 1)
            {
                sizeButton.image.color = Color.yellow;
            }
            if (boardScale == 2)
            {
                sizeButton.image.color = Color.red;
            }
        }
        ButtonColorTextControllerForCustomGame(boardScale, sizeButton);
        PlayerPrefs.Save();
    }
    public void Difficulty()
    {
        if (difficulty >= 2)
        {
            difficulty = 0;
            PlayerPrefs.SetInt("difficulty", 0);
        }
        else
        {
            difficulty++;
            PlayerPrefs.SetInt("difficulty", difficulty);
            if (difficulty == 1)
            {
                difficultyButton.image.color = Color.yellow;
            }
            if (difficulty == 2)
            {
                difficultyButton.image.color = Color.red;
            }
        }
        ButtonColorTextControllerForCustomGame(difficulty, difficultyButton);
        PlayerPrefs.Save();
    }

    void ButtonColorTextControllerForCustomGame(int value, Button button)
    {
        if (value == 1)
        {
            button.image.color = Color.yellow;
            if (button.name == "hardness") button.GetComponentInChildren<TextMeshProUGUI>().text = "NORMAL";
            else button.GetComponentInChildren<TextMeshProUGUI>().text = "8x8";

        }
        else if (value == 2)
        {
            button.image.color = Color.red; if (button.name == "hardness")
            {
                if (PlayerPrefs.GetString("Locale") == english.Identifier.Code)
                    button.GetComponentInChildren<TextMeshProUGUI>().text = "HARD";
                else button.GetComponentInChildren<TextMeshProUGUI>().text = "ZOR";
            }
            else button.GetComponentInChildren<TextMeshProUGUI>().text = "9x9";

        }
        else
        {
            button.image.color = Color.green;
            if (button.name == "hardness")
            {
                if (PlayerPrefs.GetString("Locale") == english.Identifier.Code)
                    button.GetComponentInChildren<TextMeshProUGUI>().text = "EASY";
                else button.GetComponentInChildren<TextMeshProUGUI>().text = "KOLAY";
            }
            else button.GetComponentInChildren<TextMeshProUGUI>().text = "7x7";

        }
    }
    public void PlayCustomMode()
    {
        SceneManager.LoadScene(4);
    }

    public void ButtonClickSound()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
    }



    [Header("HighScoreBoard")]
    public GameObject HighscoreBoardPanel;
    public Transform scoreContentParent;
    public GameObject scoreEntryPrefab;
    public GameObject noConnectionPanel;

    public TextMeshProUGUI highscoreTitle;

    public void NoConnectionPanel()
    {
        if (noConnectionPanel.activeSelf)
        {
            noConnectionPanel.SetActive(false);
        }
        else
        {
            noConnectionPanel.SetActive(true);
        }
    }
    public void ShowHighScoreBoardPanel(int listType)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            noConnectionPanel.SetActive(true);
            return;
        }
        if (listType==-1) { HighscoreBoardPanel.SetActive(false); return; }

        HighscoreBoardPanel.SetActive(true);
        foreach (Transform child in scoreContentParent.transform)
            Destroy(child.gameObject);

        string title = "HIGHEST SCORES ";
        string countryCode = PlayerPrefs.GetString("CountryCode", "");
        string city = PlayerPrefs.GetString("City", "");

        switch (listType)
        {
            case 0:
                title += "GLOBAL";
                if (PlayerPrefs.GetString("Locale") == turkish.Identifier.Code) title = "EN YÜKSEK SKORLAR GLOBAL";
                break;
            case 1:
                title += "MY COUNTRY";
                if (PlayerPrefs.GetString("Locale") == turkish.Identifier.Code) title = "EN YÜKSEK SKORLAR ÜLKEM";
                break;
            case 2:
                title += "MY CITY";
                if (PlayerPrefs.GetString("Locale") == turkish.Identifier.Code) title = "EN YÜKSEK SKORLAR ÞEHRÝM";
                break;
        }
        highscoreTitle.text = title;

        // YENÝ PlayFab çaðrýsý
        PlayFabManager.Instance.GetFilteredLeaderboard(listType, countryCode, city, (leaderboardList) =>
        {
            if (leaderboardList != null)
            {
                for (int i = 0; i < leaderboardList.Count; i++)
                {
                    GameObject entry = Instantiate(scoreEntryPrefab, scoreContentParent);
                    TextMeshProUGUI rankText = entry.transform.Find("RankText").GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI nameText = entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI scoreText = entry.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();

                    rankText.text = (i + 1) + "."; // Sadece filtrelenmiþ listedeki sýrayý kullanýyoruz.
                    nameText.text = leaderboardList[i].DisplayName;
                    scoreText.text = leaderboardList[i].StatValue.ToString();
                }
            }
            else
            {
                Debug.LogError("Liderlik listesi getirilemedi.");
            }
        });
    }

    [Header("Player Name")]
    public GameObject setNamePanel;
    public TMP_InputField nameField;
    private const string PLAYER_NAME_KEY = "PlayerName";
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI statusText;
    public Button confirmNameButton;
    public TextMeshProUGUI welcomText;
    IEnumerator CheckForPlayerName()
    {
        // Login tamamlanana kadar bekle
        while (!PlayFabManager.Instance.IsLoggedIn)
            yield return null;

        string playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY);

        if (string.IsNullOrEmpty(playerName) && Application.internetReachability != NetworkReachability.NotReachable)
        {
            FirstLanguageChecker();
            setNamePanel.SetActive(true);
            yield break; // Ýsim boþsa ve panel açýldýysa devam etme
        }

        if (!string.IsNullOrEmpty(playerName))
        {
            PlayFabManager.Instance.CheckIfPlayerExists(playerName, (nameExists) =>
            {
                if (nameExists)
                {
                    welcomText.text = (PlayerPrefs.GetString("Locale") == english.Identifier.Code)
                        ? "WELCOME: " + playerName
                        : "HOÞGELDÝN: " + playerName;
                }
                else
                {
                    PlayFabManager.Instance.SetPLayerName(playerName);
                    welcomText.text = (PlayerPrefs.GetString("Locale") == english.Identifier.Code)
                        ? "WELCOME: " + playerName
                        : "HOÞGELDÝN: " + playerName;
                }
            });

            ScoreChecker(); // Giriþ tamamlandýktan sonra skor kontrolü
        }
    }

    public void ConfirmPlayerName()
    {
        string playerName = nameField.text;

        if (string.IsNullOrWhiteSpace(playerName) || playerName.Length < 3)
        {
            StartCoroutine(ShowErrorText());
            return;
        }

        confirmNameButton.interactable = false;
        statusText.color = Color.white;
        if (PlayerPrefs.GetString("Locale") == english.Identifier.Code) statusText.text = "Checking...";
        else statusText.text = "Kontrol ediliyor...";

        PlayFabManager.Instance.CheckIfPlayerExists(playerName, (nameExists) =>
        {
            confirmNameButton.interactable = true;
            if (nameExists)
            {
                if (PlayerPrefs.GetString("Locale") == english.Identifier.Code) statusText.text = "This username is used by another player";
                else statusText.text = "Bu kullanýcý adý kullanýlýyor";
                statusText.color = Color.red;
            }
            else
            {
                statusText.text = "Name saved!";
                PlayFabManager.Instance.SetPLayerName(playerName);
                setNamePanel.SetActive(false);
                if (PlayerPrefs.GetString("Locale") == english.Identifier.Code) welcomText.text = "WELCOME: " + playerName;
                else welcomText.text = "HOÞGELDÝN: " + playerName;
                HowToPlayPanelButton();
                
            }
        });
    }
    System.Collections.IEnumerator ShowErrorText()
    {
        errorText.text = "Username must be longer than 3 characters!";
        float timer = 0f;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            errorText.GetComponent<CanvasGroup>().alpha = 1;
            yield return null;
        }
        errorText.GetComponent<CanvasGroup>().alpha = 0;
    }
    void ScoreChecker()
    {
        int highScore = PlayerPrefs.GetInt("HighestScore", -1);
        if (Application.internetReachability != NetworkReachability.NotReachable && highScore != -1)
        {
            if (PlayFabManager.Instance != null)
            {
                PlayFabManager.Instance.SubmitScore(PlayerPrefs.GetInt("HighestScore"));
            }
        }
    }

    [Header("CreditPanel")]
    [SerializeField] private GameObject creditPanel;
    [SerializeField] private GameObject creditText;

    public void CreditPanelOpen()
    {
        if (creditPanel.activeSelf)
        {
            creditPanel.SetActive(false);
        }
        else
        {
            creditPanel.SetActive(true);
            StartCoroutine(CreditTextPanel());
        }
    }
    System.Collections.IEnumerator CreditTextPanel()
    {
        Vector3 startPos = new Vector3(creditText.transform.position.x, 0, 0);
        creditText.transform.position = startPos;
        Vector3 targetPos = new Vector3(creditText.transform.position.x, creditText.transform.position.y + 3000, creditText.transform.position.z);
        float timer = 0;
        float duration = 20f;
        while (timer < duration)
        {
            creditText.transform.position = Vector3.Lerp(startPos, targetPos, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        creditPanel.SetActive(false);
    }

    [Header("How to Play")]
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject first, second, third, fourth, fifth;
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;

    int howtoplayIndex = 0;

    public void HowToPlayPanelButton()
    {
        howtoplayIndex = 1;
        if (howToPlayPanel.activeSelf)
        {
            howToPlayPanel.SetActive(false);
            return;
        }
        howToPlayPanel.SetActive(true);
        first.SetActive(true);
        second.SetActive(false);
        third.SetActive(false);
        fourth.SetActive(false);
        fifth.SetActive(false);
        nextButton.SetActive(true);
        previousButton.SetActive(false);
    }
    public void HowToPlayNextButton()
    {
        nextButton.SetActive(true);
        previousButton.SetActive(true);
        howtoplayIndex++;
        if (howtoplayIndex >= 5)
        {
            howtoplayIndex = 5;
            nextButton.SetActive(false);
        }
        ShowHowToPlayPage(howtoplayIndex);

    }
    public void HowToPlayPreviousButton()
    {
        nextButton.SetActive(true);
        previousButton.SetActive(true);
        howtoplayIndex--;
        if (howtoplayIndex <= 1)
        {
            howtoplayIndex = 1;
            previousButton.SetActive(false);
        }
        ShowHowToPlayPage(howtoplayIndex);
    }
    void ShowHowToPlayPage(int i)
    {
        if (i == 1)
        {
            first.SetActive(true);
            second.SetActive(false);
            third.SetActive(false);
            fourth.SetActive(false);
            fifth.SetActive(false);
        }
        else if (i == 2)
        {
            first.SetActive(false);
            second.SetActive(true);
            third.SetActive(false);
            fourth.SetActive(false);
            fifth.SetActive(false);
        }
        else if (i == 3)
        {
            first.SetActive(false);
            second.SetActive(false);
            third.SetActive(true);
            fourth.SetActive(false);
            fifth.SetActive(false);
        }
        else if (i == 4)
        {
            first.SetActive(false);
            second.SetActive(false);
            third.SetActive(false);
            fourth.SetActive(true);
            fifth.SetActive(false);
        }
        else if (i == 5)
        {
            first.SetActive(false);
            second.SetActive(false);
            third.SetActive(false);
            fourth.SetActive(false);
            fifth.SetActive(true);
        }
    }

    [Header("Language")]
    [SerializeField] private GameObject trFlag;
    [SerializeField] private Locale english;
    [SerializeField] private Locale turkish;
    public void ChangeLanguage()
    {
        if (PlayerPrefs.GetString("Locale", english.Identifier.Code) == english.Identifier.Code)
        {
            LocalizationSettings.SelectedLocale = turkish;
            trFlag.SetActive(true);
            PlayerPrefs.SetString("Locale", turkish.Identifier.Code);
            PlayerPrefs.Save();
        }
        else if (PlayerPrefs.GetString("Locale", english.Identifier.Code) == turkish.Identifier.Code)
        {
            LocalizationSettings.SelectedLocale = english;
            trFlag.SetActive(false);
            PlayerPrefs.SetString("Locale", english.Identifier.Code);
            PlayerPrefs.Save();
        }
    }
    void CheckLanguage()
    {
        if (PlayerPrefs.GetString("Locale",english.Identifier.Code) == english.Identifier.Code)
        {
            LocalizationSettings.SelectedLocale = english;
            trFlag.SetActive(false);
        }
        else if (PlayerPrefs.GetString("Locale", english.Identifier.Code) == turkish.Identifier.Code)
        {
            LocalizationSettings.SelectedLocale = turkish;
            trFlag.SetActive(true);
        }
    }
    void FirstLanguageChecker()
    {
       if( LocationManager.Instance!=null && LocationManager.Instance.countryCode == "TR")
        {
            LocalizationSettings.SelectedLocale = turkish;
            trFlag.SetActive(true);
            PlayerPrefs.SetString("Locale", turkish.Identifier.Code);
            PlayerPrefs.Save();
        }
        else
        {
            if (PlayerPrefs.GetString("Locale", english.Identifier.Code) == turkish.Identifier.Code)
            {
                LocalizationSettings.SelectedLocale = turkish;
                trFlag.SetActive(false);
                PlayerPrefs.SetString("Locale", turkish.Identifier.Code);
                PlayerPrefs.Save();
            }
            else
            {
                LocalizationSettings.SelectedLocale = english;
                trFlag.SetActive(false);
                PlayerPrefs.SetString("Locale", english.Identifier.Code);
                PlayerPrefs.Save();
            }

        }
    }

}
