
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


    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public void ShowGameOverPanel()
    {
        gameoverText.SetActive(true);
        panel.SetActive(true);
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