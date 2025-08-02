using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Ayarlarý")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private GameObject gridCellPrefab;

    public Animator birdAnimator;

    private Transform[,] logicGrid;
    private bool isClearing = false;

    //private Renderer[,] visualGridCells;
    CustomGameManager cGameManager;
   // Color initialGridColor;
    private MaterialPropertyBlock mpb;
    private void Awake()
    { 
        mpb=new MaterialPropertyBlock();
       // initialGridColor=startBlocksMaterial.color;
        cGameManager = FindFirstObjectByType<CustomGameManager>();
        if (cGameManager!=null)
        {
            width = height = cGameManager.size; 
        }
        logicGrid = new Transform[width, height];
        //visualGridCells = new Renderer[width, height];
        
    }

    void Start()
    {
        GenerateGrid();
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
                GameObject newCell = Instantiate(gridCellPrefab, new Vector3(x, y, 0.5f), Quaternion.identity, this.transform);
                newCell.name = $"Cell({x},{y})";
                //visualGridCells[x, y] = newCell.GetComponent<SpriteRenderer>();
                
            }
        }
        //if (visualGridCells[0, 0] != null) initialGridColor = visualGridCells[0, 0].material.color;
    }

    public void PlaceBlock(GameObject blockObject, Vector2Int gridPosition)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("PutBlock");
        Block blockData = blockObject.GetComponent<Block>();
        foreach (Vector2Int cellOffset in blockData.currentShapeCells)
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
        var completedLines=CheckForCompletedLines(true);
        bool linesWereCleared = (completedLines.rows.Count > 0 || completedLines.cols.Count > 0);
        if (GameMechanicsManager.Instance != null) GameMechanicsManager.Instance.OnBlockPlaced();
        if (!linesWereCleared)
        {
            BlockSpawner.Instance.OnActionFinished();
        }
    }

    private (List<int> rows, List<int> cols) CheckForCompletedLines(bool executeClear, Transform[,] gridToUse = null)
    {
        Transform[,] targetGrid = gridToUse ?? logicGrid;
        List<int> completedRows = new List<int>();
        for (int y = 0; y < height; y++)
        {
            bool rowIsComplete = true;
            for (int x = 0; x < width; x++) { if (targetGrid[x, y] == null) { rowIsComplete = false; break; } }
            if (rowIsComplete) { completedRows.Add(y); }
        }

        List<int> completedCols = new List<int>();
        for (int x = 0; x < width; x++)
        {
            bool colIsComplete = true;
            for (int y = 0; y < height; y++) { if (targetGrid[x, y] == null) { colIsComplete = false; break; } }
            if (colIsComplete) { completedCols.Add(x); }
        }

        if (executeClear&&(completedRows.Count > 0 || completedCols.Count > 0))
        {
            int totalLines = completedRows.Count + completedCols.Count;
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(totalLines);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("CleanLineSound");
            StartCoroutine(ClearLinesRoutine(completedRows, completedCols));
        }
        return (completedRows, completedCols);
    }
    [Header("Görsel Efektler")]
    [SerializeField] private GameObject lineClearEffectPrefab;
    private IEnumerator ClearLinesRoutine(List<int> rows, List<int> cols)
    {
        isClearing = true;

        List<Transform> cellsToClear = new List<Transform>();
        float duration = 0.4f;
        foreach (int y in rows)
        {
            for (int x = 0; x < width; x++)
            {
                if (logicGrid[x, y] != null && !cellsToClear.Contains(logicGrid[x, y]))
                {
                    if (logicGrid[x, y].name == "SurpriseBox")
                    {
                        if (GameMechanicsManager.Instance != null) GameMechanicsManager.Instance.OnTriggerSurprise();
                    }
                    if (birdAnimator != null) birdAnimator.Play("Spin");
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
                    if (logicGrid[x, y].name == "SurpriseBox")
                    {
                        if (GameMechanicsManager.Instance != null) GameMechanicsManager.Instance.OnTriggerSurprise();
                    }
                    if (birdAnimator != null) birdAnimator.Play("Spin");
                    cellsToClear.Add(logicGrid[x, y]);
                }
            }
        }
        if (lineClearEffectPrefab != null)
        {
            foreach (Transform cell in cellsToClear)
            {
                if (cell != null)
                {
                    Instantiate(lineClearEffectPrefab, cell.position, Quaternion.identity);
                }
            }
        }
        if (cellsToClear.Count > 0)
        {

            // Parlama rengini ayarla.
            mpb.SetColor("_BaseColor", blockColor); // URP ise "_BaseColor"

            // Her bir hücreye bu parlama özelliðini uygula.
            foreach (Transform cell in cellsToClear)
            {
                if (cell != null && cell.name!="SurpriseBox")
                {
                    cell.GetComponent<Renderer>().SetPropertyBlock(mpb);
                }
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

    public bool CanPlaceBlock(List<Vector2Int> blocks, Vector2Int gridPosition)
    {
        if (isClearing) return false;

        foreach (Vector2Int cellOffset in blocks)
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
    public bool IsAnyMovePossible(List<Vector2Int> block)
    {
        List<Vector2Int> blockType2 = new List<Vector2Int>();
        List<Vector2Int> blockType3 = new List<Vector2Int>();
        List<Vector2Int> blockType4 = new List<Vector2Int>();
        
        for (int i = 0; i < block.Count; i++)
        {
            blockType2.Add(new Vector2Int(block[i].y, -block[i].x)) ;
            blockType3.Add(new Vector2Int(blockType2[i].y, -blockType2[i].x));
            blockType4.Add(new Vector2Int(blockType3[i].y, -blockType3[i].x));
        }
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (CanPlaceBlock(block, new Vector2Int(x, y)))
                {
                    return true;
                }
                if (CanPlaceBlock(blockType2, new Vector2Int(x, y)))
                {
                    return true;
                }
                if (CanPlaceBlock(blockType3, new Vector2Int(x, y)))
                {
                    return true;
                }
                if (CanPlaceBlock(blockType4, new Vector2Int(x, y)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    Color blockColor;
    public void HighlightLines(List<int> rows, List<int> cols, Color highlightColor)
    {
        ResetGridColors();
        blockColor = highlightColor;
        mpb.SetColor("_BaseColor", highlightColor);

        if (rows != null)
        {
            foreach (int y in rows)
            {
                for (int x = 0; x < width; x++)
                {
                    if (logicGrid[x, y] != null) // Eðer hücrede bir blok parçasý varsa
                    {
                        if (logicGrid[x, y].name != "SurpriseBox")
                        {
                            Renderer rend = logicGrid[x, y].GetComponent<Renderer>();
                            if (rend != null)
                            {
                                // Materyali DEÐÝÞTÝRME, sadece property block'u ata!
                                rend.SetPropertyBlock(mpb);
                            }
                        }
                    }
                }
            }
        }
        if (cols != null)
        {
            foreach (int x in cols)
            {
                for (int y = 0; y < height; y++)
                {
                    if (logicGrid[x, y] != null)
                    {
                        if (logicGrid[x, y].name != "SurpriseBox")
                        {
                            Renderer rend = logicGrid[x, y].GetComponent<Renderer>();
                            if (rend != null)
                            {
                                rend.SetPropertyBlock(mpb);
                            }
                        }
                    }
                }
            }
        }
    }
    
    public void ResetGridColors()
    {
        mpb.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (logicGrid[x, y] != null)
                {
                    Transform cellTransform = logicGrid[x, y];
                    Renderer rend = cellTransform.GetComponent<Renderer>();

                    // Parçanýn ana bloðunu (parent'ýný) bul.
                    Block parentBlock = cellTransform.GetComponentInParent<Block>();

                    // Hem renderer hem de ana blok bulunduysa devam et (NullReferenceException'ý önler).
                    if (rend != null && parentBlock != null)
                    {
                        if (logicGrid[x, y].name != "SurpriseBox")
                        {
                            // 1. Ana bloðun hafýzasýndan (Block script'i) doðru rengi oku.
                            Color originalColor = parentBlock.color; // Block.cs'te 'public Color color;' olmalý.

                            // 2. MPB'yi bu renkle doldur.
                            mpb.SetColor("_BaseColor", originalColor); // Veya "_BaseColor"

                            // 3. Bu rengi, o anki blok parçasýna uygula.
                            rend.SetPropertyBlock(mpb);
                        }
                    }
                }
            }
        }
    }
    public (List<int> rows, List<int> cols) SimulateLineCompletion(List<Vector2Int> blockData, Vector2Int gridPosition)
    {
        Transform[,] simulatedGrid = (Transform[,])logicGrid.Clone();
        foreach (var cellOffset in blockData)
        {
            Vector2Int pos = gridPosition + cellOffset;
            if (IsWithinGrid(pos.x, pos.y))
            {
                simulatedGrid[pos.x, pos.y] = this.transform;
            }
        }

        // Artýk ayný fonksiyonu SÝMÜLASYON modunda çaðýrýyoruz.
        return CheckForCompletedLines(false, simulatedGrid);
    }

    public Vector2Int? GetRandomEmptyCell()
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (logicGrid[x, y] == null) emptyCells.Add(new Vector2Int(x, y));
            }
        }
        if (emptyCells.Count == 0) return null;
        return emptyCells[Random.Range(0, emptyCells.Count)];
    }

    public void OccupyCell(Vector2Int position, Transform obj, string name)
    {
        if (IsWithinGrid(position.x, position.y))
        {
            logicGrid[position.x, position.y] = obj;
            logicGrid[position.x, position.y].name = name;
        }
    }
    public void GenerateInitialBlocks(float initialFillPercentage, int maxPlacementTries)
    {
        if (initialFillPercentage <= 0) return;
      
        int totalCells = width * height;
        int cellsToFill = Mathf.RoundToInt(totalCells * initialFillPercentage);
        int cellsFilled = 0;
        int tries = 0;
        while (cellsFilled < cellsToFill && tries < maxPlacementTries)
        {
            tries++;

            // 1. Rastgele bir blok verisi al
            BlockData randomBlockData = BlockSpawner.Instance.GetRandomBlockData();
            if (randomBlockData == null) continue;

            // 2. Rastgele bir konum seç
            Vector2Int randomPosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));

            // 3. Bloðun oraya yerleþip yerleþemeyeceðini kontrol et
            if (CanPlaceBlock(randomBlockData.cells, randomPosition))
            {
                // 4. Yerleþebiliyorsa, yerleþtir.
                // Bu, oyun baþýnda olduðu için animasyon veya ses istemiyoruz.
                // Sadece mantýksal ve görsel olarak yerleþtireceðiz.
                GameObject blockObject = Instantiate(BlockSpawner.Instance.blockBasePrefab, (Vector2)randomPosition, Quaternion.identity, this.transform);
                blockObject.name = $"Initial_{randomBlockData.name}";

                Color randomColor= BlockSpawner.Instance.GetRandomColor();

                blockObject.GetComponent<Block>().color=randomColor;

                mpb.SetColor("_BaseColor", randomColor);

                foreach (Vector2Int cellPos in randomBlockData.cells)
                {
                    // Görsel hücreleri oluþtur
                    GameObject cell = Instantiate(randomBlockData.blockCellPrefab, blockObject.transform);
                    cell.transform.localPosition = (Vector2)cellPos;
                    cell.GetComponent<Renderer>().SetPropertyBlock(mpb);

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
    }
}