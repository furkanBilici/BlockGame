
using GoogleMobileAds.Api.AdManager;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject panel;

    public static UIManager Instance { get; private set; }
    public GameObject gameoverText;
    public GameObject continueButton;
    public TextMeshProUGUI highScore;
    public GameObject scoreTextEnd;
    public GameObject scoreText;

    public int GameType = 0;//0 classic, 1 timed, 2 custom

    public bool panelActive = false;
    

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }
    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllMusic();
            if (GameType == 0)
            {
                AudioManager.Instance.PlayMusic("GameMusic");
            }
            if (GameType == 1)
            {
                AudioManager.Instance.PlayMusic("TimedGameMusic");
            }
            if (GameType == 2)
            {
                AudioManager.Instance.PlayMusic("CustomGameMusic");
            }
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            StopGame();
        }
        if (watchAdButton != null && watchAdButton.gameObject.activeInHierarchy)
        {
            // Butonun týklanabilirliðini, reklamýn yüklenip yüklenmediðine göre ayarla.
            // Bu, oyuncunun hazýr olmayan bir reklama basmasýný engeller.
           if(AdsManager.Instance!=null) watchAdButton.interactable = AdsManager.Instance.IsRewardedAdLoaded;
        }
    }
    public void ShowGameOverPanel()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("GameOverSound");
            AudioManager.Instance.StopAllMusic();
        }
        panelActive = true;
        ScoreManager.Instance.CheckHighestScore();  
        gameoverText.SetActive(true);
        panel.SetActive(true);
        if (scoreTextEnd != null) 
        {
            scoreTextEnd.SetActive(true);
            scoreTextEnd.GetComponent<TextMeshProUGUI>().text = scoreText.GetComponent<TextMeshProUGUI>().text;
        }
        if (scoreText != null) 
        {
            scoreText.SetActive(false);
        }
        if (highScore != null) 
        {
            if(PlayerPrefs.GetString("Locale")=="en") highScore.text = "HIGHEST SCORE: " + PlayerPrefs.GetInt("HighestScore", 0);
            else highScore.text = "EN YÜKSEK SKOR: " + PlayerPrefs.GetInt("HighestScore", 0);

        }
        if (watchAdButton != null && AdsManager.Instance!=null)
        {
            watchAdButton.gameObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        panelActive=false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void MainMenu()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("OutGame");
            AudioManager.Instance.StopAllMusic();
        }

        panelActive = false;
        SceneManager.LoadScene(0);
    }
    public void StopGame()
    {
        if (!panelActive) {
            panelActive = true;
            continueButton.SetActive(true);
            panel.SetActive(true);
        }
        else
        {
            panelActive = false;
            continueButton.SetActive(false);
            panel.SetActive(false);
        }
        
    }
    public void ButtonClickSound()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
    }
    public void Continue()
    {
        panelActive = false;
        panel.SetActive(false);
        continueButton.SetActive(false);
    }

    [Header("Reklam Butonu")]
    [SerializeField] private Button watchAdButton;

    public void HideGameOverPanel()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("CustomGameMusic");
        panelActive=false;
        panel.SetActive(false);
        gameoverText.SetActive(false);
        if(scoreTextEnd!=null) scoreTextEnd.SetActive(false);
        scoreText.SetActive(true);
        if (watchAdButton != null && AdsManager.Instance != null)
        {
            watchAdButton.gameObject.SetActive(false);
        }
    }
    public void OnWatchAdButtonClicked()
    {
        // Butonu geçici olarak pasif yap ki tekrar tekrar basýlmasýn.
        watchAdButton.interactable = false;

        // Reklamý göstermesi için AdsManager'ý çaðýr.
        if(AdsManager.Instance!=null)AdsManager.Instance.ShowRewardedAd();
    }
    public void NextTimedLevel()
    {
        int neededScore=20;
        float time=25;
        int level = PlayerPrefs.GetInt("CompletedLevels", 0) + 1;
        PlayerPrefs.SetInt("TimedGameScore", (int)(neededScore * level * MathF.Sqrt(level)));
        PlayerPrefs.SetFloat("TimedGameTime", time * level);
        PlayerPrefs.Save();
        SceneManager.LoadScene(3);
    }
}