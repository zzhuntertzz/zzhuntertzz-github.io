#if USING_APPSFLYER

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
#if USING_FIREBASE
using Firebase.Analytics;
using Firebase.Crashlytics;
#endif
using Tutti.G;
using UnityEngine;

public class AppsFlyerController : SinglePrivaton<AppsFlyerController>, IAppsFlyerConversionData
{
    private string devKey = "G3MBmMRHTuEpXbqyqSWGeK";
    private string idGooglePlay => FunctionCommon.StoreUrl();
    private string idAppleStore;
    private bool isDebug;

    private string CustomerUserID
    {
        get
        {
            if (!PlayerPrefs.HasKey(nameof(CustomerUserID)))
                PlayerPrefs.SetString(nameof(CustomerUserID), GameUtils.UserId());
            return PlayerPrefs.GetString(nameof(CustomerUserID));
        }
        set => PlayerPrefs.SetString(nameof(CustomerUserID), value);
    }

    private bool IsConversionLogged
    {
        get => PlayerPrefs.GetInt(nameof(IsConversionLogged)) == 1;
        set => PlayerPrefs.SetInt(nameof(IsConversionLogged), value ? 1 : 0);
    }

    private void Start()
    {
        AppsFlyer.setCustomerUserId(CustomerUserID);
        AppsFlyer.setIsDebug(isDebug);

        AppsFlyer.OnInAppResponse += (sender, args) =>
        {
            var af_args = args as AppsFlyerRequestEventArgs;
            AppsFlyer.AFLog("AppsFlyerOnRequestResponse", " status code " + af_args.statusCode);
        };

        string appID = devKey;
#if UNITY_IOS
        appID = idAppleStore;
#elif UNITY_ANDROID
        appID = idGooglePlay;
#endif

        AppsFlyer.initSDK(devKey, appID, this);
        AppsFlyer.startSDK();
        AppsFlyerAdRevenue.start();

        if (FirebaseController.IsFirebaseReadied) SetFirebaseCustomerID();
        else FirebaseController.OnFirebaseInitialized += SetFirebaseCustomerID;
    }

    private void SetFirebaseCustomerID()
    {
#if USING_FIREBASE
        FirebaseAnalytics.SetUserId(CustomerUserID);
        Crashlytics.SetUserId(CustomerUserID);
#endif

        Debug.Log("Set customer user ID: " + CustomerUserID);
    }

    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("didReceiveConversionData", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        if (!IsConversionLogged)
        {
            Debug.Log("first conversionData: " + conversionData);
            StartCoroutine(Wait(conversionData));
        }
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }

    private IEnumerator Wait(string conversionData)
    {
#if USING_FIREBASE
        while (!FirebaseController.IsFirebaseReadied)
            yield return null;

        yield return new WaitForSeconds(3);
        if (!IsConversionLogged)
        {
            IsConversionLogged = true;
            Debug.Log("onConversionDataSuccess" + conversionData);
            var obj2 = JsonUtility.FromJson<Dictionary<string, object>>(conversionData);
            List<Parameter> parameters = new List<Parameter>();
            foreach (var keyValuePair in obj2)
                if (!(keyValuePair.Value == null || keyValuePair.Value.ToString().Length > 100))
                {
                    var strValue = keyValuePair.Value.ToString();
                    parameters.Add(new Parameter(keyValuePair.Key, strValue, GetPrior(strValue)));
                }

            var logs = parameters.OrderBy(pr => pr.order).Take(25).ToArray();
            Debug.Log(string.Join(",", logs.Select(x => x.log)));
            FirebaseAnalytics.LogEvent("af_conversion_data", logs.Select(x => x.Param).ToArray());
        }
#else
            yield return null;
#endif
    }

#if USING_FIREBASE
    private class Parameter
    {
        public Firebase.Analytics.Parameter Param;

        public int order { get; private set; }
        public string log { get; private set; }

        public Parameter(string name, string value, int order = 0)
        {
            this.order = order;
            Param = new Firebase.Analytics.Parameter(name, value);
            log = name + "=" + value;
        }
    }
#endif

    private int GetPrior(string compare)
    {
        if (compare == "click_time "
            || compare == "af_status"
            || compare == "media_source"
            || compare == "campaign_id"
            || compare == "campaign"
            || compare == "advertising_id"
            || compare == "adgroup_id"
            || compare == "is_retargeting"
            || compare == "retargeting_conversion_type"
            || compare == "engmnt_source"
            || compare == "ts"
            || compare == "channel"
            || compare == "adset"
            || compare == "adset_id"
            || compare == "ad"
            || compare == "ad_id"
            || compare == "ad_type"
            || compare == "dma")
            return 0;

        if (compare.StartsWith("af_sub")
            || compare.StartsWith("iscache")
            || compare.StartsWith("af_r")
            || compare.StartsWith("is_universal_link")
            || compare.StartsWith("af_click_lookback")
            || compare.StartsWith("is_incentivized")
        )
            return 1;

        if (compare == "clickid"
            || compare == "match_type"
            || compare == "is_branded_link"
            || compare == "af_r"
            || compare == "http_referrer "
        )
            return 2;

        return 0;
    }
}

#endif