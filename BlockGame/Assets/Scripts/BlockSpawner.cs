using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Blok Verileri")]
    // Buraya Unity editöründen tüm blok varyasyonlarýný sürükleyeceðiz.
    [SerializeField] private List<BlockData> allBlockData;

    [Header("Prefablar ve Konumlar")]
    // Her bloðun ana taþýyýcýsý olan boþ obje prefab'ý.
    [SerializeField] private GameObject blockBasePrefab;
    // Bloklarýn oluþturulacaðý ana obje (hiyerarþiyi düzenli tutmak için).
    [SerializeField] private Transform spawnParent;

    // Bloklarýn ekranda bekleyeceði pozisyonlar.
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
