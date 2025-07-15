// BlockDragger.cs (Nihai ve D�zeltilmi� Versiyon)

using UnityEngine;

public class BlockDragger : MonoBehaviour
{
    private Vector3 offset;
    private Camera mainCamera;
    private Transform blockParent;
    private Vector3 initialPosition;

    private GridManager gridManager;
    public bool isPlaced = false;
    private bool isDragging = false;

    // Sabitler daha okunakl� yapar
    private const int DRAGGING_SORTING_ORDER = 10;
    private const int PLACED_SORTING_ORDER = 0;

    

    private void Awake()
    {
        mainCamera = Camera.main;
        blockParent = transform.parent;
        gridManager = FindFirstObjectByType<GridManager>();
    }

    void OnMouseDown()
    {
        if (isPlaced || UIManager.Instance.panelActive) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("HoldBlock");
        isDragging = true;

        GhostBlockCreator();
        
        initialPosition = blockParent.position;
        offset = blockParent.position - GetMouseWorldPosition();
        SetSortingOrderOfChildren(DRAGGING_SORTING_ORDER);
    }

    void OnMouseDrag()
    {
        if (!isDragging || UIManager.Instance.panelActive) return;
        blockParent.position = GetMouseWorldPosition() + offset;
        ShowGhost();   
    }

    void OnMouseUp()
    {
        if (!isDragging || UIManager.Instance.panelActive) return;

        
        Destroy(ghostBlock);
        gridManager.ResetGridColors();
        isDragging = false;
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(blockParent.position.x), Mathf.RoundToInt(blockParent.position.y));
        Block block = blockParent.GetComponent<Block>();

        if (gridManager.CanPlaceBlock(block.data, gridPos))
        {
            // YERLE�T�RME BA�ARILI
            blockParent.position = (Vector2)gridPos;

            // 1. �NCE blo�u aktif listeden silmesi i�in Spawner'a haber ver.
            BlockSpawner.Instance.RemoveFromActiveBlocks(blockParent.gameObject);

            // 2. SONRA blo�u yerle�tirmesi ve s�reci devam ettirmesi i�in GridManager'� tetikle.
            gridManager.PlaceBlock(blockParent.gameObject, gridPos);

            block.AnimationPlacement();
            SetAsPlaced();
        }
        else
        {
            ReturnToInitialPosition();
        }
    }


    private void SetAsPlaced()
    {
        foreach (var dragger in blockParent.GetComponentsInChildren<BlockDragger>())
        {
            dragger.isPlaced = true;
        }
        SetSortingOrderOfChildren(PLACED_SORTING_ORDER);
    }

    private void ReturnToInitialPosition()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("CannotPutBlock");
        blockParent.position = initialPosition;
        gridManager.ResetGridColors();
        SetSortingOrderOfChildren(PLACED_SORTING_ORDER);
    }

    private void SetSortingOrderOfChildren(int order)
    {
        foreach (var renderer in blockParent.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.sortingOrder = order;
        }
    }

    // D�zeltilmi� fonksiyon
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    private GameObject ghostBlock;
    float ghostBlockVisuality = 0.6f;
    private void ShowGhost()
    {
        Block block=blockParent.GetComponent<Block>();
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(blockParent.position.x), Mathf.RoundToInt(blockParent.position.y));
        if (gridManager.CanPlaceBlock(block.data, gridPos))
        {
            ghostBlock.transform.position = new Vector2(gridPos.x,gridPos.y);
            ghostBlock.SetActive(true);
            // 1. bu hamlenin hangi hatlar� tamamlayaca��n� sim�le et
            var completedLines = gridManager.SimulateLineCompletion(block.data, gridPos);

            // 2. e�er tamamlanacak hat varsa, onlar� hayalet blok renginde vurgula
            if (completedLines.rows.Count > 0 || completedLines.cols.Count > 0)
            {
                // Hayalet blo�un rengini al (ilk h�cresinden).
                Color ghostColor = ghostBlock.GetComponentInChildren<SpriteRenderer>().color;
                gridManager.HighlightLines(completedLines.rows, completedLines.cols, ghostColor);
            }
            else // e�er tamamlanacak hat yoksa, t�m renkleri s�f�rla.
            {
                gridManager.ResetGridColors();
            }
        }
        else
        {
            ghostBlock.SetActive(false);
            // Ge�ersiz bir hamle ise de renkleri s�f�rla.
            gridManager.ResetGridColors();
        }
    }

    void GhostBlockCreator()
    {
        ghostBlock = Instantiate(blockParent.gameObject);

        foreach (SpriteRenderer sr in ghostBlock.GetComponentsInChildren(typeof(SpriteRenderer))) 
        {
            Color color = sr.color;
            color.a = ghostBlockVisuality;
            sr.color = color;
        }

        ghostBlock.SetActive(false);
    }


}