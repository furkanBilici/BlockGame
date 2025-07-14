// BlockDragger.cs (Nihai ve Düzeltilmiþ Versiyon)

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

    // Sabitler daha okunaklý yapar
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
        if (isPlaced) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("HoldBlock");
        isDragging = true;

        GhostBlockCreator();
        
        initialPosition = blockParent.position;
        offset = blockParent.position - GetMouseWorldPosition();
        SetSortingOrderOfChildren(DRAGGING_SORTING_ORDER);
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        blockParent.position = GetMouseWorldPosition() + offset;
        ShowGhost();   
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("PutBlock");
        Destroy(ghostBlock);
        isDragging = false;
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(blockParent.position.x), Mathf.RoundToInt(blockParent.position.y));
        Block block = blockParent.GetComponent<Block>();

        if (gridManager.CanPlaceBlock(block.data, gridPos))
        {
            // YERLEÞTÝRME BAÞARILI
            blockParent.position = (Vector2)gridPos;

            // 1. ÖNCE bloðu aktif listeden silmesi için Spawner'a haber ver.
            BlockSpawner.Instance.RemoveFromActiveBlocks(blockParent.gameObject);

            // 2. SONRA bloðu yerleþtirmesi ve süreci devam ettirmesi için GridManager'ý tetikle.
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
        blockParent.position = initialPosition;
        SetSortingOrderOfChildren(PLACED_SORTING_ORDER);
    }

    private void SetSortingOrderOfChildren(int order)
    {
        foreach (var renderer in blockParent.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.sortingOrder = order;
        }
    }

    // Düzeltilmiþ fonksiyon
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
        }
        else
        {
            ghostBlock.SetActive(false);
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