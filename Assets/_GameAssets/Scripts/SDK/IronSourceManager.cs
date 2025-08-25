#if USING_IRONSOURCE

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Tutti.G;
using UnityEngine;

public static class IronSourceManager
{
    private static AdConfig _adConfig;
    
    private static string _appKey = "unexpected_platform";
    
    private static readonly string _bannerCap = "banner_bottom";
    private static bool isRewarded = false;
    private static Action onRewarded = delegate { }, onRewardFail = delegate { };

    private static int timeDelayPrepareAd = 5000;
    private static bool isInitDone = false;

    private static List<MyAdType> lstLoadingAd = new();

    public static void Init(AdConfig adConfig)
    {
        if (adConfig.isTestAd)
        {
#if UNITY_ANDROID
            _appKey = "85460dcd";
#elif UNITY_IPHONE
            _appKey = "8545d445";
#endif
        }
        else
        {
#if UNITY_ANDROID
            _appKey = "1a8f1c035";
#elif UNITY_IPHONE
            _appKey = "";
#endif
        }
        
        // IronSource.Agent.validateIntegration();
        
        IronSource.Agent.init(_appKey);
        IronSource.Agent.setAdaptersDebug(adConfig.isDebug);
        
        
        IronSourceBannerEvents.onAdLoadFailedEvent += async delegate (IronSourceError err)
        {
            lstLoadingAd.Remove(MyAdType.Banner);
            Debug.LogError($">> err banner {err.getErrorCode()} {err.getDescription()}");
            await UniTask.Delay(timeDelayPrepareAd, DelayType.UnscaledDeltaTime);
            PrepareAd(MyAdType.Banner);
        };
        
        
        IronSourceInterstitialEvents.onAdReadyEvent += delegate(IronSourceAdInfo info)
        {
            lstLoadingAd.Remove(MyAdType.Inters);
            MyTracking.LogInterLoad(info.adNetwork);
        };
        IronSourceInterstitialEvents.onAdClickedEvent += delegate(IronSourceAdInfo info)
        {
            MyTracking.LogInterClick(info.adNetwork);
        };
        IronSourceInterstitialEvents.onAdLoadFailedEvent += async delegate (IronSourceError err)
        {
            lstLoadingAd.Remove(MyAdType.Inters);
            Debug.LogError($">> err inter {err.getErrorCode()} {err.getDescription()}");
            await UniTask.Delay(timeDelayPrepareAd, DelayType.UnscaledDeltaTime);
            PrepareAd(MyAdType.Inters);
            MyTracking.LogInterFail($"{err.getErrorCode()}");
        };
        IronSourceInterstitialEvents.onAdShowSucceededEvent += delegate(IronSourceAdInfo info)
        {
            MyTracking.LogInterShow(info.adNetwork);
        };
        IronSourceInterstitialEvents.onAdClosedEvent += async delegate
        {
            lstLoadingAd.Remove(MyAdType.Inters);
            await UniTask.Delay(timeDelayPrepareAd, DelayType.UnscaledDeltaTime);
            PrepareAd(MyAdType.Inters);
        };
        
        
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += async delegate
        {
            Debug.LogError($">> reward unavailable");
        };
        IronSourceRewardedVideoEvents.onAdAvailableEvent += delegate(IronSourceAdInfo info)
        {
            Debug.Log($">> reward loaded");
            MyTracking.LogRewardLoad(info.adNetwork);
        };
        IronSourceRewardedVideoEvents.onAdLoadFailedEvent += async delegate (IronSourceError err)
        {
            Debug.LogError($">> err reward load {err.getErrorCode()} {err.getDescription()}");
            MyTracking.LogRewardFail($"{err.getErrorCode()}");
        };
        IronSourceRewardedVideoEvents.onAdClickedEvent += delegate(IronSourcePlacement placement, IronSourceAdInfo info)
        {
            MyTracking.LogRewardClick();
        };
        IronSourceRewardedVideoEvents.onAdClosedEvent += delegate
        {
            // if (isRewarded)
            // {
            onRewarded();
            isRewarded = false;
            // }

            onRewarded = delegate { };

            PrepareAd(MyAdType.Reward);
        };
        IronSourceRewardedVideoEvents.onAdRewardedEvent += delegate
        {
            isRewarded = true;
            MyTracking.LogRewardComplete();
        };
        IronSourceRewardedVideoEvents.onAdOpenedEvent += delegate(IronSourceAdInfo info)
        {
            MyTracking.LogRewardShow(info.adNetwork);
        };
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += async delegate
            (IronSourceError err, IronSourceAdInfo info)
        {
            Debug.LogError($">> err reward show {err.getErrorCode()} {err.getDescription()}");
            MyTracking.LogRewardFail($"{err.getErrorCode()} {err.getDescription()}");
            onRewardFail();
            onRewardFail = delegate { };
        };

        IronSourceEvents.onImpressionDataReadyEvent += ImpressionSuccessEvent;
        IronSourceEvents.onSdkInitializationCompletedEvent += OnSdkInitDone;
        
        Debug.Log($">>> initing sdk");
    }

    private static void OnSdkInitDone()
    {
        isInitDone = true;
        
        PrepareAd(MyAdType.Inters);
        PrepareAd(MyAdType.Reward);

        Debug.Log($">>> init sdk done");
    }

    public static async void PrepareAd(MyAdType MyAdType, params object[] objects)
    {
        if (!isInitDone) return;
        if (!AdvertiseManager.IsCanShowAd() && MyAdType != MyAdType.Reward) return;
        if (IsLoaded(MyAdType))
        {
            return;
        }
        if (lstLoadingAd.Contains(MyAdType))
        {
            Debug.Log($">> loading ad {MyAdType.ToString()}");
            return;
        }
        lstLoadingAd.Add(MyAdType);
        
        Debug.Log($">> prepare ad {MyAdType.ToString()}");
        switch (MyAdType)
        {
            case MyAdType.Banner:
                var bannerSize = IronSourceBannerSize.BANNER;
                bannerSize.SetAdaptive(false);
                if (PlayerData.PlayerRemoteData.bannerAdaptive)
                {
                    bannerSize = IronSourceBannerSize.SMART;
                    bannerSize.SetAdaptive(true);
                }
                if (objects.Length > 0 && objects[0] is IronSourceBannerSize newSize)
                    bannerSize = newSize;
                var bannerPos = IronSourceBannerPosition.BOTTOM;
                if (objects.Length > 1 && objects[1] is IronSourceBannerPosition newPos)
                    bannerPos = newPos;
                IronSource.Agent.loadBanner(bannerSize, bannerPos, _bannerCap);
                break;
            
            case MyAdType.Inters:
                IronSource.Agent.loadInterstitial();
                break;
            
            case MyAdType.Reward:
                IronSource.Agent.loadRewardedVideo();
                break;
        }
        
        if (MyAdType == MyAdType.Reward)
        {
            if (IsLoaded(MyAdType))
            {
                lstLoadingAd.Remove(MyAdType);
                return;
            }

            await UniTask.Delay(timeDelayPrepareAd, DelayType.UnscaledDeltaTime);
            lstLoadingAd.Remove(MyAdType);
            if (!IsLoaded(MyAdType))
                PrepareAd(MyAdType);
        }
    }

    public static void ShowAd(MyAdType MyAdType, params object[] objects)
    {
        if (!isInitDone)
        {
            if (MyAdType == MyAdType.Reward)
            {
#if UNITY_EDITOR
                if (objects.Length > 0 && objects[0] is Action reward)
                    reward();
#else
                if (objects.Length > 1 && objects[1] is Action fail)
                    fail();
#endif
            }
            return;
        }
        
        if (!IsLoaded(MyAdType))
            PrepareAd(MyAdType, objects);
        else
            Debug.Log($">> show ad {MyAdType.ToString()}");
        
        switch (MyAdType)
        {
            case MyAdType.Banner:
                IronSource.Agent.displayBanner();
                break;
            
            case MyAdType.Inters:
                IronSource.Agent.showInterstitial();
                break;
            
            case MyAdType.Reward:
                if (objects.Length > 0 && objects[0] is Action onRewarded)
                    IronSourceManager.onRewarded = onRewarded;
                if (objects.Length > 1 && objects[1] is Action onRewardFail)
                    IronSourceManager.onRewardFail = onRewardFail;
                if (IsLoaded(MyAdType))
                {
                    IronSource.Agent.showRewardedVideo();
                }
                else
                {
                    IronSourceManager.onRewardFail();
                }
                break;
        }
    }
    
    public static void HideAd(MyAdType MyAdType, params object[] objects)
    {
        switch (MyAdType)
        {
            case MyAdType.Banner:
                IronSource.Agent.hideBanner();
                break;
        }
    }

    public static bool IsLoaded(MyAdType MyAdType)
    {
        switch (MyAdType)
        {
            case MyAdType.Banner:
                return IronSource.Agent.isBannerPlacementCapped(_bannerCap);
            
            case MyAdType.Inters:
                return IronSource.Agent.isInterstitialReady();
            
            case MyAdType.Reward:
                return IronSource.Agent.isRewardedVideoAvailable();
            
            default: return false;
        }
    }
    
    private static void ImpressionSuccessEvent(IronSourceImpressionData impressionData) {
        if (impressionData != null) {
            Firebase.Analytics.Parameter[] AdParameters = {
                new Firebase.Analytics.Parameter("ad_platform", "ironSource"),
                new Firebase.Analytics.Parameter("ad_source", impressionData.adNetwork),
                new Firebase.Analytics.Parameter("ad_unit_name", impressionData.instanceName),
                new Firebase.Analytics.Parameter("ad_format", impressionData.adUnit),
                new Firebase.Analytics.Parameter("currency","USD"),
                new Firebase.Analytics.Parameter("value", impressionData.revenue.GetValueOrDefault())
            };
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdParameters);
        }
        Debug.Log($">> impression {impressionData}");
    }
}

#endif