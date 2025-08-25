#if USING_FIREBASE
using System;
using System.Globalization;
using Tutti.G;
using Firebase.Analytics;
#if USING_IAP
using UnityEngine.Purchasing;
#endif

public enum FirebaseEventName
{
    StartFirst,
    StartFirstEvent,
    Start,
    StartEvent,
    StartBoss,
    StartBossEvent,
    Win,
    WinEvent,
    WinBoss,
    WinBossEvent,
    Lose,
    LoseEvent,
    LoseBoss,
    LoseBossEvent,
    Continue,
    
    Rate,
    WatchSkinPop,
    WatchSkinNail,
    
    Booster_Hint,
    Booster_Time,
    Booster_Skip,
    Booster_Remove_Nail,
    
    Booster_Hint_Boss,
    Booster_Time_Boss,
    Booster_Skip_Boss,
    Booster_Remove_Nail_Boss,
    
    ad_reward_load,
    ad_reward_load_success,
    ad_reward_load_failed,
    ad_reward_show,
    ad_reward_show_success,
    ad_reward_show_failed,
    ad_reward_close,
    ad_interstitial_load,
    ad_interstitial_load_success,
    ad_interstitial_load_failed,
    ad_interstitial_show,
    ad_interstitial_show_success,
    ad_interstitial_show_failed,
    ad_interstitial_close,
    
    iap_click,
    iap_success,
    iap_fail,
    
    OnInternet,
    OffInternet,
}

public enum AppFlyerEventName
{
}

public enum FirebaseUserProperty
{
    level,
    highest_level_played,
    paying_type,
    retent_type,
    days_played,
}


[Serializable]
public class EventContinue : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.Continue.ToString();

    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
    }
}

[Serializable]
public class EventBoosterHint : FirebaseTrackingEvent
{
    protected override string EventName
    {
        get
        {
            if (PlayerData.PlayerDataTemp.bossLevel)
            {
                return FirebaseEventName.Booster_Hint_Boss.ToString();
            }
            else
            {
                return FirebaseEventName.Booster_Hint.ToString();
            }
        }
    }

    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
    }
}
[Serializable]
public class EventBoosterTime : FirebaseTrackingEvent
{
    protected override string EventName
    {
        get
        {
            if (PlayerData.PlayerDataTemp.bossLevel)
            {
                return FirebaseEventName.Booster_Time_Boss.ToString();
            }
            else
            {
                return FirebaseEventName.Booster_Time.ToString();
            }
        }
    }

    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
    }
}
[Serializable]
public class EventBoosterSkip : FirebaseTrackingEvent
{
    protected override string EventName
    {
        get
        {
            if (PlayerData.PlayerDataTemp.bossLevel)
            {
                return FirebaseEventName.Booster_Skip_Boss.ToString();
            }
            else
            {
                return FirebaseEventName.Booster_Skip.ToString();
            }
        }
    }
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
    }
}
[Serializable]
public class EventBoosterRemoveNail : FirebaseTrackingEvent
{
    protected override string EventName
    {
        get
        {
            if (PlayerData.PlayerDataTemp.bossLevel)
            {
                return FirebaseEventName.Booster_Remove_Nail_Boss.ToString();
            }
            else
            {
                return FirebaseEventName.Booster_Remove_Nail.ToString();
            }
        }
    }

    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
    }
}

[Serializable]
public class EventLevelStartFirst : FirebaseTrackingEvent
{
    protected override string EventName => PlayerData.PlayerDataTemp.levelMode == 0 ?
        FirebaseEventName.StartFirst.ToString() : FirebaseEventName.StartFirstEvent.ToString();

    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
    }
}

[Serializable]
public class EventLevelStart : FirebaseTrackingEvent
{
    protected override string EventName
    {
        get
        {
            if (PlayerData.PlayerDataTemp.bossLevel)
            {
                return PlayerData.PlayerDataTemp.levelMode == 0 ?
                    FirebaseEventName.StartBoss.ToString() : FirebaseEventName.StartBossEvent.ToString();
            }
            else
            {
                return PlayerData.PlayerDataTemp.levelMode == 0 ?
                    FirebaseEventName.Start.ToString() : FirebaseEventName.StartEvent.ToString();
            }
        }
    }

    protected override void SetupDefaultParameters()
    {
        FirebaseAnalytics.SetUserProperty(FirebaseUserProperty.level.ToString(),
            MyTracking.CurrentLevel);
        FirebaseAnalytics.SetUserProperty(FirebaseUserProperty.highest_level_played.ToString(),
            MyTracking.HighestLevel);
        FirebaseAnalytics.SetUserProperty(FirebaseUserProperty.retent_type.ToString(),
            PlayerData.PlayerDaily.retention.ToString());
        FirebaseAnalytics.SetUserProperty(FirebaseUserProperty.days_played.ToString(),
            PlayerData.PlayerDaily.GetDaysPlayed().ToString());
        FirebaseAnalytics.SetUserProperty(FirebaseUserProperty.paying_type.ToString(),
            PlayerData.PlayerShoppingData.payType.ToString());

        AddParameter("level", MyTracking.CurrentLevel);
    }
}

[Serializable]
public class EventLevelWin : FirebaseTrackingEvent
{
    protected override string EventName
    {
        get
        {
            if (PlayerData.PlayerDataTemp.bossLevel)
            {
                return PlayerData.PlayerDataTemp.levelMode == 0 ?
                    FirebaseEventName.WinBoss.ToString() : FirebaseEventName.WinBossEvent.ToString();
            }
            else
            {
                return PlayerData.PlayerDataTemp.levelMode == 0 ?
                    FirebaseEventName.Win.ToString() : FirebaseEventName.WinEvent.ToString();
            }
        }
    }

    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
    }
}

[Serializable]
public class EventLevelLose : FirebaseTrackingEvent
{
    protected override string EventName
    {
        get
        {
            if (PlayerData.PlayerDataTemp.bossLevel)
            {
                return PlayerData.PlayerDataTemp.levelMode == 0 ?
                    FirebaseEventName.LoseBoss.ToString() : FirebaseEventName.LoseBossEvent.ToString();
            }
            else
            {
                return PlayerData.PlayerDataTemp.levelMode == 0 ?
                    FirebaseEventName.Lose.ToString() : FirebaseEventName.LoseEvent.ToString();
            }
        }
    }

    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
    }
}


[Serializable]
public class EventRewardLoad : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_reward_load.ToString();
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
    }
}

[Serializable]
public class EventRewardLoadSuccess : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_reward_load_success.ToString();
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
    }
}

[Serializable]
public class EventRewardLoadFailed : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_reward_load_failed.ToString();
    public string ad_reward_load_failed_reason;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
        AddParameter(nameof(ad_reward_load_failed_reason), ad_reward_load_failed_reason);
    }
}

[Serializable]
public class EventRewardShow : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_reward_show.ToString();
    public string placement;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
        AddParameter("placement", placement);
    }
}

[Serializable]
public class EventRewardShowSuccess : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_reward_show_success.ToString();
    public string ad_reward_position, session_counter;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
        AddParameter(nameof(ad_reward_position), ad_reward_position);
        AddParameter(nameof(session_counter), session_counter);
    }
}

[Serializable]
public class EventRewardShowSuccessCount : FirebaseTrackingEvent
{
    protected override string EventName => $"rewarded_{PlayerData.PlayerTracking.adRewardCount:0000}";
    
    protected override void SetupDefaultParameters()
    {
    }
}

[Serializable]
public class EventAFRewardShowSuccessCount : GameAppsFlyerEvent
{
    public override string EventName => $"rewarded_{PlayerData.PlayerTracking.adRewardCount:0000}";

    protected override void SetupDefaultParameters()
    {
    }
}

[Serializable]
public class EventRewardShowFailed : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_reward_show_failed.ToString();
    public string ad_reward_show_failed_reason;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
        AddParameter(nameof(ad_reward_show_failed_reason), ad_reward_show_failed_reason);
    }
}

[Serializable]
public class EventRewardClose : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_reward_close.ToString();
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
    }
}

[Serializable]
public class EventInterstitialLoad : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_interstitial_load.ToString();
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
    }
}

[Serializable]
public class EventInterstitialLoadSuccess : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_interstitial_load_success.ToString();
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
    }
}

[Serializable]
public class EventInterstitialLoadFailed : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_interstitial_load_failed.ToString();
    public string ad_inter_load_failed_reason;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
        AddParameter(nameof(ad_inter_load_failed_reason), ad_inter_load_failed_reason);
    }
}

[Serializable]
public class EventInterstitialShow : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_interstitial_show.ToString();
    public string ad_inter_position, session_counter;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter(nameof(ad_inter_position), ad_inter_position);
        AddParameter(nameof(session_counter), session_counter);
    }
}

[Serializable]
public class EventInterstitialShowSuccess : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_interstitial_show_success.ToString();
    
    protected override void SetupDefaultParameters()
    {
    }
}

[Serializable]
public class EventInterstitialShowSuccessCount : FirebaseTrackingEvent
{
    protected override string EventName => $"inter_{PlayerData.PlayerTracking.adInterCount:0000}";
    
    protected override void SetupDefaultParameters()
    {
    }
}

[Serializable]
public class EventAFInterstitialShowSuccessCount : GameAppsFlyerEvent
{
    public override string EventName => $"inter_{PlayerData.PlayerTracking.adInterCount:0000}";

    protected override void SetupDefaultParameters()
    {
    }
}

[Serializable]
public class EventInterstitialShowFailed : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_interstitial_show_failed.ToString();
    public string ad_interstitial_show_failed_reason;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("fps", GameUtils.GetFps().ToString(CultureInfo.InvariantCulture));
        AddParameter(nameof(ad_interstitial_show_failed_reason), ad_interstitial_show_failed_reason);
    }
}

[Serializable]
public class EventInterstitialClose : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.ad_interstitial_close.ToString();
    
    protected override void SetupDefaultParameters()
    {
    }
}

#if USING_IAP
[Serializable]
public class EventProductPurchaseClick : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.iap_click.ToString();
    public string product_id;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("product_id", product_id);
    }
}

[Serializable]
public class EventProductPurchaseSuccess : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.iap_success.ToString();
    public Product product;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("product_id", product.definition.id);
    }
}

[Serializable]
public class EventProductPurchaseFail : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.iap_fail.ToString();
    public Product product;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("product_id", product.definition.id);
    }
}
#endif


[Serializable]
public class EventRate : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.Rate.ToString();
    public int star;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("star", star);
    }
}

[Serializable]
public class EventWatchSkinPop : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.WatchSkinPop.ToString();
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
    }
}

[Serializable]
public class EventWatchSkinNail : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseEventName.WatchSkinNail.ToString();
    
    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
    }
}

#endif