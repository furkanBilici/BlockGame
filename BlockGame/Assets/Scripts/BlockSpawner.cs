// BlockSpawner.cs (D�zeltilmi� Versiyon)

using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Block Data")]
    [SerializeField] private List<BlockData> allBlockData;

    [Header("Prefabs & Parents")]
    [SerializeField] private GameObject blockBasePrefab;
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

    // Bu fonksiyon yeni bir 3'l� set olu�turur.
    public void SpawnNewBlockSet()
    {
        // �nce eski referanslar� temizle
        activeBlocks.Clear();

        // Hiyerar�ide kalan eski blok objelerini de temizle (e�er varsa)
        foreach (Transform child in spawnParent)
        {
            Destroy(child.gameObject);
        }

        // Her bir bekleme pozisyonu i�in yeni bir blok olu�tur.
        foreach (Transform pos in spawnPositions)
        {
            SpawnRandomBlockAt(pos.position);
        }

        // Yeni set geldikten hemen sonra oyun sonu kontrol� yap.
        if (IsGameOver())
        {
            HandleGameOver();
        }
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

        foreach (Vector2Int cellPos in selectedData.cells)
        {
            GameObject cell = Instantiate(selectedData.blockCellPrefab, blockObject.transform);
            cell.transform.localPosition = (Vector2)cellPos;
        }
    }

    public void OnBlockPlaced(GameObject placedBlock)
    {
        if (activeBlocks.Contains(placedBlock))
        {
            activeBlocks.Remove(placedBlock);
        }

        if (activeBlocks.Count > 0)
        {
            if (IsGameOver())
            {
                HandleGameOver();
            }
        }
        else
        {
            Invoke(nameof(SpawnNewBlockSet), 0.1f);
        }
    }

    private bool IsGameOver()
    {
        foreach (GameObject blockObject in activeBlocks)
        {
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
}