using System;
using UnityEngine;

public static partial class GameFunction
{
    public static readonly int ID_BASE = 10000;
    
    public static readonly string PRODUCT_NO_AD = "no_ad";
    
    public static void GetID(this int id, out int type, out int typeId)
    {
        type = id / ID_BASE;
        typeId = id % ID_BASE;
    }

    public static void ShowReward(string placement, Action GetReward, Vector2 notiPos)
    {
        if (!GameUtils.IsConnectToNetwork)
        {
            FunctionCommon.ShowNotiText("no internet connection", notiPos);
            return;
        }
        AdvertiseManager.ShowVideoAds(placement, GetReward, delegate
        {
            FunctionCommon.ShowNotiText("no video ad available", notiPos);
        });
    }

    public static void ShowMrec()
    {
        AdvertiseManager.CheckShowMRec();
        AdvertiseManager.HideBanner();
    }
    
    public static void HideMrec()
    {
        AdvertiseManager.HideMRec();
        AdvertiseManager.CheckShowBannerBottom();
    }
}