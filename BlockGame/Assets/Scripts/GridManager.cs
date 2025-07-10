using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int height = 8;
    [SerializeField] private int width = 8;
    
    [SerializeField] private GameObject gridPrefab;

    private Transform[,] logicGrid;
    void Start()
    {
        logicGrid = new Transform[width, height];//dizi
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
    public void PlaceBlock(BlockData blockData, Vector2Int gridPos)
    {
        foreach(Vector2Int offset in blockData.cells)
        {
            Vector2Int pos = gridPos + offset;
            logicGrid[pos.x, pos.y] = transform;
        }
    }
   
}
