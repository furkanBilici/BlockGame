
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    public static UIManager Instance { get; private set; }
    public GameObject gameoverText;
    public GameObject continueButton;
    public TextMeshProUGUI highScore;
    public GameObject scoreTextEnd;
    public GameObject scoreText;
    

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public void ShowGameOverPanel()
    {
        ScoreManager.Instance.CheckHighestScore();  
        gameoverText.SetActive(true);
        panel.SetActive(true);
        scoreTextEnd.SetActive(true);
        scoreTextEnd.GetComponent<TextMeshProUGUI>().text = scoreText.GetComponent<TextMeshProUGUI>().text;
        scoreText.SetActive(false);          
        highScore.text = "HIGHEST SCORE: " + PlayerPrefs.GetInt("HighestScore", 0);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void StopGame()
    {
        continueButton.SetActive(true);
        panel.SetActive(true);
    }
    public void Continue()
    {
        panel.SetActive(false);
        continueButton.SetActive(false);
    }
}