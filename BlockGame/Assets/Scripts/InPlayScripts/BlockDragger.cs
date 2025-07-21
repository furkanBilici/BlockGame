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

    private int dragPlaneLayerMask;
    private GameObject ghostBlock;

    // Sabitler daha okunaklý yapar
    private const int DRAGGING_SORTING_ORDER = 10;
    private const int PLACED_SORTING_ORDER = 0;

    Vector3 offsetForBlock=new Vector3(0,3f,0f);
    Vector3 offsetForZ = new Vector3(0, 0,-0.5f);
    private float ghostBlockVisuality = 0.5f;
    [SerializeField] Material ghostBlockMaterial;
    Material blockMaterial;

    private void Awake()
    {
        mainCamera = Camera.main;
        blockParent = transform.parent;
        gridManager = FindFirstObjectByType<GridManager>();
        dragPlaneLayerMask = LayerMask.GetMask("DragPlane");
    }

    void OnMouseDown()
    {
        if (isPlaced || UIManager.Instance.panelActive) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("HoldBlock");
        isDragging = true;
        blockMaterial = blockParent.GetComponentInChildren<Renderer>().material;

        GhostBlockCreator();
        Vector3? hitPoint = GetMouseWorldPositionOnPlane();
        initialPosition = blockParent.position;
        if (hitPoint.HasValue)
        {
            offset = blockParent.position - (hitPoint.Value + offsetForBlock);
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging || UIManager.Instance.panelActive) return;

        Vector3? hitPoint = GetMouseWorldPositionOnPlane();
        if (hitPoint.HasValue)
        {
            blockParent.position = hitPoint.Value + offsetForBlock + offset+ offsetForZ;
        }
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
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("PutBlock");

            // Yerleþtirme baþarýlý
            Vector3 finalPos = new Vector3(gridPos.x, gridPos.y, 0); // Z pozisyonunu sýfýrla
            blockParent.position = finalPos;

            BlockSpawner.Instance.RemoveFromActiveBlocks(blockParent.gameObject);
            if (GameMechanicsManager.Instance != null) GameMechanicsManager.Instance.OnBlockPlaced();
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
        
    }

    private void ReturnToInitialPosition()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("CannotPutBlock");
        blockParent.position = initialPosition;
        gridManager.ResetGridColors();
    }
    private Vector3? GetMouseWorldPositionOnPlane()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, dragPlaneLayerMask))
        {
           
            return hitInfo.point;
        }
        return null;
    }



    private void ShowGhost()
    {
        if (ghostBlock == null) return;

        Block block = blockParent.GetComponent<Block>();
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(blockParent.position.x), Mathf.RoundToInt(blockParent.position.y));

        if (gridManager.CanPlaceBlock(block.data, gridPos))
        {
            ghostBlock.transform.position = new Vector3(gridPos.x, gridPos.y, 0); // Z pozisyonu 0'da
            ghostBlock.SetActive(true);

            var completedLines = gridManager.SimulateLineCompletion(block.data, gridPos);
            if (completedLines.rows.Count > 0 || completedLines.cols.Count > 0)
            {
               
                gridManager.HighlightLines(completedLines.rows, completedLines.cols, blockMaterial);
            }
            else
            {
                gridManager.ResetGridColors();
            }
        }
        else
        {
            ghostBlock.SetActive(false);
            gridManager.ResetGridColors();
        }
    }

    void GhostBlockCreator()
    {
        ghostBlock = Instantiate(blockParent.gameObject);
        ghostBlock.name = "GhostBlock";

        // Hayaletin sürüklenememesi için üzerindeki tüm BlockDragger'larý kaldýr.
        foreach (var dragger in ghostBlock.GetComponentsInChildren<BlockDragger>())
        {
            Destroy(dragger);
        }

        // 3D objeler için SpriteRenderer deðil, Renderer'ý hedef almalýyýz.
        foreach (Renderer renderer in ghostBlock.GetComponentsInChildren<Renderer>())
        {
            renderer.material = ghostBlockMaterial;
        }

        ghostBlock.SetActive(false);
    }
}
