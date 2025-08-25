using System;
using System.Collections.Generic;

public class PlayerShoppingData : BasePlayerData<PlayerShoppingData>
{
    public static event Action onRemoveAd = delegate { };
    
    public List<string> lstProductPurchased = new();
    public float paidValue = 0;
    public int payType = 0;

    public bool IsAdRemoved()
    {
        return HasProduct(GameFunction.PRODUCT_NO_AD);
    }

    public void AddProduct(string id)
    {
        if (lstProductPurchased.Contains(id)) return;

        if (id.Contains(GameFunction.PRODUCT_NO_AD))
        {
            id = GameFunction.PRODUCT_NO_AD;
            AdvertiseManager.HideBanner();
            onRemoveAd();
        }

        PurchaseProduct(id);
        lstProductPurchased.Add(id);
        Save();
    }

    public void PurchaseProduct(string id)
    {
        var catalogItem = MyIAPManager.GetCatalogItem(id);
        if (catalogItem is not null)
        {
            paidValue += (float)catalogItem.googlePrice.value;
            payType = paidValue switch
            {
                >= 50 => 50,
                >= 20 => 20,
                >= 10 => 10,
                >= 5 => 5,
                >= 2 => 2,
                _ => payType
            };
        }
        Save();
    }

    public bool HasProduct(string id)
    {
        return lstProductPurchased.Contains(id);
    }
}

public static partial class PlayerData
{
    public static PlayerShoppingData PlayerShoppingData = new();
}