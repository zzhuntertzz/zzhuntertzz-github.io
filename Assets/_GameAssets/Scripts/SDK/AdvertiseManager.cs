using System;
using DG.Tweening;

[Serializable]
public class AdConfig
{
    public bool
        isTestAd = false,
        isShowAd = true,
        isSkipFirstAdInter = false,
        isDebug = false,
        usingBannerCollapse;
}

public enum MyAdType
{
    Banner, Inters, Offer, Reward, Native, MRec, Collapse
}

public enum MyBannerPos
{
    TopLeft,
    TopCenter,
    TopRight,
    Centered,
    CenterLeft,
    CenterRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public static class AdvertiseManager
{
    public static AdConfig adConfig;
    
    public static bool isRequestShowBanner = true;

    public static void Init(AdConfig config)
    {
        adConfig = config;
        if (!IsCanShowAd())
            isRequestShowBanner = false;
#if USING_IRONSOURCE
        IronSourceManager.Init(adConfig);
#endif
#if USING_MAX
        MaxManager.Init(adConfig);
#endif
#if USING_ADMOB
        AdmobManager.Init(adConfig);
#endif
        AppOpenAdLauncher.Instance.Init();
    }
    
    public static void CheckShowBannerBottom()
    {
        if (!IsCanShowAd()) return;
        if (adConfig.usingBannerCollapse)
        {
#if USING_ADMOB
            AdmobManager.ShowAd(MyAdType.Collapse);
#endif
            return;
        }
#if USING_IRONSOURCE
        IronSourceManager.ShowAd(MyAdType.Banner,
            IronSourceBannerSize.SMART, 
            IronSourceBannerPosition.BOTTOM);
#elif USING_MAX
        MaxManager.ShowAd(MyAdType.Banner, MaxSdkBase.BannerPosition.BottomCenter);
#elif USING_ADMOB
        // AdmobManager.ShowAd(MyAdType.Banner);
#endif
    }
    
    public static void CheckShowMRec()
    {
        if (!IsCanShowAd()) return;
        if (adConfig.usingBannerCollapse)
        {
            // AdmobManager.ShowAd(MyAdType.Collapse);
            return;
        }
#if USING_IRONSOURCE
#elif USING_MAX
        MaxManager.ShowAd(MyAdType.MRec);
#endif
    }
    
    public static void CheckShowNative(params object[] objects)
    {
#if USING_IRONSOURCE
#elif USING_MAX
#elif USING_ADMOB
        // AdmobManager.PrepareAd(MyAdType.Native, objects);
#endif
    }
    
    public static void HideBanner()
    {
        if (adConfig.usingBannerCollapse)
        {
            // AdmobManager.HideAd(MyAdType.Collapse);
            return;
        }
#if USING_IRONSOURCE
        IronSourceManager.HideAd(MyAdType.Banner);
#elif USING_MAX
        MaxManager.HideAd(MyAdType.Banner);
#elif USING_ADMOB
        // AdmobManager.HideAd(MyAdType.Banner);
#endif
    }
    
    public static void HideCollapse()
    {
        if (adConfig.usingBannerCollapse)
        {
#if USING_ADMOB
            AdmobManager.HideAd(MyAdType.Collapse);
#endif
            return;
        }
    }
    
    public static void HideMRec()
    {
        if (adConfig.usingBannerCollapse)
        {
            // AdmobManager.HideAd(MyAdType.Collapse);
            return;
        }
#if USING_IRONSOURCE
        IronSourceManager.HideAd(MyAdType.MRec);
#elif USING_MAX
        MaxManager.HideAd(MyAdType.MRec);
#endif
    }

    public static bool BannerIsShowing()
    {
#if USING_IRONSOURCE
        return IronSourceManager.IsLoaded(MyAdType.Banner);
#elif USING_MAX
        MaxManager.IsLoaded(MyAdType.Banner);
#elif USING_ADMOB
        // AdmobManager.IsLoaded(MyAdType.Banner);
#endif
        return false;
    }

    public static void ShowInterstitial(string placement)
    {
        if (!adConfig.isShowAd) return;
        if (adConfig.isSkipFirstAdInter)
        {
            adConfig.isSkipFirstAdInter = false;
            return;
        }
        if (!IsCanShowAd()) return;
        // Debug.Log($">check level {TrackingController.inter_show_at_level}");
        if (PlayerData.PlayerInfo.levelPassed < (int) RemoteConfigManager.GetValue(
            RemoteConfigManager.KEY_INTERS_SHOW_AT_LEVEL).LongValue) return;
        if (IsDelayingAdInter()) return;
        DelayAdInter();
        // Debug.Log(">> Show Inters");
#if USING_IRONSOURCE
        IronSourceManager.ShowAd(MyAdType.Inters);
#elif USING_MAX
        MaxManager.ShowAd(MyAdType.Inters, placement);
#elif USING_ADMOB
        // AdmobManager.ShowAd(MyAdType.Inters, placement);
#endif
    }
    public static bool IsVideoAdsReady()
    {
#if USING_IRONSOURCE
        return IronSourceManager.IsLoaded(MyAdType.Reward);
#elif USING_MAX
        return MaxManager.IsLoaded(MyAdType.Reward);
#elif USING_ADMOB
        // return AdmobManager.IsLoaded(MyAdType.Reward);
#endif
        return false;
    }

    public static void RequestRewardedAd()
    {
#if USING_IRONSOURCE
        IronSourceManager.PrepareAd(MyAdType.Reward);
#elif USING_MAX
        MaxManager.PrepareAd(MyAdType.Reward);
#elif USING_ADMOB
        // AdmobManager.PrepareAd(MyAdType.Reward);
#endif
    }

    public static void ShowVideoAds(string placement, Action onSuccess, Action onFailed)
    {
        if (!adConfig.isShowAd)
        {
            onSuccess();
            return;
        }
        DelayAdInter();
#if USING_IRONSOURCE
        IronSourceManager.ShowAd(MyAdType.Reward, onSuccess, onFailed);
#elif USING_MAX
        MaxManager.ShowAd(MyAdType.Reward, placement, onSuccess, onFailed);
#elif USING_ADMOB
        // AdmobManager.ShowAd(MyAdType.Reward, placement, onSuccess);
#else
        onSuccess();
#endif
    }
    
    public static void OnApplicationPause(bool isPaused) {              
#if USING_IRONSOURCE   
        IronSource.Agent.onApplicationPause(isPaused);
#endif
    }

    public static float GetBannerAdHeight()
    {
#if USING_IRONSOURCE
#elif USING_MAX
        return MaxManager.GetBannerAdHeight();
#elif USING_ADMOB
#else
#endif
        return default;
    }

    public static bool IsCanShowAd()
    {
        return !PlayerData.PlayerShoppingData.IsAdRemoved();
    }

    private static bool isDelayingInter = false;
    private static Tween twDelayInter;
    private static void DelayAdInter()
    {
#if USING_FIREBASE
        isDelayingInter = true;
        twDelayInter?.Kill();
        twDelayInter = FunctionCommon.DelayTime((int) RemoteConfigManager.GetValue(
                RemoteConfigManager.Key_INTERS_DELAY_SHOW).LongValue,
            delegate
            {
                isDelayingInter = false;
            }).SetUpdate(true);
#endif
    }
    private static bool IsDelayingAdInter()
    {
        return isDelayingInter;
    }
}