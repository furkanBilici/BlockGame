using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int height = 8;
    [SerializeField] private int width = 8;
    
    [SerializeField] private GameObject gridPrefab;

    private Transform[,] logicGrid;
    void Start()
    {
        //transform.position = new Vector3( transform.position.x-width, transform.position.y-height, transform.position.z);
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

   
}
