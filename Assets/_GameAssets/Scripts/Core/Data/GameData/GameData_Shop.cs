using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[Serializable]
public class ShopData : RewardData
{
    public enum ShopItemType
    {
        FREE, PRICE, AD, IAP
    }
    
    public string ico, title, desc, priceId;

    public ShopItemType GetItemType()
    {
        var tmpPrice = 0;
        if (!int.TryParse(priceId, out tmpPrice))
        {
            if (priceId == "ad")
                return ShopItemType.AD;
            return ShopItemType.IAP;
        }
        if (tmpPrice > 0)
        {
            return ShopItemType.PRICE;
        }
        return ShopItemType.FREE;
    }
    
    public bool IsIAP()
    {
        return GetItemType() == ShopItemType.IAP;
    }

    public bool IsValid()
    {
        return true;
    }
}

[Serializable]
public class GameData_Shop : IDataGSheet
{
    public Dictionary<int, ShopData> shopDatas = new();

    [Button]
    public void LoadGSheet()
    {
        LoadShopData();
    }

    public void LoadShopData()
    {
        shopDatas.Clear();
        var table = CSVReader.ReadGSheet(
            MyKeys.GSheet.Sheet.GameData, MyKeys.GSheet.Page.ShopIAP);
        foreach (var row in table)
        {
            var id = int.Parse(row["_id"]);
            if (id == 0) continue;
            var shopData = CSVReader.Create<ShopData>(row);
            shopDatas.Add(id, shopData);
        }
    }

    public List<ShopData> GetShopData()
    {
        return shopDatas.Values.Where(x => x.IsValid()).ToList();
    }
}
    
public partial class GameData
{
    [OdinSerialize] public GameData_Shop Shop;
}