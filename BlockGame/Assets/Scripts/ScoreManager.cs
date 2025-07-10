using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Skor Ayarlar�")]
    [SerializeField] private int scorePerLine = 10;
    [SerializeField] private int comboBonusMultiplier = 5;

    [Header("UI Elemanlar�")]
    [SerializeField] private TextMeshProUGUI scoreText;

    private int currentScore=0;
    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance!= this)Destroy(gameObject);
        else Instance = this;
    }
    private void Start()
    {
        UpdateScoreUi();
    }

    public void AddScore(int completedLineCount)
    {
        int baseScore=completedLineCount*scorePerLine;//temel puan k�sm�

        int comboBonus = 0;
        if (completedLineCount > 1)
        {
            comboBonus = comboBonusMultiplier * (completedLineCount - 1);
        }
        currentScore += baseScore+comboBonus;
        UpdateScoreUi();

    }
    void UpdateScoreUi()
    {
        scoreText.text = "Score: " + currentScore;
    }

}
