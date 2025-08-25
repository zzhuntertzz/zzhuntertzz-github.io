using System.Collections.Generic;
#if USING_FIREBASE
using Firebase;
using Firebase.RemoteConfig;
using Firebase.Extensions;
#endif

public class PlayerRemoteData : BasePlayerData<PlayerRemoteData>
{
    public int booster_time = 60,
        rate = 7,
        delay_load_admob,
        delay_load_max,
        unlock_event_level = 9999,
        unlock_screw_after_level = 5;
        // , unlock_daily_reward = 5;
    public bool internetRequire;
    public string remove_ad_price = "no_ad";
    public List<int> lstUnlockScrewLevel = new();
    public List<int> lstUnlockBoardEventLevel = new();
    public List<int> lstRemoveAdLevel = new();
    public List<int> lstRemoveAd2Level = new();
    public Dictionary<int, List<int>> dailyReward = new();
}

public static partial class PlayerData
{
    public static PlayerRemoteData PlayerRemoteData = new();
}

public static class RemoteConfigManager
{
    public static readonly string KEY_INTERS_SHOW_AT_LEVEL = "inter_show_at_level";
    public static readonly string Key_INTERS_DELAY_SHOW = "inter_delay_show";
    public static readonly string KEY_INTERS_LEVEL_SHOW_RATE = "inter_level_show_rate";
    public static readonly string KEY_USE_AOA = "open_ad_use";
    public static readonly string KEY_BOOSTER_TIME = "booster_time";
    public static readonly string KEY_RATE = "rate";
    public static readonly string KEY_INTERNET_REQUIRE = "internet_require";
    public static readonly string KEY_DELAY_FAIL_ADMOB = "delay_fail_admob";
    public static readonly string KEY_DELAY_FAIL_MAX = "delay_fail_max";
    public static readonly string KEY_UNLOCK_SCREW_LEVELS = "unlock_screw_levels";
    public static readonly string KEY_UNLOCK_SCREW_AFTER_LEVEL = "unlock_screw_after_level";
    // public static readonly string KEY_UNLOCK_BOARD_EVENT_LEVELS = "unlock_board_event_levels";
    public static readonly string KEY_REMOVE_AD_LEVELS = "remove_ad_levels";
    public static readonly string KEY_REMOVE_AD_2_LEVELS = "remove_ad_2_levels";
    // public static readonly string KEY_UNLOCK_EVENT_LEVEL = "unlock_event_level";
    // public static readonly string KEY_UNLOCK_DAILY_REWARD = "unlock_daily_reward";
    public static readonly string KEY_REMOVE_AD_PRICE = "remove_ad_price";
    public static string KEY_DAY_X(int day) => $"day_{day}";
    
#if USING_FIREBASE

#endif

    public static void Init()
    {
#if USING_FIREBASE
        InitSuccess();
#endif
    }
    
#if USING_FIREBASE
    public static ConfigValue GetValue(string key)
    {
        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        if (remoteConfig is null) return default;
        var value = remoteConfig.GetValue(key);
        return value;
    }
#else
    public class ConfigValue
    {
        public bool BooleanValue;
        public long LongValue;
        public double DoubleValue;
        public string StringValue;
    }
    public static ConfigValue GetValue(string key)
    {
        return new();
    }
#endif


#if USING_FIREBASE
    private static void InitSuccess()
    {
        var defaults = new Dictionary<string, object>();
        defaults.Add(KEY_INTERS_SHOW_AT_LEVEL, 2);
        defaults.Add(Key_INTERS_DELAY_SHOW, 60);
        defaults.Add(KEY_INTERS_LEVEL_SHOW_RATE, 3);
        defaults.Add(KEY_USE_AOA, true);
        defaults.Add(KEY_BOOSTER_TIME, 10);
        defaults.Add(KEY_RATE, 5);
        defaults.Add(KEY_DELAY_FAIL_ADMOB, 5);
        defaults.Add(KEY_DELAY_FAIL_MAX, 5);
        defaults.Add(KEY_INTERNET_REQUIRE, true);
        defaults.Add(KEY_UNLOCK_SCREW_LEVELS, "5,15,25");
        defaults.Add(KEY_UNLOCK_SCREW_AFTER_LEVEL, 5);
        defaults.Add(KEY_REMOVE_AD_LEVELS, "6");
        defaults.Add(KEY_REMOVE_AD_2_LEVELS, "12");
        // defaults.Add(KEY_UNLOCK_DAILY_REWARD, 5);
        defaults.Add(KEY_DAY_X(1), "10002");
        defaults.Add(KEY_DAY_X(2), "10003");
        defaults.Add(KEY_DAY_X(3), "10004");
        defaults.Add(KEY_DAY_X(4), "10005");
        defaults.Add(KEY_DAY_X(5), "10006");
        defaults.Add(KEY_DAY_X(6), "10007");
        defaults.Add(KEY_DAY_X(7), "10008,10009,10010");
        defaults.Add(KEY_REMOVE_AD_PRICE, "no_ad");
        
        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        remoteConfig.SetDefaultsAsync(defaults).ContinueWithOnMainThread(
            task =>
            {
                FetchRemoteConfig(OnSuccess);
            }
        );
    }

    private static void OnSuccess()
    {
        AppOpenAdManager.ConfigOpenApp = GetValue(KEY_USE_AOA).BooleanValue;
        
        PlayerData.PlayerRemoteData.remove_ad_price = GetValue(KEY_REMOVE_AD_PRICE).StringValue;
        PlayerData.PlayerRemoteData.booster_time = (int) GetValue(KEY_BOOSTER_TIME).LongValue;
        PlayerData.PlayerRemoteData.rate = (int) GetValue(KEY_RATE).LongValue;
        PlayerData.PlayerRemoteData.internetRequire = GetValue(KEY_INTERNET_REQUIRE).BooleanValue;
        PlayerData.PlayerRemoteData.delay_load_admob = (int)(GetValue(KEY_DELAY_FAIL_ADMOB).LongValue * 1000);
        PlayerData.PlayerRemoteData.delay_load_max = (int)(GetValue(KEY_DELAY_FAIL_MAX).LongValue * 1000);
        PlayerData.PlayerRemoteData.unlock_screw_after_level = (int)(GetValue(KEY_UNLOCK_SCREW_AFTER_LEVEL).LongValue);
        // PlayerData.PlayerRemoteData.unlock_daily_reward = (int) GetValue(KEY_UNLOCK_DAILY_REWARD).LongValue;
        try
        {
            var strLstSkinLevels = GetValue(KEY_UNLOCK_SCREW_LEVELS).StringValue;
            PlayerData.PlayerRemoteData.lstUnlockScrewLevel = 
                strLstSkinLevels.Split(',')
                    .Select(x => int.Parse(x)).ToList();
        }
        catch (Exception e)
        {
            Debug.LogError($"> convert skin level fail");
        }
        try
        {
            var strLstRemoveAdLevels = GetValue(KEY_REMOVE_AD_LEVELS).StringValue;
            PlayerData.PlayerRemoteData.lstRemoveAdLevel = 
                strLstRemoveAdLevels.Split(',')
                    .Select(x => int.Parse(x)).ToList();
        }
        catch (Exception e)
        {
            Debug.LogError($"> convert remove ad level fail");
        }
        try
        {
            var strLstRemoveAdLevels = GetValue(KEY_REMOVE_AD_2_LEVELS).StringValue;
            PlayerData.PlayerRemoteData.lstRemoveAd2Level = 
                strLstRemoveAdLevels.Split(',')
                    .Select(x => int.Parse(x)).ToList();
        }
        catch (Exception e)
        {
            Debug.LogError($"> convert remove ad 2 level fail");
        }
        try
        {
            PlayerData.PlayerRemoteData.dailyReward.Clear();
            
            var strDay1 = GetValue(KEY_DAY_X(1)).StringValue;
            var day1 = strDay1.Split(',')
                .Select(x => int.Parse(x)).ToList(); 
            PlayerData.PlayerRemoteData.dailyReward.Add(1, day1);
            
            var strDay2 = GetValue(KEY_DAY_X(2)).StringValue;
            var day2 = strDay2.Split(',')
                .Select(x => int.Parse(x)).ToList(); 
            PlayerData.PlayerRemoteData.dailyReward.Add(2, day2);
            
            var strDay3 = GetValue(KEY_DAY_X(3)).StringValue;
            var day3 = strDay3.Split(',')
                .Select(x => int.Parse(x)).ToList(); 
            PlayerData.PlayerRemoteData.dailyReward.Add(3, day3);
            
            var strDay4 = GetValue(KEY_DAY_X(4)).StringValue;
            var day4 = strDay4.Split(',')
                .Select(x => int.Parse(x)).ToList(); 
            PlayerData.PlayerRemoteData.dailyReward.Add(4, day4);
            
            var strDay5 = GetValue(KEY_DAY_X(5)).StringValue;
            var day5 = strDay5.Split(',')
                .Select(x => int.Parse(x)).ToList(); 
            PlayerData.PlayerRemoteData.dailyReward.Add(5, day5);
            
            var strDay6 = GetValue(KEY_DAY_X(6)).StringValue;
            var day6 = strDay6.Split(',')
                .Select(x => int.Parse(x)).ToList(); 
            PlayerData.PlayerRemoteData.dailyReward.Add(6, day6);
            
            var strDay7 = GetValue(KEY_DAY_X(7)).StringValue;
            var day7 = strDay7.Split(',')
                .Select(x => int.Parse(x)).ToList(); 
            PlayerData.PlayerRemoteData.dailyReward.Add(7, day7);
                
        }
        catch (Exception e)
        {
            Debug.LogError($"> convert skin board level event fail");
        }
        
        PlayerData.PlayerRemoteData.Save();
    }
    
    private static void FetchRemoteConfig(Action onFetchAndActivateSuccessful = null)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            Debug.LogError(
                $"Do not use Firebase until it is properly initialized");
            return;
        }

        Debug.Log("Fetching data...");
        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        remoteConfig.FetchAsync(System.TimeSpan.Zero).ContinueWithOnMainThread(
            previousTask=>
            {
                if (!previousTask.IsCompleted)
                {
                    Debug.LogError($"{nameof(remoteConfig.FetchAsync)} incomplete: Status '{previousTask.Status}'");
                    return;
                }
                ActivateRetrievedRemoteConfigValues(onFetchAndActivateSuccessful);
            });
    }
    
    private static void ActivateRetrievedRemoteConfigValues(Action onFetchAndActivateSuccessful = null)
    {
        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        var info = remoteConfig.Info;
        if (info.LastFetchStatus == LastFetchStatus.Success)
        {
            remoteConfig.ActivateAsync().ContinueWithOnMainThread(
                previousTask =>
                {
                    Debug.Log($"Remote data loaded and ready (last fetch time {info.FetchTime}).");
                    onFetchAndActivateSuccessful?.Invoke();
                });
        }
    }
#endif
}