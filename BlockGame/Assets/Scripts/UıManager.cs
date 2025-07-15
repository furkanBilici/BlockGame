
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
        if(highScore!=null) highScore.text = "HIGHEST SCORE: " + PlayerPrefs.GetInt("HighestScore", 0);
    }

    public void RestartGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
        panelActive=false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void MainMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("OutGame");
        panelActive = false;
        SceneManager.LoadScene(0);
    }
    public void StopGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
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
    public void Continue()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
        panelActive = false;
        panel.SetActive(false);
        continueButton.SetActive(false);
    }
}