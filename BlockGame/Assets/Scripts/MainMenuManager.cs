using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject HighScore;
    public GameObject quitPanel;

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
    }
    private void Start()
    {
        HighScore.SetActive(true);
        if(PlayerPrefs.GetInt("HighestScore",0)>0) HighScore.GetComponent<TextMeshProUGUI>().text = "HIGH SCORE: " + PlayerPrefs.GetInt("HighestScore", 0);
    }
    public void StartGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
        SceneManager.LoadScene(1);
    }
    public void QuitGame()//oyundan çýkýþ butonu (2 butonda kullanýlacak)
    {
        if(AudioManager.Instance!=null)AudioManager.Instance.PlaySFX("OutGame");
        Application.Quit();
    }
    public void BackToMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
        quitPanel.SetActive(false);
    }
}
