using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        logicGrid = new Transform[width, height];
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
                GameObject newCell = Instantiate(gridCellPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);
                newCell.name = $"Cell({x},{y})";
            }
        }
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
                if (cell != null) cell.GetComponent<SpriteRenderer>().color = Color.white;
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
}