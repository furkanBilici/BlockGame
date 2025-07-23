// BlockSpawner.cs (TEMÝZLENMÝÞ VE NÝHAÝ VERSÝYON)

using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Block Data")]
    [SerializeField] public List<BlockData> allBlockData;

    [Header("Prefabs & Parents")]
    [SerializeField] public GameObject blockBasePrefab;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private List<Transform> spawnPositions;

    private List<GameObject> activeBlocks = new List<GameObject>();
    private GridManager gridManager;

    [Header("Oyun Dengesi")]
    [SerializeField] private int maxSpawnAttempts = 50;

    MaterialPropertyBlock mpbBlock;

    public static BlockSpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
        gridManager = FindFirstObjectByType<GridManager>();
    }

    void Start()
    {
        mpbBlock = new MaterialPropertyBlock();
        SpawnNewBlockSet();
    }

    public void SpawnNewBlockSet()
    {
        activeBlocks.Clear();
        foreach (Transform child in spawnParent)
        {
            Destroy(child.gameObject);
        }

        bool foundValidSet = false;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            List<BlockData> candidateData = new List<BlockData>();
            for (int i = 0; i < spawnPositions.Count; i++)
            {
                candidateData.Add(GetRandomBlockData());
            }

            bool isSetValid = false;
            foreach (BlockData data in candidateData)
            {
                if (gridManager.IsAnyMovePossible(data))
                {
                    isSetValid = true;
                    break;
                }
            }

            if (isSetValid)
            {
                for (int i = 0; i < spawnPositions.Count; i++)
                {
                    SpawnBlock(candidateData[i], spawnPositions[i].position);
                }
                foundValidSet = true;
                break;
            }
        }

        if (!foundValidSet)
        {
            Debug.LogError("GEÇERLÝ SET BULUNAMADI! OYUN BÝTÝYOR.");
            HandleGameOver();
        }
    }



    [Header("Colors")]
    public Color[] colors;
    Color randomColor;
    public Color GetRandomColor()
    {
        int random=Random.Range(0,colors.Length);
        return colors[random];
    }

    [Header("Materials")]
    public Material[] materials;
    Material randomMaterial;
    
    Material GetRandomMaterial()
    {
        int random = Random.Range(0, materials.Length);
        return materials[random];
    }
    private void SpawnBlock(BlockData data, Vector2 position)
    {
        if (data == null) return;

        GameObject blockObject = Instantiate(blockBasePrefab, position, Quaternion.identity, spawnParent);
        blockObject.name = data.name;

        Block blockScript = blockObject.GetComponent<Block>();
        blockScript.data = data;
        activeBlocks.Add(blockObject);
        
        // Renklendirme mantýðýný koruyoruz
        randomColor = GetRandomColor();
        blockScript.color = randomColor;
        mpbBlock.SetColor("_BaseColor",randomColor);
        foreach (Vector2Int cellPos in data.cells)
        {
            GameObject cell = Instantiate(data.blockCellPrefab, blockObject.transform);
            cell.transform.localPosition = (Vector2)cellPos;
            cell.GetComponent<Renderer>().SetPropertyBlock(mpbBlock);
        }
    }

    // ARTIK TEK BÝR SÝNYAL FONKSÝYONU VAR! Bu, GridManager'dan çaðrýlacak.
    public void OnActionFinished()
    {
        // Elimizde hala yerleþtirilecek blok var mý?
        if (activeBlocks.Count > 0)
        {
            // Varsa, kalan bloklarla oyun bitti mi diye kontrol et.
            if (IsGameOver())
            {
                HandleGameOver();
            }
        }
        else // Elimizde hiç blok kalmadýysa...
        {
            // Yeni bir set getir.
            SpawnNewBlockSet();
        }
    }

    private bool IsGameOver()
    {
        foreach (GameObject blockObject in activeBlocks)
        {
            if (blockObject == null) continue; // Güvenlik kontrolü
            BlockData data = blockObject.GetComponent<Block>().data;
            if (gridManager.IsAnyMovePossible(data))
            {
                return false;
            }
        }
        return true;
    }

    private void HandleGameOver()
    {
        UIManager.Instance.ShowGameOverPanel();
    }

    // YERLEÞEN BLOÐU LÝSTEDEN SÝLMEK ÝÇÝN YENÝ BÝR PUBLIC FONKSÝYON
    public void RemoveFromActiveBlocks(GameObject blockToRemove)
    {
        if (activeBlocks.Contains(blockToRemove))
        {
            activeBlocks.Remove(blockToRemove);
        }
    }

    public BlockData GetRandomBlockData()
    {
        if (allBlockData.Count == 0) return null;

        int randomIndex = Random.Range(0, allBlockData.Count);
        return allBlockData[randomIndex];
    }

    void OnEnable()
    {
        AdsManager.OnRewardEarned += ContinueGameAfterAd;//script açýldýðýnda bunu kontrol etmeli
    }

    void OnDisable()
    {
            AdsManager.OnRewardEarned -= ContinueGameAfterAd;//script kapatýldýðýnda dinlemeye devam etmesin, performans için
    }

    private void ContinueGameAfterAd()
    {
        Debug.Log("Reklam ödülü alýnýyor: Oyuna devam ediliyor.");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideGameOverPanel();
        }

        SpawnNewBlockSet();
    }
}