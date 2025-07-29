using JetBrains.Annotations;
using LootLocker.LootLockerEnums;
using LootLocker.Requests;
using System;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
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
            AudioManager.Instance.PlaySFX("ButtonClick");
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
        if (AdsManager.Instance != null) AdsManager.Instance.LoadBannerAd();
        if (AudioManager.Instance != null) AudioManager.Instance.StopAllMusic();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("MenuMusic");
        modePanelBool = modePanel.activeSelf;
        SoundButtonControl();
        MusicButtonControl();
        HighScore.SetActive(true);
        if (PlayerPrefs.GetInt("HighestScore", 0) > 0) HighScore.GetComponent<TextMeshProUGUI>().text = "HIGH SCORE: " + PlayerPrefs.GetInt("HighestScore", 0);
        
        boardScale = PlayerPrefs.GetInt("boardScale", 1);
        difficulty = PlayerPrefs.GetInt("difficulty", 0);
        ButtonColorTextControllerForCustomGame(boardScale, sizeButton);
        ButtonColorTextControllerForCustomGame(difficulty, difficultyButton);
        if(LootLockerManager.Instance!=null) CheckForPlayerName();
        
    }
    public void StartGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
        SceneManager.LoadScene(2);
    }
    public void QuitGame()//oyundan çýkýþ butonu (2 butonda kullanýlacak)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("OutGame");
        Application.Quit();
    }
    public void BackToMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
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
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
        settingPanel.SetActive(true);
    }
    public void CloseSound()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
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
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
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
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
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
            level.GetComponentInChildren<TextMeshProUGUI>().text="LEVEL "+(PlayerPrefs.GetInt("CompletedLevels",0)+1);
        }
    }
    public void StartTimedGame()
    {
        int level = PlayerPrefs.GetInt("CompletedLevels", 0) + 1;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
        PlayerPrefs.SetInt("TimedGameScore", neededScore * level* (int)MathF.Sqrt(level));
        PlayerPrefs.SetFloat("TimedGameTime", time * level);
        SceneManager.LoadScene(3);
    }

    
    [Header("CustomMode")]
    [SerializeField] int boardScale = 0;//0=7x7,1=8x8,2=9x9
    [SerializeField] int difficulty = 0;//0=easy, 1 normal, 2=hard
    public void CustomModePanel()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
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
        if(boardScale >= 2)
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
            button.image.color = Color.red;
            if (button.name == "hardness") button.GetComponentInChildren<TextMeshProUGUI>().text = "HARD";
            else button.GetComponentInChildren<TextMeshProUGUI>().text = "9x9";
                
        }
        else
        {
            button.image.color = Color.green;
            if(button.name=="hardness") button.GetComponentInChildren<TextMeshProUGUI>().text = "EASY";
            else button.GetComponentInChildren<TextMeshProUGUI>().text = "7x7";
            
        }
    }
    public void PlayCustomMode()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
        SceneManager.LoadScene(4);
    }





    [Header("HighScoreBoard")]
    public GameObject HighscoreBoardPanel;
    public Transform scoreContentParent;
    public GameObject scoreEntryPrefab;
    public GameObject noConnectionPanel;

    public TextMeshProUGUI highscoreTitle;

    private const string LEADERBOARD_KEY = "global_high_scores";
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
        if ((HighscoreBoardPanel.activeSelf || noConnectionPanel.activeSelf) && !(listType==1 || listType==2|| listType==0))
        {
            HighscoreBoardPanel.SetActive(false);
            noConnectionPanel.SetActive(false);
            return;
        }
       

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("internet yok");
            noConnectionPanel.SetActive(true);
            return;
        }
        
       
        HighscoreBoardPanel.SetActive(true);
        foreach (Transform child in scoreContentParent.transform) Destroy(child.gameObject);

        string title = "HIGHEST SCORES ";

        switch (listType)
        {
            case 0: // GLOBAL
                title += "GLOBAL";
                break;
            case 1: // ÜLKE
                title += "MY COUNTRY";// Ülke kodunu PlayerPrefs'ten al.
                break;
            case 2: // ÞEHÝR
                title += "MY CITY";
                break; // Fonksiyonu bitir.
        }
        highscoreTitle.text = title;

        LootLockerSDKManager.GetScoreList(LEADERBOARD_KEY, 1000,(response) =>
        {
            if (response.success)
            {
                LootLockerLeaderboardMember[] scores = response.items;
                string metadata;
                for(int i = 0; i < scores.Length; i++)
                {
                    metadata = scores[i].metadata;
                    if(listType == 0)
                    {
                        GameObject entry = Instantiate(scoreEntryPrefab, scoreContentParent);
                        TextMeshProUGUI rankText = entry.transform.Find("RankText").GetComponent<TextMeshProUGUI>();
                        TextMeshProUGUI nameText = entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
                        TextMeshProUGUI scoreText = entry.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();
                        rankText.text = scores[i].rank + ".";
                        nameText.text = scores[i].player.name;
                        scoreText.text = scores[i].score.ToString();
                    }
                    if (listType==1 && metadata.Contains(PlayerPrefs.GetString("CountryCode")))
                    {
                        GameObject entry = Instantiate(scoreEntryPrefab, scoreContentParent);
                        TextMeshProUGUI rankText = entry.transform.Find("RankText").GetComponent<TextMeshProUGUI>();
                        TextMeshProUGUI nameText = entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
                        TextMeshProUGUI scoreText = entry.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();
                        rankText.text = (i + 1) + ".";
                        nameText.text = scores[i].player.name;
                        scoreText.text = scores[i].score.ToString();
                    }
                    if (listType == 2 && metadata.Contains(PlayerPrefs.GetString("City")))
                    {
                        
                        GameObject entry = Instantiate(scoreEntryPrefab, scoreContentParent);
                        TextMeshProUGUI rankText = entry.transform.Find("RankText").GetComponent<TextMeshProUGUI>();
                        TextMeshProUGUI nameText = entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
                        TextMeshProUGUI scoreText = entry.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();
                        rankText.text = (i+1) + ".";
                        nameText.text = scores[i].player.name;
                        scoreText.text = scores[i].score.ToString();
                    }

                }
            }
            
            else
            {
                Debug.Log("liste getirilemedi");
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
    void CheckForPlayerName()
    {
        string playerName=PlayerPrefs.GetString(PLAYER_NAME_KEY);
        if (String.IsNullOrEmpty(playerName) && Application.internetReachability!=NetworkReachability.NotReachable)
        {
            setNamePanel.SetActive(true);
        }
        if(!String.IsNullOrEmpty(playerName))
        {  
            LootLockerManager.Instance.CheckIfPlayerExists(playerName, (nameExists) =>
            {
                if (nameExists)
                {
                    welcomText.text = "WELCOME: " + playerName;
                }
                else
                {
                    LootLockerManager.Instance.SetPlayerName(playerName);
                    welcomText.text = "WELCOME: " + playerName;
                }
            });
            ScoreChecker();
        }
    }
    public void ConfirmPlayerName() 
    {
        string playerName = nameField.text;

        if (string.IsNullOrWhiteSpace(playerName) || playerName.Length < 3)
        {
            StartCoroutine(ShowErrorText());
            Debug.Log("bu isim olmaz");
            return;
        }

        confirmNameButton.interactable = false;
        statusText.color = Color.white;
        statusText.text = "Checking...";

        LootLockerManager.Instance.CheckIfPlayerExists(playerName, (nameExists) =>
        {
            confirmNameButton.interactable = true;
            if (nameExists) 
            {
                statusText.text = "This username is used by another player";
                statusText.color = Color.red;
            }
            else 
            {
                statusText.text = "Name saved!"; 
                LootLockerManager.Instance.SetPlayerName(playerName);
                setNamePanel.SetActive(false);
                welcomText.text = "WELCOME: " + playerName;
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
            timer+= Time.deltaTime;
            errorText.GetComponent<CanvasGroup>().alpha = 1;
            yield return null;
        }
        errorText.GetComponent<CanvasGroup>().alpha = 0;
    }
    void ScoreChecker()
    {
        int highScore = PlayerPrefs.GetInt("HighestScore",-1);
        if (Application.internetReachability != NetworkReachability.NotReachable && highScore!=-1)
        {
            if (LootLockerManager.Instance != null)
            {
                LootLockerManager.Instance.SubmitScore(PlayerPrefs.GetInt("HighestScore"));
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
        Vector3 targetPos = new Vector3(creditText.transform.position.x,creditText.transform.position.y+3000,creditText.transform.position.z);
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
    [SerializeField ]private GameObject previousButton;
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
}
