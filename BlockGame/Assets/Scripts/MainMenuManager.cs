using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject HighScore;
    private void Start()
    {
        HighScore.SetActive(true);
        if(PlayerPrefs.GetInt("HighestScore",0)>0) HighScore.GetComponent<TextMeshProUGUI>().text = "HIGH SCORE: " + PlayerPrefs.GetInt("HighestScore", 0);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
