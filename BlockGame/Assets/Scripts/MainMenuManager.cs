using TMPro;
using UnityEngine;
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
        if (AudioManager.Instance != null) AudioManager.Instance.StopAllMusic();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("MenuMusic");
        modePanelBool=modePanel.activeSelf;
        SoundButtonControl();
        MusicButtonControl();
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
        AudioManager.Instance.closeSounds=!AudioManager.Instance.closeSounds;
        SoundButtonControl();
    }
    void SoundButtonControl()
    {
        if (!AudioManager.Instance.closeSounds)
        {
            sound.GetComponentInChildren<TextMeshProUGUI>().text = "ON";
            sound.image.color = Color.green;
        }
        else
        {
            sound.GetComponentInChildren<TextMeshProUGUI>().text = "OFF";
            sound.image.color = Color.red;
        }
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
            music.GetComponentInChildren<TextMeshProUGUI>().text = "ON";
            if (AudioManager.Instance != null) { AudioManager.Instance.PlayMusic("MenuMusic"); }
            music.image.color = Color.green;
        }
        else
        {
            music.GetComponentInChildren<TextMeshProUGUI>().text = "OFF";
            music.image.color = Color.red;
        }
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
    public void StartTimedGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ButtonClick");
        SceneManager.LoadScene(2);
    }

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
}
