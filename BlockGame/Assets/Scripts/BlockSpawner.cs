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

        foreach (Transform pos in spawnPositions)
        {
            SpawnRandomBlockAt(pos.position);
        }

        if (IsGameOver())
        {
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
    private void SpawnRandomBlockAt(Vector2 position)
    {
        if (allBlockData.Count == 0) return;
        int randomIndex = Random.Range(0, allBlockData.Count);
        BlockData selectedData = allBlockData[randomIndex];
        GameObject blockObject = Instantiate(blockBasePrefab, position, Quaternion.identity, spawnParent);
        blockObject.name = selectedData.name;
        Block blockScript = blockObject.AddComponent<Block>();
        blockScript.data = selectedData;
        activeBlocks.Add(blockObject);
        randomColor = GetRandomColor();
        foreach (Vector2Int cellPos in selectedData.cells)
        {
            GameObject cell = Instantiate(selectedData.blockCellPrefab, blockObject.transform);
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