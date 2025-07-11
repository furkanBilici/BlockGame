using UnityEngine;

public class BlockDragger : MonoBehaviour
{
    Vector3 offset;
    Camera mainCamera;
    Transform blockParent;
    Vector3 initialPosition;

    GridManager gridManager;
    public bool isPlaced = false;
    public bool isDragging = false;
    const int draggingSortingOrder = 10;
    const int droppedSortingOrder = 2;

    private void Awake()
    {
        mainCamera = Camera.main;
        blockParent = transform.parent;
        gridManager=FindAnyObjectByType<GridManager>();   
    }
    void OnMouseDown()
    {
        if(isPlaced) return;
        isDragging = true;
        initialPosition = blockParent.position;
        offset = blockParent.position - GetMouseWorldPosition();
        SetSortingOrderOfChildren(draggingSortingOrder);
    }
    
    void OnMouseDrag()
    {
        if (!isDragging) return;
            blockParent.position=GetMouseWorldPosition()+ offset;
    }
    void OnMouseUp()
    {
        if (!isDragging)return;

        isDragging = false; // Sürükleme bitti.

        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(blockParent.position.x), Mathf.RoundToInt(blockParent.position.y));
        Block block = blockParent.GetComponent<Block>();

        if (gridManager.CanPlaceBlock(block.data, gridPos))
        {
            PlaceBlock(gridPos);
        }
        else
        {
            ReturnToInitialPosition();
        }
    }
    private void PlaceBlock(Vector2Int gridPos)
    {
        blockParent.position = (Vector2)gridPos;
        gridManager.PlaceBlock(blockParent.gameObject, gridPos);
        BlockSpawner.Instance.OnBlockPlaced(blockParent.gameObject);

        // Bloðun tüm parçalarýna yerleþtiðini bildir.
        foreach (var dragger in blockParent.GetComponentsInChildren<BlockDragger>())
        {
            dragger.isPlaced = true;
        }

        SetSortingOrderOfChildren(droppedSortingOrder);
    }
    private void ReturnToInitialPosition()
    {
        blockParent.position = initialPosition;
        SetSortingOrderOfChildren(droppedSortingOrder);
    }
    private void SetSortingOrderOfChildren(int order)
    {
        foreach (var renderer in blockParent.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.sortingOrder = order;
        }
    }
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint=Input.mousePosition;
        mousePoint.z=-mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(new Vector3(mousePoint.x, mousePoint.y, 0));
    }
}
