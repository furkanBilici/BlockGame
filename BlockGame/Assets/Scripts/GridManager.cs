using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int height = 8;
    [SerializeField] private int width = 8;
    
    [SerializeField] private GameObject gridPrefab;

    private Transform[,] logicGrid;
    private void Awake()
    {
        logicGrid = new Transform[width, height];//dizi
    }
    void Start()
    { 
        GenerateGrid();
    }
    void GenerateGrid()
    {
        if (gridPrefab == null) 
        {
            Debug.Log("prefab bulunamadý");
            return;
        }
        for (int x = 0; x < width; x++) 
        {
            for (int y = 0; y < height; y++) 
            {
                GameObject newBlock = Instantiate(gridPrefab, new Vector3(x, y,0), Quaternion.identity);//blok konuma oluþturuluyor
                newBlock.transform.parent = this.transform;//parent olarak manager verildi
                newBlock.name=$"Cell({x},{y})";//neseneye isim verme
            }
        }
    }

    public bool CanPlaceBlock(BlockData blockData, Vector2Int gridPosition)
    {
        foreach(Vector2Int cellOffset in blockData.cells)
        {
            Vector2Int pos=gridPosition + cellOffset;
            if(!IsWithinGrid(pos.x, pos.y))
            {
                return false;
            }
            if (IsCellOccupied(pos.x, pos.y)) 
            {
                return false;
            }  
        }
        return true;
    }

    public bool IsWithinGrid(int x, int y)
    {
        return (x>=0 && y>=0 && x<width && y<height);
    }
    public bool IsCellOccupied(int x, int y)
    {
        return logicGrid[x, y] != null;
    }
    public void PlaceBlock(GameObject blockObject, Vector2Int gridPosition)
    {
        BlockData blockData = blockObject.GetComponent<Block>().data;

        foreach (Vector2Int cellOffset in blockData.cells)
        {
            Vector2Int targetPos = gridPosition + cellOffset;

            foreach (Transform childCell in blockObject.transform)
            {
                if (Vector2.Distance(childCell.localPosition, (Vector2)cellOffset) < 0.01f)
                {
                    logicGrid[targetPos.x, targetPos.y] = childCell;
                    break; 
                }
            }
        }

        blockObject.transform.parent = this.transform;
        CheckForCompletedLines();
    }

    bool rowIsCompleted;
    bool colIsCompleted;
    void CheckForCompletedLines()
    {
        List<int> completedRows = new List<int>();
        List<int> completedCols = new List<int>();

        for (int y = 0; y < height; y++) 
        { 
            rowIsCompleted = true;
            for (int x = 0; x < width; x++)
            {
               
                if (logicGrid[x, y] == null)
                {
                    rowIsCompleted = false;
                    break;
                }
            }
            if (rowIsCompleted) 
            {
                completedRows.Add(y);
            }
        }
        for (int x = 0; x < width; x++)
        {
            colIsCompleted=true;
            for (int y = 0; y < height; y++)
            {
                
                if(logicGrid[x, y] == null)
                {
                    colIsCompleted = false;
                    break;
                }
            }
            if (colIsCompleted) 
            {
                completedCols.Add(x);            
            }
        }
        if (completedCols.Count > 0 || completedRows.Count > 0) 
        {
            int totalClearedRowsAndColumns=completedCols.Count+completedRows.Count;

            ScoreManager.Instance.AddScore(totalClearedRowsAndColumns);

            ClearLines(completedRows, completedCols);
        }
    }
    void ClearLines(List<int> rows, List<int> cols) 
    {
        foreach (int y in rows) 
        {
            for(int x = 0;x < width; x++)
            {
                if (logicGrid[x, y] != null)
                {
                    Destroy(logicGrid[x, y].gameObject);
                    logicGrid[x, y] = null;
                }
            }  
        }
        foreach (int x in cols)
        {
            for (int y = 0; y < height; y++)
            {
                if (logicGrid[x, y] != null)
                {
                    Destroy(logicGrid[x, y].gameObject);
                    logicGrid[x, y] = null;
                }
            }
        }
    }
    public bool IsAnyMovePossible(BlockData blockData)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++) 
            {
                if(CanPlaceBlock(blockData, new Vector2Int(x, y)))
                {
                    return true;
                }
            }
        }
        return false;
    }

}
