using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// [2024/12/9 kmlee123, 24-12-9-1] (code add)
using GoogleMobileAds;
using GoogleMobileAds.Api;
using Core.Library;

public enum AdWatchError
{
    Success = 0, // 성공
    FailRemainAdNotExist, // 남은 광고가 없음
    FailNowWatchedAd, // 광고를 아직 시청중임
}

// _SJ      광고 관리자
public class AdvertisementManager : SingletonTemplate<AdvertisementManager>
{
#if UNITY_ANDROID
    private string rewardAdUnitId = "ca-app-pub-6576422988170332/4757428302";
    private string bannerAdUnitId = "ca-app-pub-6576422988170332/5325067229";
#elif UNITY_IPHONE
    private string rewardAdUnitId = "ca-app-pub-6576422988170332/6777648062";
    private string bannerAdUnitId = "ca-app-pub-6576422988170332/4011985553";
#else
    private string rewardAdUnitId = "unused";
    private string bannerAdUnitId = "unused";
#endif

    public class AdvertisementEvent : UnityEvent{}
    
    public class AdvertisementWatchEvent : UnityEvent<AdWatchError>{}
    
    // [2024/12/9 kmlee123, 24-12-9-1] (code del)
    // [SerializeField]
    // private AdvertisementEvent mOnPlayAD = new AdvertisementEvent(); // 재생 이벤트 (출력하는 쪽 기준)

    [SerializeField]
    private AdvertisementWatchEvent mOnWatchEndAD = new AdvertisementWatchEvent(); // 재생 끝났음 이벤트 (보는 쪽 기준)

    private bool isNowWatchedAD = false; 

    // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    private RewardedAd rewardedAd;
    private BannerView bannerView;

    // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    private GameObject bannerBackgroundPanel = null;

    // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    private float bannerHeight = 0.0f;
    public float GetBannerHeight() { return bannerHeight; }

    // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    UnityAction bannerLoadedAction = null;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void Init()  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    {
        MobileAds.Initialize(initState => { });
    }

    public void SetPlayAdEvent(UnityAction playStart)
    {
        ResetPlayAdEvent();
        //mOnPlayAD.AddListener(playStart);  // [2024/12/9 kmlee123, 24-12-9-1] (code del)
    }

    public void ResetPlayAdEvent()
    {
        //mOnPlayAD.RemoveAllListeners();  // [2024/12/9 kmlee123, 24-12-9-1] (code del)
    }

    public void LoadRewardedAd()  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var adRequest = new AdRequest();

        Debug.Log("Call rewaredeAdd load");
        
        RewardedAd.Load(rewardAdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("GoogleAdmob rewarded ad load fail. error : " + error);
                    return;
                }

                rewardedAd = ad;

                RegisterRewaredAdHandler();
            });
    }
    
    private void RegisterRewaredAdHandler()  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    {
        if (rewardedAd == null)
        {
            Debug.Log("RegisterRewaredAdHandler: banner view is null");
            return;
        }

        rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                LoadRewardedAd();
            };

        rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                LoadRewardedAd();
            };
    }

    public void PlayAd(UnityAction<AdWatchError> onWatchEndEvent)
    {
        mOnWatchEndAD.RemoveAllListeners();
        if (onWatchEndEvent != null)
        {
            mOnWatchEndAD.AddListener(onWatchEndEvent);
        }
        
        if (!ItemManager.Instance.IsActivateAD())
        {
            // 광고 제거 활성화 되어있으면 바로 끝내기처리
            OnEndPlayAd();
            return;
        }

        // [2024/12/9 kmlee123, 24-12-9-1] (code modify)
        //if (isNowWatchedAD)
        if (isNowWatchedAD || rewardedAd == null || !rewardedAd.CanShowAd())
        {
            OnEndPlayAdWithError(AdWatchError.FailNowWatchedAd);
            return; // 광고 시청중 (혹은 광고없음)
        }
        
        isNowWatchedAD = true;

        // [2024/12/9 kmlee123, 24-12-9-1] (code modify)
        //mOnPlayAD?.Invoke();
        rewardedAd.Show((Reward reward) =>
            {
                OnEndPlayAd();
            });

        return;
    }

    private void OnEndPlayAdWithError(AdWatchError error)
    {
        mOnWatchEndAD?.Invoke(error);
    }

    public void OnEndPlayAd()
    {
        isNowWatchedAD = false;
        mOnWatchEndAD?.Invoke(AdWatchError.Success);
    }

    public void StartBannerAd(GameObject bannerAdPanel, UnityAction bannerLoaded)  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    {
        bannerBackgroundPanel = bannerAdPanel;
        bannerLoadedAction = bannerLoaded;

        bool isActivateAD = ItemManager.Instance.IsActivateAD(); // 광고 활성화 여부
        if (isActivateAD)
        {
            LoadBannerAd();
        }

        if (bannerBackgroundPanel)  // 광고 로드가 되기전에는 배너 배경을 일단 숨긴다.
        {
            bannerHeight = 0.0f;
            bannerBackgroundPanel.SetActive(false);
        }
    }

    public void ReleaseBannerAd()  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    {
        bannerBackgroundPanel = null;
        bannerLoadedAction = null;
        
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
    }

    public void LoadBannerAd()  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    {
        if (bannerView == null)
        {
            CreateBannerView();
        }

        RegisterBannerAdHandler();

        var adRequest = new AdRequest();

        Debug.Log("Call banner ad load");

        bannerView.LoadAd(adRequest);
    }

    private void CreateBannerView()  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);

        bannerView = new BannerView(bannerAdUnitId, adaptiveSize, AdPosition.Bottom);
    }

    private void RegisterBannerAdHandler()  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    {
        if (bannerView == null)
        {
            Debug.Log("RegisterBannerAdHandler: banner view is null");
            return;
        }

        bannerView.OnBannerAdLoaded += () =>
            {
                OnBannerLoaded();
            };
    }

    private void OnBannerLoaded()  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    {
        bannerHeight = bannerView.GetHeightInPixels();
        
        if (bannerBackgroundPanel)
        {
            RectTransform bannerRect = CodeUtilLibrary.GetRectTransform(bannerBackgroundPanel);
            if (bannerRect)
            {
                Vector2 bannerSize = bannerRect.sizeDelta;
                bannerSize.y = bannerHeight;
                bannerRect.sizeDelta = bannerSize;
            }
            
            bannerBackgroundPanel.SetActive(true);
        }

        bannerLoadedAction?.Invoke();
    }
}
