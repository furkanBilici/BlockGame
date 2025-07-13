using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Skor Ayarlarý")]
    [SerializeField] private int scorePerLine = 10;
    [SerializeField] private int comboBonusMultiplier = 5;

    [Header("UI Elemanlarý")]
    [SerializeField] private TextMeshProUGUI scoreText;

    private int currentScore=0;
    private int highestScore = 0;
    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance!= this)Destroy(gameObject);
        else Instance = this;
    }
    private void Start()
    {
        highestScore=PlayerPrefs.GetInt("HighestScore", 0);
        UpdateScoreUi();
    }

    public void AddScore(int completedLineCount)
    {
        int baseScore=completedLineCount*scorePerLine;//temel puan kýsmý
        bool isCombo = false;
        int comboBonus = 0;
        if (completedLineCount > 1)
        {
            comboBonus = comboBonusMultiplier * (completedLineCount - 1);
            isCombo = true;
        }
        currentScore += baseScore+comboBonus;
        UpdateScoreUi();
        StartCoroutine(ScoreAnimationRoutine(isCombo));
    }

    private IEnumerator ScoreAnimationRoutine(bool isCombo)
    {
        // Animasyon parametreleri
        float duration = 0.2f; // Animasyonun toplam süresi
        Vector3 originalScale = Vector3.one;
        // Combo ise daha çok büyüsün, deðilse normal büyüsün.
        Vector3 targetScale = isCombo ? Vector3.one * 1.5f : Vector3.one * 1.2f;

        // Combo ise rengi de anlýk olarak deðiþtirelim.
        Color originalColor = scoreText.color;
        if (isCombo)
        {
            scoreText.color = Color.yellow; // Veya baþka parlak bir renk
        }

        // Büyüme Aþamasý
        float timer = 0f;
        while (timer < duration / 2)
        {
            scoreText.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / (duration / 2));
            timer += Time.deltaTime;
            yield return null;
        }
        scoreText.transform.localScale = targetScale;

        // Küçülme Aþamasý
        timer = 0f;
        while (timer < duration / 2)
        {
            scoreText.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / (duration / 2));
            timer += Time.deltaTime;
            yield return null;
        }
        scoreText.transform.localScale = originalScale;

        // Eðer rengi deðiþtirdiysek, orijinal rengine geri döndür.
        if (isCombo)
        {
            scoreText.color = originalColor;
        }
    }

    void UpdateScoreUi()
    {
        scoreText.text = "Score: " + currentScore;
    }
    public void CheckHighestScore()
    {
        if (highestScore < currentScore)
        {
            highestScore = currentScore;    
            PlayerPrefs.SetInt("HighestScore",highestScore);
            PlayerPrefs.Save(); 
        }
    }
}
