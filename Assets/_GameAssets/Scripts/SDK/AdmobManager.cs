#if USING_ADMOB

using System;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

public static class AdmobManager
{
#if UNITY_ANDROID
    private static string AD_UNIT_ID_BANNER =
        "ca-app-pub-8652816628411018/6274246228";
    private static string AD_UNIT_ID_INTERS =
        "ca-app-pub-9819920607806935/1439823808";
    private static string AD_UNIT_ID_REWARD =
        "ca-app-pub-9819920607806935/9463290652";
    public static string AD_UNIT_ID_NATIVE =
        "ca-app-pub-8652816628411018/9017358438";
    public static string AD_UNIT_ID_COLLAPSE =
        "ca-app-pub-9819920607806935/6455412929";
#elif UNITY_IOS
    private static string AD_UNIT_ID_BANNER = "";
    private static string AD_UNIT_ID_INTERS = "";
    private static string AD_UNIT_ID_REWARD = "";
    public static string AD_UNIT_ID_NATIVE = "";
#else
    private static string AD_UNIT_ID_BANNER = "";
    private static string AD_UNIT_ID_INTERS = "";
    private static string AD_UNIT_ID_REWARD = "";
    public static string AD_UNIT_ID_NATIVE = "";
#endif

    private static BannerView _bannerView;
    private static InterstitialAd _interstitialAd;
    private static RewardedAd _rewardedAd;
    
    private static string bannerAdapter => _bannerView.GetResponseInfo().GetMediationAdapterClassName();
    private static string interstitialAdapter => _interstitialAd.GetResponseInfo().GetMediationAdapterClassName();
    private static string rewardedAdapter => _rewardedAd.GetResponseInfo().GetMediationAdapterClassName();

    private static bool bannerLoaded = false;
    private static Action onRewarded = delegate { };
    private static Action onRewardedFail = delegate { };

    private static AdConfig _adConfig;
    
    public static void Init(AdConfig adConfig)
    {
        _adConfig = adConfig;
        if (_adConfig.isTestAd)
        {
            AD_UNIT_ID_BANNER = "ca-app-pub-3940256099942544/6300978111";
            AD_UNIT_ID_INTERS = "ca-app-pub-3940256099942544/1033173712";
            AD_UNIT_ID_REWARD = "ca-app-pub-3940256099942544/5224354917";
            AD_UNIT_ID_NATIVE = "ca-app-pub-3940256099942544/2247696110";
            AD_UNIT_ID_COLLAPSE = "ca-app-pub-3940256099942544/2014213617";
        }
        
        MobileAds.Initialize(HandleInitCompleteAction);
    }

    public static void PrepareAd(MyAdType MyAdType, params object[] objects)
    {
        if (bannerLoaded && MyAdType == MyAdType.Banner) return;
        if (IsLoaded(MyAdType)) return;
        AdRequest adRequest = new AdRequest.Builder().Build();
        switch (MyAdType)
        {
            case MyAdType.Banner:
            case MyAdType.Collapse:
                var isCollapse = MyAdType == MyAdType.Collapse;
                _bannerView = new BannerView(
                    isCollapse ? AD_UNIT_ID_COLLAPSE : AD_UNIT_ID_BANNER,
                    AdSize.GetLandscapeAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth),
                    AdPosition.Bottom);
                // adRequest = new AdRequest();
                // Create an extra parameter that aligns the bottom of the expanded ad to the
                // bottom of the bannerView.
                if (isCollapse)
                    adRequest.Extras.Add("collapsible", "bottom");
                
                _bannerView.LoadAd(adRequest);
                _bannerView.OnBannerAdLoaded += delegate
                {
                    if (AdvertiseManager.isRequestShowBanner)
                        ShowAd(MyAdType);
                    else
                        HideAd(MyAdType);
                };
                
                _bannerView.OnAdPaid += delegate(AdValue value)
                {
                    HandleAdPaidEvent(value, isCollapse ?
                        AdTypeLog.bannerCollapse : AdTypeLog.banner, bannerAdapter);
                };
                _bannerView.OnBannerAdLoaded += delegate
                {
                    bannerLoaded = true;
                };
                _bannerView.OnBannerAdLoadFailed += async delegate
                {
                    PrepareAd(MyAdType);
                };
                break;
            
            case MyAdType.Inters:
                _interstitialAd?.Destroy();
                _interstitialAd = null;
                InterstitialAd.Load(AD_UNIT_ID_INTERS, adRequest,
                    async delegate(InterstitialAd ad, LoadAdError error)
                    {
                        if (error is not null || ad is null)
                        {
                            if (error != null)
                                new EventInterstitialLoadFailed()
                                {
                                    ad_inter_load_failed_reason = error.GetMessage(),
                                }.Post();
                            await UniTask.Delay(PlayerData.PlayerRemoteData.delay_load_admob);
                            PrepareAd(MyAdType.Inters);
                            return;
                        }

                        _interstitialAd = ad;
                        new EventInterstitialLoadSuccess().Post();
                        new ABIEventAFAdInterCall().Post();
                        
                        _interstitialAd.OnAdPaid += delegate(AdValue value)
                        {
                            HandleAdPaidEvent(value, AdTypeLog.interstitial, interstitialAdapter);
                        };
                        _interstitialAd.OnAdFullScreenContentOpened += delegate
                        {
                            new EventInterstitialShowSuccess().Post();
                            new ABIEventAFAdInterShow().Post();
                            
                            PlayerData.PlayerTracking.WatchAdInter();
                        };
                        _interstitialAd.OnAdFullScreenContentClosed += async delegate
                        {
                            new EventInterstitialClose().Post();
                            
                            _interstitialAd?.Destroy();
                            _interstitialAd = null;
                            PrepareAd(MyAdType.Inters);

                            await UniTask.Delay(1000);
                            AppOpenAdManager.ResumeFromAds = false;
                        };
                        _interstitialAd.OnAdFullScreenContentFailed += async delegate (AdError error)
                        {
                            AppOpenAdManager.ResumeFromAds = false;
                            
                            new EventInterstitialShowFailed()
                            {
                                ad_interstitial_show_failed_reason = error.GetMessage(),
                            }.Post();
                            
                            PrepareAd(MyAdType.Inters);
                        };
                    });
                new EventInterstitialLoad().Post();
                break;
            
            case MyAdType.Reward:
                Debug.Log($"> admob req reward");
                _rewardedAd?.Destroy();
                _rewardedAd = null;
                RewardedAd.Load(AD_UNIT_ID_REWARD, adRequest,
                    async delegate(RewardedAd ad, LoadAdError error)
                    {
                        if (error is not null || ad is null)
                        {
                            if (error != null)
                                new EventRewardLoadFailed()
                                {
                                    ad_reward_load_failed_reason = error.GetMessage(),
                                }.Post();
                            Debug.Log($"> admob req reward fail");
                            await UniTask.Delay(PlayerData.PlayerRemoteData.delay_load_admob);
                            PrepareAd(MyAdType.Reward);
                            return;
                        }
                        
                        Debug.Log($"> admob req success");
                        _rewardedAd = ad;
                        new EventRewardLoadSuccess().Post();
                        new ABIEventAFAdRewardCall().Post();
                        
                        _rewardedAd.OnAdFullScreenContentOpened += delegate
                        {
                            AppOpenAdManager.ResumeFromAds = true;
                            
                            new EventRewardShow().Post();
                            PlayerData.PlayerTracking.WatchAdReward();
                        };
                        _rewardedAd.OnAdFullScreenContentFailed += async delegate (AdError error)
                        {
                            AppOpenAdManager.ResumeFromAds = false;
                            
                            new EventRewardShowFailed()
                            {
                                ad_reward_show_failed_reason = error.GetMessage(),
                            }.Post();
                            
                            PrepareAd(MyAdType.Reward);
                            onRewardedFail?.Invoke();
                        };
                        _rewardedAd.OnAdPaid += delegate(AdValue value)
                        {
                            HandleAdPaidEvent(value, AdTypeLog.rewarded, rewardedAdapter);
                        };
                        _rewardedAd.OnAdFullScreenContentClosed += async delegate
                        {
                            new EventRewardClose().Post();
                            
                            _rewardedAd?.Destroy();
                            _rewardedAd = null;
                            PrepareAd(MyAdType.Reward);

                            await UniTask.Delay(1000);
                            AppOpenAdManager.ResumeFromAds = false;
                        };
                });
                new EventRewardLoad().Post();
                break;
            
            case MyAdType.Native:
                break;
        }
    }

    public static void ShowAd(MyAdType MyAdType, params object[] objects)
    {
        switch (MyAdType)
        {
            case MyAdType.Banner:
            case MyAdType.Collapse:
                break;
            
            case MyAdType.MRec:
                break;
            
            case MyAdType.Inters:
                new ABIEventAFAdInterEligible().Post();
                break;
            
            case MyAdType.Reward:
                new ABIEventAFAdRewardEligible().Post();
                break;
        }
        
        if (!IsLoaded(MyAdType))
        {
            PrepareAd(MyAdType, objects);
            return;
        }

        string placement = "";
        switch (MyAdType)
        {
            case MyAdType.Banner:
            case MyAdType.Collapse:
                _bannerView.Show();
                break;
            
            case 
                MyAdType.Inters:
                AppOpenAdManager.ResumeFromAds = true;
                _interstitialAd.Show();
                Debug.Log($"> Admob Show Inter");
                break;
            
            case MyAdType.Reward:
                AppOpenAdManager.ResumeFromAds = true;
                if (objects.Length > 1 && objects[1] is Action rewardTrue)
                    onRewarded = rewardTrue;
                if (objects.Length > 2 && objects[2] is Action rewardFail)
                    onRewardedFail = rewardFail;
                _rewardedAd.Show(delegate(Reward reward)
                {
                    onRewarded?.Invoke();
                    onRewarded = null;
                    new ABIEventAFAdRewardComplete().Post();
                });
                Debug.Log($"> Admob Show Reward");
                break;
            
            case MyAdType.Native:
                break;
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
            case MyAdType.Collapse:
                _bannerView?.Hide();
                break;
        }
    }

    public static bool IsLoaded(MyAdType MyAdType)
    {
        switch (MyAdType)
        {
            case MyAdType.Banner:
            case MyAdType.Collapse:
                return _bannerView is not null || bannerLoaded;
            
            case MyAdType.Inters:
                if (_interstitialAd is null) return false;
                return _interstitialAd.CanShowAd();
            
            case MyAdType.Reward:
                if (_rewardedAd is null) return false;
                return _rewardedAd.CanShowAd();
            
            case MyAdType.Native:
                return false;
            
            default: return false;
        }
    }

    public static void HandleAdPaidEvent(AdValue adInfo, AdTypeLog adType, string adapter)
    {
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            GameTracking.SendRevenue("GoogleAdMob", new()
            {
                revenue = adInfo.Value,
                RevenuePrecision = adInfo.Precision.ToString(),
                CurrencyCode = adInfo.CurrencyCode,  
            });
        });
    }

    private static void HandleInitCompleteAction(InitializationStatus initStatus)
    {
        MobileAdsEventExecutor.ExecuteInUpdate(async () =>
        {
            await UniTask.Delay(500);
            // PrepareAd(MyAdType.Banner);
            if (AdvertiseManager.isRequestShowBanner)
            {
                PrepareAd(MyAdType.Collapse);
            }
            
            PrepareAd(MyAdType.Inters);
            
            await UniTask.Delay(500);
            PrepareAd(MyAdType.Reward);
        });
    }
}

#endif