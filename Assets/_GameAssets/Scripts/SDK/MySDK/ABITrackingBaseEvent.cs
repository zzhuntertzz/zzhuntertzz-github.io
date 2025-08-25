#if USING_FIREBASE
using Firebase.Analytics;
#endif

#if USING_IAP
using UnityEngine.Purchasing;
#endif

#if USING_FIREBASE

public class ABIEventCheckPoint : FirebaseTrackingEvent
{
    protected override string EventName => $"checkpoint_{MyTracking.CurrentLevel}";
    protected override void SetupDefaultParameters()
    {
    }
}

public class ABIEventLevelStart : FirebaseTrackingEvent
{
    protected override string EventName => $"level_start";
    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
        AddParameter("current_gold", PlayerData.PlayerInventory
            .GetQuantity(GameFunction.ID_CURRENCY_GOLD));
    }
}

public class ABIEventLevelComplete : FirebaseTrackingEvent
{
    protected override string EventName => $"level_complete";
    public int timeplayed;
    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
        AddParameter("timeplayed", timeplayed);
    }
}

public class ABIEventLevelFail : FirebaseTrackingEvent
{
    protected override string EventName => $"level_fail";
    protected override void SetupDefaultParameters()
    {
        AddParameter("level", MyTracking.CurrentLevel);
        AddParameter("failcount", PlayerData.PlayerInfo.loseCount);
    }
}

public class ABIEventEarnCurrency : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseAnalytics.EventEarnVirtualCurrency;
    public string name, source;
    public int value;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter(FirebaseAnalytics.ParameterVirtualCurrencyName, name);
        AddParameter("value", value);
        AddParameter("source", source);
    }
}

public class ABIEventSpendCurrency : FirebaseTrackingEvent
{
    protected override string EventName => FirebaseAnalytics.EventSpendVirtualCurrency;
    public string name, source;
    public int value;
    
    protected override void SetupDefaultParameters()
    {
        AddParameter(FirebaseAnalytics.ParameterVirtualCurrencyName, name);
        AddParameter("value", value);
        AddParameter("source", source);
    }
}

#endif

#if USING_APPSFLYER

public class ABIEventAFCheckPoint : GameAppsFlyerEvent
{
    public override string EventName => $"checkpoint_{MyTracking.CurrentLevel}";
    protected override void SetupDefaultParameters()
    {
    }
}

public class ABIEventAFPurchase : GameAppsFlyerEvent
{
    public override string EventName => "af_purchase";
#if USING_IAP
      
    public Product product;

    protected override void SetupDefaultParameters()
    {
        AddParameter("af_revenue", product.metadata.localizedPrice);
        AddParameter("af_currency", product.metadata.isoCurrencyCode);
        AddParameter("af_quantity", 1);
        AddParameter("af_content_id", product.definition.id);
        AddParameter("af_customer_user_id", GameUtils.UserId());
    }

#endif
}

public class ABIEventAFAdInterEligible : GameAppsFlyerEvent
{
    public override string EventName => "af_inters_ad_eligible";

    protected override void SetupDefaultParameters()
    {
    }
}

public class ABIEventAFAdInterCall : GameAppsFlyerEvent
{
    public override string EventName => "af_inters_api_called";

    protected override void SetupDefaultParameters()
    {
    }
}

public class ABIEventAFAdInterShow : GameAppsFlyerEvent
{
    public override string EventName => "af_inters_displayed";

    protected override void SetupDefaultParameters()
    {
    }
}

public class ABIEventAFAdRewardEligible : GameAppsFlyerEvent
{
    public override string EventName => "af_rewarded_ad_eligible";

    protected override void SetupDefaultParameters()
    {
    }
}

public class ABIEventAFAdRewardCall : GameAppsFlyerEvent
{
    public override string EventName => "af_rewarded_api_called";

    protected override void SetupDefaultParameters()
    {
    }
}

public class ABIEventAFAdRewardShow : GameAppsFlyerEvent
{
    public override string EventName => "af_rewarded_displayed";

    protected override void SetupDefaultParameters()
    {
    }
}

public class ABIEventAFAdRewardComplete : GameAppsFlyerEvent
{
    public override string EventName => "af_rewarded_ad_completed";

    protected override void SetupDefaultParameters()
    {
    }
}

#endif
