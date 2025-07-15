// BlockSpawner.cs (TEM�ZLENM�� VE N�HA� VERS�YON)

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

    public static BlockSpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
        gridManager = FindFirstObjectByType<GridManager>();
    }

    void Start()
    {
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
            Debug.LogError("GE�ERL� SET BULUNAMADI! OYUN B�T�YOR.");
            HandleGameOver();
        }
    }



    [Header("Colors")]
    public Color[] colors;
    Color randomColor;
    Color GetRandomColor()
    {
        int random=Random.Range(0,colors.Length);
        return colors[random];
    }
    private void SpawnBlock(BlockData data, Vector2 position)
    {
        if (data == null) return;

        GameObject blockObject = Instantiate(blockBasePrefab, position, Quaternion.identity, spawnParent);
        blockObject.name = data.name;

        Block blockScript = blockObject.AddComponent<Block>();
        blockScript.data = data;
        activeBlocks.Add(blockObject);

        // Renklendirme mant���n� koruyoruz
        randomColor = GetRandomColor();
        foreach (Vector2Int cellPos in data.cells)
        {
            GameObject cell = Instantiate(data.blockCellPrefab, blockObject.transform);
            cell.transform.localPosition = (Vector2)cellPos;
            cell.GetComponent<SpriteRenderer>().color = randomColor;
        }
    }

    // ARTIK TEK B�R S�NYAL FONKS�YONU VAR! Bu, GridManager'dan �a�r�lacak.
    public void OnActionFinished()
    {
        // Elimizde hala yerle�tirilecek blok var m�?
        if (activeBlocks.Count > 0)
        {
            // Varsa, kalan bloklarla oyun bitti mi diye kontrol et.
            if (IsGameOver())
            {
                HandleGameOver();
            }
        }
        else // Elimizde hi� blok kalmad�ysa...
        {
            // Yeni bir set getir.
            SpawnNewBlockSet();
        }
    }

    private bool IsGameOver()
    {
        foreach (GameObject blockObject in activeBlocks)
        {
            if (blockObject == null) continue; // G�venlik kontrol�
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

    // YERLE�EN BLO�U L�STEDEN S�LMEK ���N YEN� B�R PUBLIC FONKS�YON
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
    

}