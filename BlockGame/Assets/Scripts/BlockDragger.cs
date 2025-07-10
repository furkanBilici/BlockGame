using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;

public class BlockDragger : MonoBehaviour
{
    Vector3 offset;
    Camera mainCamera;
    Transform blockParent;
    Vector3 initialPosition;

    GridManager gridManager;
    public bool isPlaced = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        blockParent = transform.parent;
        gridManager=FindAnyObjectByType<GridManager>();   
    }
    void OnMouseDown()
    {
        if (!isPlaced)
        {
            initialPosition = blockParent.position;
            offset = blockParent.position - GetMouseWorldPosition();
        }
    }
    void OnMouseDrag()
    {
        if (!isPlaced)
            blockParent.position=GetMouseWorldPosition()+ offset;
    }
    void OnMouseUp()
    {
        if (!isPlaced)
        {
            Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(blockParent.position.x), Mathf.RoundToInt(blockParent.position.y));
            Block block = blockParent.GetComponent<Block>();
            if (gridManager.CanPlaceBlock(block.data, gridPos))
            {
                blockParent.position = (Vector2)gridPos;
                gridManager.PlaceBlock(block.data, gridPos);
                foreach (var dragger in blockParent.GetComponentsInChildren<BlockDragger>())
                {
                    dragger.isPlaced = true;
                }
            }
            else
            {
                blockParent.position = initialPosition;
            }
        }
    }
    
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint=Input.mousePosition;
        return mainCamera.ScreenToWorldPoint(new Vector3(mousePoint.x, mousePoint.y, 0));
    }
}
