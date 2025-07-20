using UnityEngine;

public class ClassicGameManager : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.GameType = 0;
        if(AdsManager.Instance != null)AdsManager.Instance.LoadBannerAd();
    }

}
