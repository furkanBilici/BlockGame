using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockRotateData : MonoBehaviour
{
    public static BlockRotateData Instance {  get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;    
    }
    public List<Vector2Int> RotateBlockDataCells(List<Vector2Int> blockData)
    {
        List<Vector2Int> blockList = new List<Vector2Int>();
        Vector2Int newCell;
        foreach (Vector2Int cell in blockData) 
        {
            newCell = CheckBlockData(cell);
            blockList.Add(newCell);
        }
        return blockList;
    }
    Vector2Int CheckBlockData(Vector2Int cell)
    {
        Vector2Int cell2= new Vector2Int(cell.y,-cell.x);
        return cell2;
    }

}
