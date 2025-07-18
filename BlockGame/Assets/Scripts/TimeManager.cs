using System.Collections;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public int point = 120;
    public float startTime = 30f;
    float timeSecond;
    public TextMeshProUGUI time;
    public GameObject startScreen;
    public GameObject winScreen;
    public TextMeshProUGUI startText;
    public TextMeshProUGUI goalText;
    public int completedLevels=0;

    private void Start()
    {
        completedLevels = PlayerPrefs.GetInt("CompletedLevels", 0);
        point = PlayerPrefs.GetInt("TimedGameScore", 20);
        startTime = PlayerPrefs.GetFloat("TimedGameTime", 20);
        startText.text = point+" POINTS";
        timeSecond=startTime;
        time.text = "REMAINED: " + timeSecond+" S";
        StartCoroutine(StartAnim());
        goalText.text = "GOAL:" + point;
    }

    private void FixedUpdate()
    {
        if (timeSecond <= 0 || ScoreManager.Instance.GetCurrentScore()>=point )
        {
            FinishGame();
        }
        else
        {
            if (!UIManager.Instance.panelActive)
            {
                if (timeSecond <= 4)
                {
                    time.color = Color.red;
                }
                time.text = "REMAINED: " + Mathf.RoundToInt(timeSecond) + " S";
                timeSecond -= 0.02f;
            }
        }
    }
    void FinishGame()
    {
        if (ScoreManager.Instance.GetCurrentScore() >= point) 
        {
            if (AudioManager.Instance != null && (!UIManager.Instance.panelActive))
            {
                AudioManager.Instance.PlaySFX("WinSound");
                AudioManager.Instance.StopAllMusic();
            }
            if (completedLevels <= 3 && !winScreen.activeSelf) 
            {
                completedLevels++;
                PlayerPrefs.SetInt("CompletedLevels", completedLevels);
                PlayerPrefs.Save();
            }
            
            winScreen.SetActive(true);
            UIManager.Instance.panelActive = true;  
        }
        else
        {
            if(!UIManager.Instance.panelActive)
            UIManager.Instance.ShowGameOverPanel();
        }

    }
    IEnumerator StartAnim()
    {
        // Hedef boyutlar
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 1.05f; // yüzde 5 daha büyük
        float duration = 0.3f; // animasyon süresi

        // büyüme
        float timer = 0f;
        while (timer < duration)
        {
            startScreen.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        startScreen.transform.localScale = targetScale;

        // küçülme
        timer = 0f;
        while (timer < duration)
        {
            startScreen.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        startScreen.transform.localScale = originalScale;

        timer = 0;
        float ogAlpha = 1;
        float targetAlpha=0;
        while(timer < duration)
        {
            startScreen.GetComponent<CanvasGroup>().alpha=Mathf.Lerp(ogAlpha,targetAlpha,timer/duration);
            timer += Time.deltaTime;
            yield return null;
        }
        startScreen.SetActive(false);
    }
}
