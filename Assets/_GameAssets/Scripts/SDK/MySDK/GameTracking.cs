using System.Collections.Generic;
#if USING_APPSFLYER
using AppsFlyerSDK;
#endif
using Cysharp.Threading.Tasks;
#if USING_FIREBASE
using Firebase.Analytics;
#endif
using UnityEngine;

public class AdInfoValue
{
    public string Placement, AdFormat, CreativeIndentify, AdUnitIdentifier,
        RevenuePrecision, NetworkName, NetworkPlacement, CurrencyCode = "USD";
    public double revenue;
}

public class GameTracking
{
    public static string LastScreen;

    public static void SendRevenue(string adPlatform, AdInfoValue adValue)
    {
#if USING_FIREBASE
        SendFirebaseRevenue(adPlatform, adValue);
#endif
#if USING_APPSFLYER
        SendAppsFlyerRevenue(adPlatform, adValue);
#endif
    }

#if USING_FIREBASE
    private static async void SendFirebaseRevenue(string adPlatform, AdInfoValue adValue)
    {
        // if (!FirebaseController.IsFirebaseReadied) return;
        while (!FirebaseController.IsFirebaseReadied)
        {
            await UniTask.Delay(1000);
        }
        // await UniTask.WaitUntil(() => FirebaseController.IsFirebaseReadied);

        Parameter[] parameters = {
            new Parameter("value", adValue.revenue),
            new Parameter("currency", 
                string.IsNullOrEmpty(adValue.CurrencyCode)? "USD" : adValue.CurrencyCode),
            new Firebase.Analytics.Parameter("ad_unit_name", adValue.AdUnitIdentifier),
            new Parameter("precision", adValue.RevenuePrecision.ToString()),
            new Parameter("ad_format", adValue.AdFormat),
            new Parameter("ad_source", adValue.NetworkName),
            new Parameter("ad_platform", adPlatform)
        };
        FirebaseAnalytics.LogEvent("paid_ad_impression", parameters);
    }
#endif

#if USING_APPSFLYER
    private static void SendAppsFlyerRevenue(string adPlatform,  AdInfoValue adValue)
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add("ad_platform", adPlatform);
        dic.Add("ad_source", adValue.NetworkName);
        dic.Add("ad_unit_name", adValue.AdUnitIdentifier);
        dic.Add("ad_format", adValue.AdFormat);
        dic.Add("placement", adValue.Placement);
        dic.Add("value", $"{adValue.revenue}");
        dic.Add("currency", "USD");

        // MonoBehaviour.print($"Game logAdRevenue adapter: {adAdapter}," +
        //                     $" platform: {adPlatform}, revenue: {adValue.revenue / 1000000f}");
        
        AppsFlyerAdRevenue.logAdRevenue(adValue.NetworkName,
            GetAFNetworkType(adPlatform),
            adValue.revenue, "USD", dic);
    }

    private static AppsFlyerAdRevenueMediationNetworkType GetAFNetworkType(string platform)
    {
        if (platform.Contains(GameUtils.AD_PLATFORM.AppLovin.ToString()))
            return AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax;
        if (platform.Contains(GameUtils.AD_PLATFORM.GoogleAdMob.ToString()))
            return AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob;
        if (platform.Equals(GameUtils.AD_PLATFORM.ironsource.ToString()))
            return AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource;
        Debug.LogError($">>>> get network type: {platform}");
        return default;
    }
#endif
}

public abstract class FirebaseTrackingEvent
{
    protected virtual string EventName { get; }
#if USING_FIREBASE
    private List<Parameter> firebaseParams = new();
#endif

    protected virtual void SetupDefaultParameters()
    {

    }

    protected FirebaseTrackingEvent AddParameters(List<GameTrackingParameter> extraParams)
    {
        foreach (var param in extraParams)
        {
            AddParameter(param.Name, param.Value);
        }

        return this;
    }

    protected void AddParameter(string name, object value)
    {
        if (value is null) return;
#if USING_FIREBASE
        firebaseParams.Add(new Parameter(name, value.ToString()));
        // if (value is string)
        // {
        //     firebaseParams.Add(new Parameter(name, value.ToString()));
        // }
        // else if (value is int || value is long)
        // {
        //     firebaseParams.Add(new Parameter(name, long.Parse(value.ToString())));
        // }
        // else if (value is float || value is double)
        // {
        //     firebaseParams.Add(new Parameter(name, double.Parse(value.ToString())));
        // }
        // else if (value is bool valueBool)
        // {
        //     firebaseParams.Add(new Parameter(name, valueBool ? "TRUE" : "FALSE"));
        // }
#endif

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(EventName))
            Debug.Log($"Firebase event: {EventName}, param: {name}, value: {value.ToString()}");
#endif
    }

    public async void Post()
    {
        while (!FirebaseController.IsFirebaseReadied)
        {
            await UniTask.Delay(1000);
        }
        // await UniTask.WaitUntil(() => FirebaseController.IsFirebaseReadied);
        // if (!FirebaseController.IsFirebaseReadied)
        // {
        //     Debug.Log("Firebase is not readied. Please initiate FirebaseController!");
        //     return;
        // }

        SetupDefaultParameters();
        AddParameter(nameof(GameUtils.INTERNET_CONNECTION_TYPE).ToLower(), GameUtils.InternetConnectionType);

#if USING_FIREBASE
        if (!string.IsNullOrEmpty(EventName))
            FirebaseAnalytics.LogEvent(EventName, firebaseParams.ToArray());
#endif
    }
}

public abstract class GameAppsFlyerEvent
{
    public abstract string EventName { get; }
    private Dictionary<string, string> appsflyerParams = new();

    protected virtual void SetupDefaultParameters()
    {

    }

    protected void AddParameter(string name, object value)
    {
        appsflyerParams.Add(name, value.ToString());
        
// #if UNITY_EDITOR
        if (!string.IsNullOrEmpty(EventName))
            Debug.Log($"Appsflyer event: {EventName}, param: {name}, value: {value.ToString()}");
// #endif
    }

    public void Post()
    {
        SetupDefaultParameters();
#if USING_APPSFLYER
        if (!string.IsNullOrEmpty(EventName))
            AppsFlyer.sendEvent(EventName, appsflyerParams);
#endif
    }
}

public class GameTrackingParameter
{
    public string Name;
    public object Value;

    public GameTrackingParameter(string parameterName, object parameterValue)
    {
        Name = parameterName;
        Value = parameterValue;
    }
}


public enum AdTypeLog
{
    banner,
    bannerCollapse,
    interstitial,
    native,
    video,
    rewarded_video,
    rewarded,
    mraid,
    mrec,
    offer_wall,
    playable,
    more_apps,
    video_interstitial,
    medium,
    custom,
    banner_interstitial,
    app_open,
    other,
}