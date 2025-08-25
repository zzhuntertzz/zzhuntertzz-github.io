using System;
#if USING_AOA
using GoogleMobileAds.Api;
#endif

public class AppOpenAdManager
{
#if UNITY_ANDROID
    private string ID_TIER_1 = "ca-app-pub-9819920607806935/5577411761";
    private string ID_TIER_2 = "ca-app-pub-9819920607806935/5577411761";
    private string ID_TIER_3 = "ca-app-pub-9819920607806935/5577411761";
#elif UNITY_IOS
    private const string ID_TIER_1 = "";
    private const string ID_TIER_2 = "";
    private const string ID_TIER_3 = "";
#else
    private const string ID_TIER_1 = "";
    private const string ID_TIER_2 = "";
    private const string ID_TIER_3 = "";
#endif

    private static AppOpenAdManager instance;

#if USING_AOA
    private AppOpenAd ad;

    private bool IsAdAvailable => ad != null && (System.DateTime.UtcNow - loadTime).TotalHours < 4;
#endif

    private DateTime loadTime;

    private bool isShowingAd = false;

    public static bool showFirstOpen = false;

    public static bool ConfigOpenApp = true;
    public static bool ConfigResumeApp = true;

    public static bool ResumeFromAds = false;
    public static bool IsBlockedAd = false;

    public static AppOpenAdManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AppOpenAdManager();
            }
            return instance;
        }
    }

    private int tierIndex = 1;

    public void LoadAd()
    {
        if (IsBlockedAd)
            return;

        LoadAOA();
    }

    public void LoadAOA()
    {
#if USING_AOA
        string id = ID_TIER_1;
        if (tierIndex == 2)
            id = ID_TIER_2;
        else if (tierIndex == 3)
            id = ID_TIER_3;

        Debug.Log("Start request Open App Ads Tier " + tierIndex);

        AdRequest request = new AdRequest.Builder().Build();

        AppOpenAd.Load(id, request, ((appOpenAd, error) =>
        {
            if (error != null)
            {
                // Handle the error.
                Debug.LogFormat("Failed to load the ad. (reason: {0}), tier {1}",
                    error.GetMessage(), tierIndex);
                tierIndex++;
                if (tierIndex <= 3)
                    LoadAOA();
                else
                    tierIndex = 1;
                return;
            }

            // App open ad is loaded.
            ad = appOpenAd;
            tierIndex = 1;
            loadTime = DateTime.UtcNow;
            if (!showFirstOpen && ConfigOpenApp)
            {
                ShowAdIfAvailable();
                showFirstOpen = true;
            }
        }));
#endif
    }

    public void ShowAdIfAvailable()
    {
#if USING_AOA
        if (!IsAdAvailable || isShowingAd || ResumeFromAds)
        {
            return;
        }

        ad.OnAdFullScreenContentClosed += HandleAdDidDismissFullScreenContent;
        ad.OnAdFullScreenContentFailed += HandleAdFailedToPresentFullScreenContent;
        ad.OnAdFullScreenContentOpened += HandleAdDidPresentFullScreenContent;
        ad.OnAdImpressionRecorded += HandleAdDidRecordImpression;
        ad.OnAdPaid += HandlePaidEvent;

        ad.Show();
#endif
    }

#if USING_AOA
    private void HandleAdDidDismissFullScreenContent()
    {
        Debug.Log("Closed app open ad");
        // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        ad = null;
        isShowingAd = false;
        LoadAd();
    }

    private void HandleAdFailedToPresentFullScreenContent(AdError args)
    {
        Debug.LogFormat("Failed to present the ad (reason: {0})", args.GetMessage());
        // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        ad = null;
        LoadAd();
    }

    private void HandleAdDidPresentFullScreenContent()
    {
        Debug.Log("Displayed app open ad");
        isShowingAd = true;
    }

    private void HandleAdDidRecordImpression()
    {
        Debug.Log("Recorded ad impression");
    }

    private void HandlePaidEvent(AdValue args)
    {
        Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
            args.CurrencyCode, args.Value);
    }
#endif
}