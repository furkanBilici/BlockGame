using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Ayarlarý")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;

    [Header("Prefab")]
    [SerializeField] private GameObject gridCellPrefab; // Ýsmi düzeltildi

    private Transform[,] logicGrid;
    private bool isClearing = false;

    private SpriteRenderer[,] visualGridCells;
    Color visualGridCellsColor;
    int firstSortingOrder=0;
    int lastSortingOrder=5;   

    private void Awake()
    {
        logicGrid = new Transform[width, height];
        visualGridCells = new SpriteRenderer[width, height];
        
    }

    void Start()
    {
        GenerateGrid();
        GenerateInitialBlocks();
    }

    void GenerateGrid()
    {
        if (gridCellPrefab == null)
        {
            Debug.LogError("Grid Cell Prefab'ý GridManager'a atanmamýþ!");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newCell = Instantiate(gridCellPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);
                newCell.name = $"Cell({x},{y})";
                visualGridCells[x, y] = newCell.GetComponent<SpriteRenderer>();
                
            }
        }
        visualGridCellsColor = visualGridCells[0, 0].color;
        firstSortingOrder=visualGridCells[0, 0].sortingOrder;   
    }

    Color placedBlockColor;

    public void PlaceBlock(GameObject blockObject, Vector2Int gridPosition)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("PutBlock");
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
        placedBlockColor=blockObject.GetComponentInChildren<SpriteRenderer>().color;  
        blockObject.transform.parent = this.transform;

        bool linesWereCleared = CheckForCompletedLines();

        if (!linesWereCleared)
        {
            BlockSpawner.Instance.OnActionFinished();
        }
    }

    private bool CheckForCompletedLines()
    {
        List<int> completedRows = new List<int>();
        for (int y = 0; y < height; y++)
        {
            bool rowIsComplete = true;
            for (int x = 0; x < width; x++) { if (logicGrid[x, y] == null) { rowIsComplete = false; break; } }
            if (rowIsComplete) { completedRows.Add(y); }
        }

        List<int> completedCols = new List<int>();
        for (int x = 0; x < width; x++)
        {
            bool colIsComplete = true;
            for (int y = 0; y < height; y++) { if (logicGrid[x, y] == null) { colIsComplete = false; break; } }
            if (colIsComplete) { completedCols.Add(x); }
        }

        if (completedRows.Count > 0 || completedCols.Count > 0)
        {
            int totalLines = completedRows.Count + completedCols.Count;
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(totalLines);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("CleanLineSound");
            StartCoroutine(ClearLinesRoutine(completedRows, completedCols));

            return true;
        }

        return false;
    }

    private IEnumerator ClearLinesRoutine(List<int> rows, List<int> cols)
    {
        isClearing = true;
        List<Transform> cellsToClear = new List<Transform>();
        float duration = 0.4f; // Animasyon süresini biraz artýralým

        // Temizlenecek tüm hücreleri tek bir listede topla
        foreach (int y in rows)
        {
            for (int x = 0; x < width; x++)
            {
                if (logicGrid[x, y] != null && !cellsToClear.Contains(logicGrid[x, y]))
                {
                    cellsToClear.Add(logicGrid[x, y]);
                }
            }
        }
        foreach (int x in cols)
        {
            for (int y = 0; y < height; y++)
            {
                if (logicGrid[x, y] != null && !cellsToClear.Contains(logicGrid[x, y]))
                {
                    cellsToClear.Add(logicGrid[x, y]);
                }
            }
        }

        // Animasyonu uygula
        if (cellsToClear.Count > 0)
        {
            // Önce hepsini parlatalým
            foreach (Transform cell in cellsToClear)
            {
                if (cell != null) cell.GetComponent<SpriteRenderer>().color = placedBlockColor;
            }

            yield return new WaitForSeconds(duration / 2);

            // Þimdi hepsini küçülterek yok edelim
            float timer = 0f;
            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.zero;

            while (timer < duration / 2)
            {
                foreach (Transform cell in cellsToClear)
                {
                    if (cell != null) cell.localScale = Vector3.Lerp(startScale, endScale, timer / (duration / 2));
                }
                timer += Time.deltaTime;
                yield return null;
            }

            // Animasyon bitti, objeleri yok et ve mantýksal grid'i temizle
            foreach (Transform cell in cellsToClear)
            {
                if (cell != null)
                {
                    bool found = false;
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (logicGrid[x, y] == cell)
                            {
                                logicGrid[x, y] = null;
                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                    }
                    Destroy(cell.gameObject);
                }
            }
        }
        
        isClearing = false;
        if (BlockSpawner.Instance != null) BlockSpawner.Instance.OnActionFinished();

    }

    public bool CanPlaceBlock(BlockData blockData, Vector2Int gridPosition)
    {
        if (isClearing) return false;

        foreach (Vector2Int cellOffset in blockData.cells)
        {
            Vector2Int pos = gridPosition + cellOffset;
            if (!IsWithinGrid(pos.x, pos.y) || IsCellOccupied(pos.x, pos.y))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsWithinGrid(int x, int y) { return (x >= 0 && x < width && y >= 0 && y < height); }
    public bool IsCellOccupied(int x, int y) { return logicGrid[x, y] != null; }
    public bool IsAnyMovePossible(BlockData blockData)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (CanPlaceBlock(blockData, new Vector2Int(x, y)))
                {
                    return true;
                }
            }
        }
        return false;
    }
    

    public void HighlightLines(List<int> rows, List<int> cols, Color highlightColor)
    {
        // Önce tüm grid'i normale döndür
        ResetGridColors();

        if (rows != null)
        {
            foreach (int y in rows)
            {
                for (int x = 0; x < width; x++)
                {
                    visualGridCells[x, y].color = highlightColor;
                    visualGridCells[x, y].sortingOrder = lastSortingOrder;
                }
            }
        }
        if (cols != null)
        {
            foreach (int x in cols)
            {
                for (int y = 0; y < height; y++)
                {
                    visualGridCells[x, y].color = highlightColor;
                    visualGridCells[x, y].sortingOrder = lastSortingOrder;
                }
            }
        }
    }
    
    public void ResetGridColors()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                visualGridCells[x, y].color = visualGridCellsColor; // Veya baþlangýç rengin neyse o.
                visualGridCells[x,y].sortingOrder = firstSortingOrder;
            }
        }
    }
    public (List<int> rows, List<int> cols) SimulateLineCompletion(BlockData blockData, Vector2Int gridPosition)
    {
        // 1. Geçici bir mantýksal grid oluþtur ve mevcut grid'i üzerine kopyala.
        // Bu, ana oyun durumunu bozmamanýn en güvenli yoludur.
        Transform[,] simulatedGrid = (Transform[,])logicGrid.Clone();

        // 2. Bloðu bu geçici grid'e "hayali" olarak yerleþtir.
        // Herhangi bir null olmayan obje, hücrenin dolu olduðunu belirtmek için yeterlidir.
        // 'transform' (GridManager'ýn kendi transform'u) bu iþ için kullanýlabilir.
        foreach (var cellOffset in blockData.cells)
        {
            Vector2Int pos = gridPosition + cellOffset;
            if (IsWithinGrid(pos.x, pos.y))
            {
                simulatedGrid[pos.x, pos.y] = this.transform;
            }
        }

        // 3. Bu geçici grid üzerinde tamamlama kontrolü yap.
        List<int> completedRows = new List<int>();
        for (int y = 0; y < height; y++)
        {
            bool rowIsComplete = true;
            for (int x = 0; x < width; x++)
            {
                if (simulatedGrid[x, y] == null)
                {
                    rowIsComplete = false;
                    break;
                }
            }
            if (rowIsComplete)
            {
                completedRows.Add(y);
            }
        }

        List<int> completedCols = new List<int>();
        for (int x = 0; x < width; x++)
        {
            bool colIsComplete = true;
            for (int y = 0; y < height; y++)
            {
                if (simulatedGrid[x, y] == null)
                {
                    colIsComplete = false;
                    break;
                }
            }
            if (colIsComplete)
            {
                completedCols.Add(x);
            }
        }

        // 4. Sonucu (tamamlanacak satýr ve sütun listelerini) geri döndür.
        return (completedRows, completedCols);
    }

        [Header("Baþlangýç Bloklarý Ayarlarý")]
    [Tooltip("Oyun baþýnda grid'in yaklaþýk yüzde kaçýnýn dolacaðýný belirtir.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float initialFillPercentage = 0.2f; // %20
    [SerializeField] private int maxPlacementTries = 50; // Sonsuz döngüyü önlemek için deneme sayýsý

    private void GenerateInitialBlocks()
    {
        if (initialFillPercentage <= 0) return;

        // Toplam hücre sayýsýna göre kaç hücre dolduracaðýmýzý hesapla
        int totalCells = width * height;
        int cellsToFill = Mathf.RoundToInt(totalCells * initialFillPercentage);
        int cellsFilled = 0;
        int tries = 0;

        // Belirlediðimiz sayýda hücreyi doldurana veya deneme hakkýmýz bitene kadar devam et
        while (cellsFilled < cellsToFill && tries < maxPlacementTries)
        {
            tries++;

            // 1. Rastgele bir blok verisi al
            BlockData randomBlockData = BlockSpawner.Instance.GetRandomBlockData();
            if (randomBlockData == null) continue;

            // 2. Rastgele bir konum seç
            Vector2Int randomPosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));

            // 3. Bloðun oraya yerleþip yerleþemeyeceðini kontrol et
            if (CanPlaceBlock(randomBlockData, randomPosition))
            {
                // 4. Yerleþebiliyorsa, yerleþtir.
                // Bu, oyun baþýnda olduðu için animasyon veya ses istemiyoruz.
                // Sadece mantýksal ve görsel olarak yerleþtireceðiz.
                GameObject blockObject = Instantiate(BlockSpawner.Instance.blockBasePrefab, (Vector2)randomPosition, Quaternion.identity, this.transform);
                blockObject.name = $"Initial_{randomBlockData.name}";

                foreach (Vector2Int cellPos in randomBlockData.cells)
                {
                    // Görsel hücreleri oluþtur
                    GameObject cell = Instantiate(randomBlockData.blockCellPrefab, blockObject.transform);
                    cell.transform.localPosition = (Vector2)cellPos;
                    cell.GetComponent<SpriteRenderer>().color = Color.yellow;

                    // Mantýksal grid'i güncelle
                    Vector2Int targetPos = randomPosition + cellPos;
                    logicGrid[targetPos.x, targetPos.y] = cell.transform;
                }

                // Tüm dragger'larý devre dýþý býrak
                foreach (var dragger in blockObject.GetComponentsInChildren<BlockDragger>())
                {
                    dragger.isPlaced = true;
                }

                cellsFilled += randomBlockData.cells.Count;
            }
        }
        Debug.Log($"{cellsFilled} hücre baþlangýçta dolduruldu.");
    }
}