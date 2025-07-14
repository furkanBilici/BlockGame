using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Ayarlar�")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;

    [Header("Prefab")]
    [SerializeField] private GameObject gridCellPrefab; // �smi d�zeltildi

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
            Debug.LogError("Grid Cell Prefab'� GridManager'a atanmam��!");
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
        float duration = 0.4f; // Animasyon s�resini biraz art�ral�m

        // Temizlenecek t�m h�creleri tek bir listede topla
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
            // �nce hepsini parlatal�m
            foreach (Transform cell in cellsToClear)
            {
                if (cell != null) cell.GetComponent<SpriteRenderer>().color = placedBlockColor;
            }

            yield return new WaitForSeconds(duration / 2);

            // �imdi hepsini k���lterek yok edelim
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

            // Animasyon bitti, objeleri yok et ve mant�ksal grid'i temizle
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
        // �nce t�m grid'i normale d�nd�r
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
                visualGridCells[x, y].color = visualGridCellsColor; // Veya ba�lang�� rengin neyse o.
                visualGridCells[x,y].sortingOrder = firstSortingOrder;
            }
        }
    }
    public (List<int> rows, List<int> cols) SimulateLineCompletion(BlockData blockData, Vector2Int gridPosition)
    {
        // 1. Ge�ici bir mant�ksal grid olu�tur ve mevcut grid'i �zerine kopyala.
        // Bu, ana oyun durumunu bozmaman�n en g�venli yoludur.
        Transform[,] simulatedGrid = (Transform[,])logicGrid.Clone();

        // 2. Blo�u bu ge�ici grid'e "hayali" olarak yerle�tir.
        // Herhangi bir null olmayan obje, h�crenin dolu oldu�unu belirtmek i�in yeterlidir.
        // 'transform' (GridManager'�n kendi transform'u) bu i� i�in kullan�labilir.
        foreach (var cellOffset in blockData.cells)
        {
            Vector2Int pos = gridPosition + cellOffset;
            if (IsWithinGrid(pos.x, pos.y))
            {
                simulatedGrid[pos.x, pos.y] = this.transform;
            }
        }

        // 3. Bu ge�ici grid �zerinde tamamlama kontrol� yap.
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

        // 4. Sonucu (tamamlanacak sat�r ve s�tun listelerini) geri d�nd�r.
        return (completedRows, completedCols);
    }

        [Header("Ba�lang�� Bloklar� Ayarlar�")]
    [Tooltip("Oyun ba��nda grid'in yakla��k y�zde ka��n�n dolaca��n� belirtir.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float initialFillPercentage = 0.2f; // %20
    [SerializeField] private int maxPlacementTries = 50; // Sonsuz d�ng�y� �nlemek i�in deneme say�s�

    private void GenerateInitialBlocks()
    {
        if (initialFillPercentage <= 0) return;

        // Toplam h�cre say�s�na g�re ka� h�cre dolduraca��m�z� hesapla
        int totalCells = width * height;
        int cellsToFill = Mathf.RoundToInt(totalCells * initialFillPercentage);
        int cellsFilled = 0;
        int tries = 0;

        // Belirledi�imiz say�da h�creyi doldurana veya deneme hakk�m�z bitene kadar devam et
        while (cellsFilled < cellsToFill && tries < maxPlacementTries)
        {
            tries++;

            // 1. Rastgele bir blok verisi al
            BlockData randomBlockData = BlockSpawner.Instance.GetRandomBlockData();
            if (randomBlockData == null) continue;

            // 2. Rastgele bir konum se�
            Vector2Int randomPosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));

            // 3. Blo�un oraya yerle�ip yerle�emeyece�ini kontrol et
            if (CanPlaceBlock(randomBlockData, randomPosition))
            {
                // 4. Yerle�ebiliyorsa, yerle�tir.
                // Bu, oyun ba��nda oldu�u i�in animasyon veya ses istemiyoruz.
                // Sadece mant�ksal ve g�rsel olarak yerle�tirece�iz.
                GameObject blockObject = Instantiate(BlockSpawner.Instance.blockBasePrefab, (Vector2)randomPosition, Quaternion.identity, this.transform);
                blockObject.name = $"Initial_{randomBlockData.name}";

                foreach (Vector2Int cellPos in randomBlockData.cells)
                {
                    // G�rsel h�creleri olu�tur
                    GameObject cell = Instantiate(randomBlockData.blockCellPrefab, blockObject.transform);
                    cell.transform.localPosition = (Vector2)cellPos;
                    cell.GetComponent<SpriteRenderer>().color = Color.yellow;

                    // Mant�ksal grid'i g�ncelle
                    Vector2Int targetPos = randomPosition + cellPos;
                    logicGrid[targetPos.x, targetPos.y] = cell.transform;
                }

                // T�m dragger'lar� devre d��� b�rak
                foreach (var dragger in blockObject.GetComponentsInChildren<BlockDragger>())
                {
                    dragger.isPlaced = true;
                }

                cellsFilled += randomBlockData.cells.Count;
            }
        }
        Debug.Log($"{cellsFilled} h�cre ba�lang��ta dolduruldu.");
    }
}