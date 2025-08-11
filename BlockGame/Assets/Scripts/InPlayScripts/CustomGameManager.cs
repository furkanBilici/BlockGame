using UnityEngine;
using UnityEngine.Rendering;

public class CustomGameManager : MonoBehaviour
{
    [SerializeField]private int size;
    [SerializeField] private int difficulty;
    [SerializeField] private GameObject back;
    private void Start()
    {
        difficulty = PlayerPrefs.GetInt("difficulty", 0);
        size= PlayerPrefs.GetInt("boardScale", 0) + 7;
        if (size == 7)
        {
            transform.position = new Vector2(transform.position.x - 0.5f, transform.position.y - 0.5f);
        }
        else if (size == 9)
        {
            transform.position = new Vector2(transform.position.x + 0.5f, transform.position.y + 0.5f);
        }
        UIManager.Instance.GameType = 2;
        if (AdsManager.Instance != null) AdsManager.Instance.LoadBannerAd();
        SetBackStaticRecursive(back,true);
    }
    void SetBackStaticRecursive(GameObject parent,bool isStatic)
    {
        parent.isStatic = isStatic;
        var meshRenderer = parent.GetComponent<MeshRenderer>();
        if (meshRenderer != null )
        {
            if( meshRenderer.shadowCastingMode == ShadowCastingMode.On) meshRenderer.staticShadowCaster = isStatic;
            meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;


        }
        foreach (Transform child in parent.transform)
        {
            SetBackStaticRecursive(child.gameObject,isStatic);  
        }
    }
}
