// BlockDragger.cs (Nihai ve Düzeltilmiþ Versiyon)

using Unity.Android.Gradle.Manifest;
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


    Vector3 offsetForZ = new Vector3(0, 2f,-0.8f);

    [SerializeField] Color ghostBlockColor;
    Color blockColor;
    private MaterialPropertyBlock ghostMpb;

    [Header("Referanslar")]
    [SerializeField] private GameObject blockBasePrefab; // BlockSpawner'dakiyle ayný boþ obje prefab'ý
    [SerializeField] private GameObject blockCellPrefab; // BlockSpawner'dakiyle ayný tek küp prefab'ý
    [SerializeField] private Material ghostMaterial;
    private void Awake()
    {
        mainCamera = Camera.main;
        blockParent = transform;
        gridManager = FindFirstObjectByType<GridManager>();
        dragPlaneLayerMask = LayerMask.GetMask("DragPlane");
    }
    private void FixedUpdate()
    {
        
        doubleClickTimeCounter += Time.deltaTime;
        if (doubleClickTimeCounter >= 0.5)
        {
            doubleClickCounter = 0;
            doubleClickTimeCounter = 0;
        }
    }
    private void Start()
    {
        ghostMpb=new MaterialPropertyBlock();
    }
    public float doubleClickTimeCounter=0;
    public float doubleClickCounter = 0;
    public bool rotate = false;
    void OnMouseDown()
    {
        if (isPlaced || UIManager.Instance.panelActive) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("HoldBlock");
        doubleClickCounter++;
        if (doubleClickCounter >= 2)
        {
            rotate = true;
        }
        if (rotate)
        {
            RotateBlock();
        }
        else
        {
            isDragging = true;
            blockColor = blockParent.GetComponentInChildren<Block>().color;

            GhostBlockCreator();
            Vector3? hitPoint = GetMouseWorldPositionOnPlane();
            initialPosition = blockParent.position;
            if (hitPoint.HasValue)
            {
                offset = blockParent.position - (hitPoint.Value);
            }
        }
    }
    void OnMouseDrag()
    {
        if (!isDragging || UIManager.Instance.panelActive) return;
        Vector3? hitPoint = GetMouseWorldPositionOnPlane();
        if (hitPoint.HasValue)
        {
            blockParent.position = hitPoint.Value + offset+ offsetForZ;
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

        if (gridManager.CanPlaceBlock(block.currentShapeCells, gridPos))
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("PutBlock");
            // Yerleþtirme baþarýlý
            Vector3 finalPos = new Vector3(gridPos.x, gridPos.y, 0); // Z pozisyonunu sýfýrla
            blockParent.position = finalPos;

            BlockSpawner.Instance.RemoveFromActiveBlocks(blockParent.gameObject);
           
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
        isPlaced = true;
        
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

        if (gridManager.CanPlaceBlock(block.currentShapeCells, gridPos))
        {
            ghostBlock.transform.position = new Vector3(gridPos.x, gridPos.y, 0); // Z pozisyonu 0'da
            ghostBlock.SetActive(true);

            var completedLines = gridManager.SimulateLineCompletion(block.currentShapeCells, gridPos);
            if (completedLines.rows.Count > 0 || completedLines.cols.Count > 0)
            {

                gridManager.HighlightLines(completedLines.rows, completedLines.cols, blockColor);
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
        ghostBlock = Instantiate(blockBasePrefab);
        ghostBlock.name = "GhostBlock";
        Block block = blockParent.GetComponent<Block>();
        ghostMpb.SetColor("_BaseColor",ghostBlockColor);
        foreach (Vector2Int cellPos in block.currentShapeCells)
        {
           GameObject cell = Instantiate(blockCellPrefab, ghostBlock.transform);
           cell.transform.localPosition = (Vector2)cellPos;

           cell.GetComponent<Renderer>().SetPropertyBlock(ghostMpb);
        }
        ghostBlock.SetActive(false);
    }
    void RotateBlock()
    {
        if(BlockRotateData.Instance != null)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("RotateBlock");
            blockParent.GetComponent<Block>().RotateCells();
            rotate=false;
        }
        else
        {
            rotate = false;
        }
    }
}
