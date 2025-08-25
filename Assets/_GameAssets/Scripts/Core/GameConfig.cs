using Lean.Pool;
using UnityEngine;

public class GameConfig : SinglePrivaton<GameConfig>
{
    public void Init()
    {
        Application.targetFrameRate = 60;
        
        InAppUpdate.Instance.Initialize();
        
        PlayerData.PlayerDaily.GetDaysPlayed();

        if (!Debug.isDebugBuild)
        {
            Debug.unityLogger.logEnabled = false;
        }
        
        ResourceController.Instance.AddQueue(async delegate
        {
            if (Debug.isDebugBuild)
            {
            }
            var appsflyer = await A.Get<GameObject>(MyKeys.Prefabs.AppsFlyerObject);
            Instantiate(appsflyer);
            var iap = await A.Get<GameObject>(MyKeys.Prefabs.ProductManager);
            Instantiate(iap);

            // var input = await A.Get<GameObject>("PlayerInput");
            // Instantiate(input);
        });
        
        FirebaseController.OnFirebaseInitialized += RemoteConfigManager.Init;
        FirebaseController.Instance.Initialize();
            
        AdvertiseManager.Init(new AdConfig()
        {
            isShowAd = true,
            isTestAd = Debug.isDebugBuild,
            isSkipFirstAdInter = true,
            isDebug = Debug.isDebugBuild,
        });
    }
}