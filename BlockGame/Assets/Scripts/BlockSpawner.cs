using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Blok Verileri")]
    // Buraya Unity edit�r�nden t�m blok varyasyonlar�n� s�r�kleyece�iz.
    [SerializeField] private List<BlockData> allBlockData;

    [Header("Prefablar ve Konumlar")]
    // Her blo�un ana ta��y�c�s� olan bo� obje prefab'�.
    [SerializeField] private GameObject blockBasePrefab;
    // Bloklar�n olu�turulaca�� ana obje (hiyerar�iyi d�zenli tutmak i�in).
    [SerializeField] private Transform spawnParent;

    // Bloklar�n ekranda bekleyece�i pozisyonlar.
    [SerializeField] private List<Transform> spawnPositions;

    void Start()
    {
        SpawnInitialPoints();
    }
    void SpawnInitialPoints()
    {
        foreach (Transform t in spawnPositions) 
        {
            SpawnBlockAtPoint(t.position);
        }
    }

    void SpawnBlockAtPoint(Vector2 spawnPosition)
    {
        int randomIndex = Random.Range(0, allBlockData.Count);
        BlockData selectedData = allBlockData[randomIndex];

        GameObject block = Instantiate(blockBasePrefab, spawnPosition, Quaternion.identity, spawnParent);
        block.name = selectedData.name;        
        Block blockScript = block.AddComponent<Block>();
        blockScript.data = selectedData;

        foreach (Vector2 cell in selectedData.cells)
        {
            GameObject blockCell= Instantiate(selectedData.blockCellPrefab, block.transform);
            blockCell.transform.localPosition = (Vector2)cell;
        }
    }
}
