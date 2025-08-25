#if USING_MAX

using System;
using Cysharp.Threading.Tasks;
using Tutti.G;
using UnityEngine;

public static class MaxManager
{
    private static readonly string MAX_SDK_KEY = "ZoNyqu_piUmpl33-qkoIfRp6MTZGW9M5xk1mb1ZIWK6FN9EBu0TXSHeprC3LMPQI7S3kTc1-x7DJGSV8S-gvFJ";

#if UNITY_ANDROID
    private static readonly string AD_UNIT_ID_BANNER = "3d4b5781aa1724f6";
    private static readonly string AD_UNIT_ID_INTERS = "ef427216d55838af";
    private static readonly string AD_UNIT_ID_REWARD = "008d36617b6ed0f2";
    private static readonly string AD_UNIT_ID_MREC = "035e7d4114494dd5";
#elif UNITY_IOS
    private static readonly string AD_UNIT_ID_BANNER = "";
    private static readonly string AD_UNIT_ID_INTERS = "";
    private static readonly string AD_UNIT_ID_REWARD = "";
    private static readonly string AD_UNIT_ID_MREC = "";
#else
    private static readonly string AD_UNIT_ID_BANNER = "";
    private static readonly string AD_UNIT_ID_INTERS = "";
    private static readonly string AD_UNIT_ID_REWARD = "";
    private static readonly string AD_UNIT_ID_MREC = "";
#endif

    public static bool isRequestShowBanner = false, isRequestShowMrec = false;
    private static bool bannerLoaded = false;
    private static bool mrecLoaded = false;
    private static bool isRewarded = false;
    private static Action onRewarded = delegate { };
    private static Action onRewardedFail = delegate { }; 
    
    public static void Init(AdConfig adConfig)
    {
        if (!adConfig.isShowAd) return;
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {
            // AppLovin SDK is initialized, start loading ads
        
            PrepareAd(MyAdType.Inters);
            PrepareAd(MyAdType.Banner);
            PrepareAd(MyAdType.Reward);
            PrepareAd(MyAdType.MRec);
        };
        
        if (AdvertiseManager.adConfig.isTestAd)
        {
            MaxSdk.SetTestDeviceAdvertisingIdentifiers(
                new string[]
                {
                    "5eff7c54-91eb-49a9-9aa7-4a011c681fe2",
                    "16fc3412-6e5e-4b11-a57c-695667664683",
                });
        }
        MaxSdk.SetSdkKey(MAX_SDK_KEY);
        MaxSdk.InitializeSdk();
        
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        
        
        MaxSdkCallbacks.MRec.OnAdLoadedEvent += delegate
        {
            mrecLoaded = true;
            if (isRequestShowMrec)
                ShowAd(MyAdType.MRec);
        };
        MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += delegate(string s, MaxSdkBase.ErrorInfo info)
        {
            ShowAd(MyAdType.MRec);
        };
        
        
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += delegate
        {
            bannerLoaded = true;
            if (isRequestShowBanner)
                ShowAd(MyAdType.Banner);
        };
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += async delegate(string s, MaxSdkBase.ErrorInfo info)
        {
            await UniTask.Delay(PlayerData.PlayerRemoteData.delay_load_max);
            PrepareAd(MyAdType.Banner);
        };
        
        
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += async delegate(string s, MaxSdkBase.ErrorInfo info)
        {
            new EventInterstitialLoadFailed()
            {
                ad_inter_load_failed_reason = info.AdLoadFailureInfo,
            }.Post();
            
            await UniTask.Delay(PlayerData.PlayerRemoteData.delay_load_max);
            PrepareAd(MyAdType.Inters);
        };
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += delegate(string s, MaxSdkBase.AdInfo info)
        {
            new EventInterstitialLoadSuccess().Post();
            new ABIEventAFAdInterCall().Post();
        };
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += async delegate(string s, MaxSdkBase.AdInfo info)
        {
            PrepareAd(MyAdType.Inters);

            new EventInterstitialClose().Post();

            await UniTask.Delay(1000);
            AppOpenAdManager.ResumeFromAds = false;
        };
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += delegate(string s, MaxSdkBase.AdInfo info)
        {
            new EventInterstitialShowSuccess().Post();
            new ABIEventAFAdInterShow().Post();
            
            PlayerData.PlayerTracking.WatchAdInter();
        };
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += delegate(string s, MaxSdkBase.ErrorInfo info, MaxSdkBase.AdInfo arg3)
        {
            AppOpenAdManager.ResumeFromAds = false;

            new EventInterstitialShowFailed()
            {
                ad_interstitial_show_failed_reason = info.AdLoadFailureInfo,
            }.Post();
        };
        
        
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += async delegate(string s, MaxSdkBase.ErrorInfo info)
        {
            new EventRewardLoadFailed()
            {
                ad_reward_load_failed_reason = info.AdLoadFailureInfo,
            }.Post();
            onRewardedFail();
            
            await UniTask.Delay(PlayerData.PlayerRemoteData.delay_load_max);
            PrepareAd(MyAdType.Reward);
        };
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += delegate(string s, MaxSdkBase.AdInfo info)
        {
            new EventRewardLoadSuccess().Post();
            new ABIEventAFAdRewardCall().Post();
        };
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += delegate(string s, MaxSdkBase.AdInfo info)
        {
            new EventRewardShowSuccess().Post();
            new ABIEventAFAdRewardShow().Post();
            
            PlayerData.PlayerTracking.WatchAdReward();
        };
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += delegate(string s, MaxSdkBase.ErrorInfo info, MaxSdkBase.AdInfo arg3)
        {
            AppOpenAdManager.ResumeFromAds = false;

            new EventRewardShowFailed()
            {
                ad_reward_show_failed_reason = info.AdLoadFailureInfo,
            }.Post();
        };
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent +=
            delegate(string s, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo arg3)
            {
                isRewarded = true;
            };
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += async delegate(string s, MaxSdkBase.AdInfo info)
        {
            if (isRewarded)
            {
                onRewarded();
                isRewarded = false;
            }
        
            PrepareAd(MyAdType.Reward);
            
            new EventRewardClose().Post();
            new ABIEventAFAdRewardComplete().Post();

            await UniTask.Delay(1000);
            AppOpenAdManager.ResumeFromAds = false;
        };
    }
    
    public static void PrepareAd(MyAdType MyAdType, params object[] objects)
    {
        if (IsLoaded(MyAdType)) return;
        switch (MyAdType)
        {
            case MyAdType.Banner:
                if (bannerLoaded) break;
                var pos = MaxSdkBase.BannerPosition.BottomCenter;
                if (objects.Length > 0)
                    pos = (MaxSdkBase.BannerPosition) objects[0];
                MaxSdk.CreateBanner(AD_UNIT_ID_BANNER, pos);
                MaxSdk.SetBannerBackgroundColor(AD_UNIT_ID_BANNER, Color.clear);
                MaxSdk.SetBannerExtraParameter(AD_UNIT_ID_BANNER, "adaptive_banner", "true");
                MaxSdk.LoadBanner(AD_UNIT_ID_BANNER);
                MaxSdk.StartBannerAutoRefresh(AD_UNIT_ID_BANNER);
                break;
            
            case MyAdType.Inters:
                MaxSdk.LoadInterstitial(AD_UNIT_ID_INTERS);
                
                new EventInterstitialLoad().Post();
                break;
            
            case MyAdType.Reward:
                MaxSdk.LoadRewardedAd(AD_UNIT_ID_REWARD);

                new EventRewardLoad().Post();
                break;
            
            case MyAdType.MRec:
                MaxSdk.CreateMRec(AD_UNIT_ID_MREC, MaxSdkBase.AdViewPosition.BottomCenter);
                MaxSdk.LoadMRec(AD_UNIT_ID_MREC);
                break;
        }
    }

    public static void ShowAd(MyAdType MyAdType, params object[] objects)
    {
        switch (MyAdType)
        {
            case MyAdType.Banner:
                isRequestShowBanner = true;
                break;
            case MyAdType.MRec:
                isRequestShowMrec = true;
                break;
            
            case MyAdType.Inters:
                new ABIEventAFAdInterEligible().Post();
                break;
            
            case MyAdType.Reward:
                new ABIEventAFAdRewardEligible().Post();
                break;
        }

        void ShowAdMob()
        {
#if USING_ADMOB
            switch (MyAdType)
            {
                case MyAdType.Inters:
                    Debug.Log($"> Show Inter Admob");
                    AdmobManager.ShowAd(MyAdType.Inters);
                    break;
                case MyAdType.Reward:
                    Debug.Log($"> Show Reward Admob");
                    AdmobManager.ShowAd(MyAdType.Reward, objects);
                    break;
            }
#endif
        }
        
        Debug.Log($"> Show Ad {MyAdType}");

        try
        {
            if (!IsLoaded(MyAdType))
            {
                PrepareAd(MyAdType, objects);
                ShowAdMob();
                return;
            }
            
            string placement = "";
            switch (MyAdType)
            {
                case MyAdType.Banner:
                    MaxSdk.ShowBanner(AD_UNIT_ID_BANNER);
                    break;
            
                case MyAdType.Inters:
                    AppOpenAdManager.ResumeFromAds = true;

                    placement = objects[0].ToString();
                    MaxSdk.ShowInterstitial(AD_UNIT_ID_INTERS, placement);
                    new EventInterstitialShow().Post();
                    break;
            
                case MyAdType.Reward:
                    AppOpenAdManager.ResumeFromAds = true;

                    placement = objects[0].ToString();
                    if (objects.Length > 1 && objects[1] is Action rewardTrue)
                        onRewarded = rewardTrue;
                    if (objects.Length > 2 && objects[2] is Action rewardFail)
                        onRewardedFail = rewardFail;
                    MaxSdk.ShowRewardedAd(AD_UNIT_ID_REWARD, placement);
                    new EventRewardShow().Post();
                    break;
            
                case MyAdType.MRec:
                    MaxSdk.ShowMRec(AD_UNIT_ID_MREC);
                    break;
            }
        }
        catch (Exception e)
        {
            AppOpenAdManager.ResumeFromAds = false;

            ShowAdMob();
            return;
        }
    }

    public static void HideAd(MyAdType MyAdType, params object[] objects)
    {
        // if (!IsLoaded(MyAdType))
        // {
        //     return;
        // }
        switch (MyAdType)
        {
            case MyAdType.Banner:
                MaxSdk.HideBanner(AD_UNIT_ID_BANNER);
                isRequestShowBanner = false;
                break;
            
            case MyAdType.MRec:
                MaxSdk.HideMRec(AD_UNIT_ID_MREC);
                isRequestShowMrec = false;
                break;
        }
    }

    public static bool IsLoaded(MyAdType MyAdType)
    {
        switch (MyAdType)
        {
            case MyAdType.Banner:
                return bannerLoaded;
            
            case MyAdType.Inters:
                return MaxSdk.IsInterstitialReady(AD_UNIT_ID_INTERS);
            
            case MyAdType.Reward:
                return MaxSdk.IsRewardedAdReady(AD_UNIT_ID_REWARD);
            
            case MyAdType.MRec:
                return mrecLoaded;
            
            default: return false;
        }
    }

    public static float GetBannerAdHeight()
    {
        var height = MaxSdkUtils.GetAdaptiveBannerHeight(Screen.width);
        return height;
    }
    
    private static void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
        double revenue = impressionData.Revenue;
        var impressionParameters = new[] {
            new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
            new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
            new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
            new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
            new Firebase.Analytics.Parameter("value", revenue),
            new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
        };
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression_abi", impressionParameters);

        GameTracking.SendRevenue("AppLovin", new()
            {
                Placement = impressionData.Placement,
                AdFormat = impressionData.AdFormat,
                revenue = revenue,
                CurrencyCode = "USD",
                NetworkName = impressionData.NetworkName,
                NetworkPlacement = impressionData.NetworkPlacement,
                AdUnitIdentifier = impressionData.AdUnitIdentifier,
                CreativeIndentify = impressionData.CreativeIdentifier,
                RevenuePrecision = impressionData.RevenuePrecision,
            });
    }
}

#endif