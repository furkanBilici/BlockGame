using UnityEngine;

public class GameMechanicsManager : MonoBehaviour
{
    public static GameMechanicsManager Instance {  get; private set; }
    [Header("SuprizeBox")]
    [SerializeField] private GameObject suprizeBlockPrefab;
    [SerializeField] private int placedBlockUntilSuprize = 5;
    int countSuprizeBlock=0;

    private GridManager gridManager;
    private ScoreManager scoreManager;

    float initialFillPercentace = 0.1f;
    int maxPlacementTries=50;
    private void Awake()
    {
        if(Instance != null&& Instance!=this) {Destroy(gameObject); return; }
        Instance = this;
        
    }
    private void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        gridManager.GenerateInitialBlocks(initialFillPercentace,maxPlacementTries);
    }
    public void OnBlockPlaced()
    {
        countSuprizeBlock++;
        if (countSuprizeBlock == placedBlockUntilSuprize)
        {
            SpawnSurpriseBox();
            countSuprizeBlock = 0;
        }
    }
    void SpawnSurpriseBox()
    {
        if (gridManager == null) return;
        Vector2Int? spawnPosition = gridManager.GetRandomEmptyCell();
        if (spawnPosition.HasValue)
        {
            GameObject suprizeBlock = Instantiate(suprizeBlockPrefab, (Vector2)spawnPosition.Value, Quaternion.identity, gridManager.transform);
            suprizeBlock.name = "SuprizeBlock";
            gridManager.OccupyCell(spawnPosition.Value,suprizeBlock.transform);
        }
    }
    public void OnTriggerSurprise()
    {
        if (Random.value > 0.5f)
        {
            Debug.Log("ÝYÝ SÜRPRÝZ! +50 Bonus Puan!");
            if (scoreManager != null) scoreManager.AddScore(50); // AddBonusScore yerine AddScore kullanalým
        }
        else
        {
            Debug.Log("KÖTÜ SÜRPRÝZ! Tahtaya bir taþ yerleþtirildi!");
            SpawnSurpriseBox();
        }
    }
}
