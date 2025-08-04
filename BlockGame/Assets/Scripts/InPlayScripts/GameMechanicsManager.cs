using System.Collections;
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

    float initialFillPercentace = 0.25f;
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


        if (UIManager.Instance.GameType == 1)
        {
            initialFillPercentace = initialFillPercentace * PlayerPrefs.GetInt("CompletedLevels",0)/3;
            if (initialFillPercentace >= 0.5) initialFillPercentace = 0.5f;
        }
        if(UIManager.Instance.GameType == 2)
        {
            initialFillPercentace = initialFillPercentace * PlayerPrefs.GetInt("difficulty", 0);
        }
        gridManager.GenerateInitialBlocks(initialFillPercentace, maxPlacementTries);

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
        if (!spawnPosition.HasValue)
        {
            return;
        }
        else
        {
            StartCoroutine(SuprizeBlockSpawner(spawnPosition));
        }
    }
    IEnumerator SuprizeBlockSpawner(Vector2Int? spawnPosition)
    {
        GameObject suprizeBlock = Instantiate(suprizeBlockPrefab, (Vector2)spawnPosition.Value, Quaternion.identity, gridManager.transform);
        suprizeBlock.name = "SurpriseBox";
        gridManager.OccupyCell(spawnPosition.Value, suprizeBlock.transform,suprizeBlock.name);

        float animationDuration = 0.2f;
        float time = 0;
        Vector3 firstScale = new Vector3(0.1f, 0.1f, 0.1f);
        Vector3 lastScale = new Vector3(1, 1, 1);
      
        suprizeBlock.transform.localScale = firstScale;
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            suprizeBlock.transform.localScale = Vector3.Lerp(firstScale, lastScale, time / animationDuration);
            yield return null;
        }
        suprizeBlock.transform.localScale = lastScale;
    }
    [SerializeField]GameObject goodLuckObject;
    [SerializeField] GameObject badLuckObject;
    public void OnTriggerSurprise(Vector3 position)
    {
        if (Random.value > 0.5f)
        {
            
            if (scoreManager != null) scoreManager.AddScore(3);
           StartCoroutine( LuckEffect(true, position));
        }
        else
        {
          
            SpawnSurpriseBox();
            StartCoroutine(LuckEffect(false, position));
        }
    }
    IEnumerator LuckEffect(bool good, Vector3 pos)
    {
        Vector3 offset = new Vector3(0, 0, -2f);
        Vector3 positionAdder = new Vector3(0, 0.02f, 0);
        GameObject luckPrefab;
        if (!good)
        {
            luckPrefab = badLuckObject; 
            Debug.Log("KÖTÜ SÜRPRÝZ! Tahtaya bir taþ yerleþtirildi!");
        }
        else
        {
            Debug.Log("ÝYÝ SÜRPRÝZ! Bonus Puan!");
            luckPrefab = goodLuckObject;
        }
        pos += offset;
        GameObject luckObject= Instantiate(luckPrefab,pos,Quaternion.identity);
        float startAlpha=0f;
        SpriteRenderer sr = luckObject.GetComponent<SpriteRenderer>();
        Color color =sr.color;
        color.a = startAlpha;
        float timer = 0;
        float duration = 0.5f;
        while (timer < duration)
        {
            timer+= Time.deltaTime*1.5f;
            startAlpha = Mathf.Lerp(0, 1, timer / duration);
            luckObject.transform.position += positionAdder;
            color.a = startAlpha;
            sr.color=color;
            yield return null;
        }
        timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime/1.5f;
            startAlpha = Mathf.Lerp(1, 0, timer / duration);
            luckObject.transform.position += positionAdder;
            color.a = startAlpha;
            sr.color = color;
            yield return null;
        }
        Destroy(luckObject);    
    }
}
