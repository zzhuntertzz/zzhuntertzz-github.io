#if USING_AOA
using GoogleMobileAds.Api;
#endif

public class AppOpenAdLauncher : SinglePrivaton<AppOpenAdLauncher>
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        AppOpenAdManager.IsBlockedAd = PlayerData.PlayerShoppingData.IsAdRemoved();
// #if USING_AOA
#if !UNITY_EDITOR && USING_AOA
        MobileAds.Initialize(status => { AppOpenAdManager.Instance.LoadAd(); });
#endif
    }

    private void OnApplicationPause(bool pause)
    {
#if !UNITY_EDITOR && USING_AOA
// #if USING_AOA
        if (!pause && AppOpenAdManager.ConfigResumeApp &&
            !AppOpenAdManager.ResumeFromAds &&
            RemoteConfigManager.GetValue(RemoteConfigManager.KEY_USE_AOA).BooleanValue &&
            !PlayerData.PlayerShoppingData.IsAdRemoved())
        {
            AppOpenAdManager.Instance.ShowAdIfAvailable();
        }
#endif
    }
    
    public void Init() { }
}
