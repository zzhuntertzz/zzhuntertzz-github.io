using System;
using UnityEngine;

public class GameUtils
{
    public enum INTERNET_CONNECTION_TYPE
    {
        none = 0,
        wifi = 1,
        mobile = 2,
        other = 3,
    }

    public enum AD_PLATFORM
    {
        GoogleAdMob,
        AppLovin,
        ironsource
    }

    public enum AD_TYPE
    {
        undefined = 0,
        banner = 1,
        interstital = 2,
        rewarded_video = 3,
        app_open = 4,
        native_banner = 5,
    }

    public static string InternetConnectionType
    {
        get
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return INTERNET_CONNECTION_TYPE.none.ToString();
            else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork) return INTERNET_CONNECTION_TYPE.mobile.ToString();
            else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) return INTERNET_CONNECTION_TYPE.wifi.ToString();
            else return INTERNET_CONNECTION_TYPE.other.ToString();
        }
    }
    
    public static bool IsConnectToNetwork
    {
        get
        {
            if (InternetConnectionType == INTERNET_CONNECTION_TYPE.none.ToString())
            {
                // Debug.Log("Error. Please Check your internet connection!");
                return false;
            }
            else
            {
                // Debug.Log("Connected to Network");
                return true;
            }
        }
    }

    public static string GetAdNetworkName(string network, string defaultNetwork = "admob")
    {
        if (string.IsNullOrEmpty(network)) return defaultNetwork;

        var lower = network.ToLower();
        if (lower.Contains("admob")) return "admob";
        if (lower.Contains("max")) return "applovinmax";
        if (lower.Contains("fyber")) return "fyber";
        if (lower.Contains("appodeal")) return "appodeal";
        if (lower.Contains("inmobi")) return "inmobi";
        if (lower.Contains("vungle")) return "vungle";
        if (lower.Contains("admost")) return "admost";
        if (lower.Contains("topon")) return "topon";
        if (lower.Contains("tradplus")) return "tradplus";
        if (lower.Contains("chartboost")) return "chartboost";
        if (lower.Contains("google")) return "googleadmanager";
        if (lower.Contains("facebook") || lower.Contains("meta")) return "facebook";
        if (lower.Contains("applovin")) return "applovin";
        if (lower.Contains("ironsource")) return "ironsource";
        if (lower.Contains("unity")) return "unity";
        if (lower.Contains("mintegral")) return "mtg";

        return defaultNetwork;
    }

    public static string GenerateARMUserID()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();

        return unixTimeMilliseconds.ToString() + "-" + Helper.RandomString(24);
    }

    public static string UserId()
    {
        return SystemInfo.deviceUniqueIdentifier;
    }

    public static bool IsWeekend()
    {
        var currentDay = (int)System.DateTime.Now.DayOfWeek;
        return currentDay == 6 || currentDay == 0;
    }

    public static float GetFps()
    {
        return 1f / Time.deltaTime;
    }
}
