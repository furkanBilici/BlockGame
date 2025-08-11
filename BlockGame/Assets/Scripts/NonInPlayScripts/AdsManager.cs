using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }
    private RewardedAd rewardedAd;
    private BannerView bannerView;
    private string rewardedAdUnitId = "ca-app-pub-3869216132353672/7814694571";
    private string bannerAdUnitId = "ca-app-pub-3869216132353672/8056768547";

    public bool adWatched = false;
    public bool IsRewardedAdLoaded => rewardedAd != null;
    public bool IsBannerAdLoaded => bannerView!=null;
    public static event Action OnRewardEarned;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            LoadRewardedAd();
            LoadBannerAd();
        });
    }

    public void LoadRewardedAd()
    {
        Debug.Log("Rewarded reklam isteniyor...");
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var adRequest = new AdRequest();

        RewardedAd.Load(rewardedAdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load with error: " + error?.GetMessage());
                return;
            }
            rewardedAd = ad;
            RegisterEventHandlers(rewardedAd);
        });
    }
    public void LoadBannerAd()
    {
        Debug.Log("Banner reklam isteniyor...");

        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        this.bannerView = new BannerView(bannerAdUnitId, adaptiveSize, AdPosition.Top);
        ListenToBannerEvents();
        var adRequest =new AdRequest();
        this.bannerView.LoadAd(adRequest);
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                OnRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.LogError("Rewarded ad is not ready yet.");
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            LoadRewardedAd(); // Reklam kapanýnca yenisini yükle.
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to show full screen content with error: " + error);
            LoadRewardedAd();
        };
    }
    public void DestroyBannerAd()
    {
        if(this.bannerView != null)
        {
            Debug.Log("BannerReklamYokediliyor");
            this.bannerView.Destroy();
            this.bannerView = null;
        }
    }
    void ListenToBannerEvents()
    {
        bannerView.OnBannerAdLoaded += () => {
            Debug.Log("Banner reklam baþarýyla yüklendi.");
        };
        bannerView.OnBannerAdLoadFailed += (LoadAdError error) => {
            Debug.LogError("Banner reklam yüklenemedi: " + error.GetMessage());
        };
    }

}