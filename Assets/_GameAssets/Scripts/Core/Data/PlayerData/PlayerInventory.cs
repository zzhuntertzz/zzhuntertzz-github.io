using System;
using System.Collections.Generic;

[Serializable]
public class PlayerInventory : BasePlayerData<PlayerInventory>
{
    public static event Action<string, int, int> OnQuantityChanged = delegate { };

    public Dictionary<string, int> itemDic = new();

    public static bool CanUse(string id, int quantity)
    {
        var isCan = PlayerData.PlayerInventory.GetQuantity(id) >= quantity;
        return isCan;
    }

    public bool HasItem(string id)
    {
        return GetQuantity(id) > 0;
    }

    public int GetQuantity(string id)
    {
        if (itemDic.ContainsKey(id))
            return itemDic[id];
        itemDic.Add(id, 0);
        return 0;
    }

    public void SetQuantity(string id, int quantity)
    {
        if (itemDic.ContainsKey(id))
            itemDic[id] = quantity;
        else
            itemDic.Add(id, 0);
    }

    public void SubQuantity(string id, int value = 1, string source = "")
    {
        var currentQuantity = GetQuantity(id);
        currentQuantity -= value;
    
        itemDic[id] = currentQuantity;
    
        OnQuantityChanged(id, currentQuantity, value);
    
        Save();

        if (!string.IsNullOrEmpty(source) && value != 0)
        {
            // new ABIEventSpendCurrency()
            // {
            //     name = DCurrency.GetCurrencyName(id),
            //     value = value,
            //     source = source,
            // }.Post();
        }
    }

    public async void AddQuantity(string id, int value = 1, string source = "")
    {
        var currentQuantity = GetQuantity(id);
        currentQuantity += value;

        // var data = await D.GetInstance();
        // if (data)
        // {
            // var limit = (int)data.GetCurrencyData(id).limit;
            // if (limit != 0 && currentQuantity > limit)
            //     currentQuantity = limit;
        // }

        itemDic[id] = currentQuantity;
    
        OnQuantityChanged(id, currentQuantity, value);
    
        Save();
        
        if (!string.IsNullOrEmpty(source) && value != 0)
        {
            // new ABIEventEarnCurrency()
            // {
            //     name = DCurrency.GetCurrencyName(id),
            //     value = value,
            //     source = source,
            // }.Post();
        }
    }
}

public static partial class PlayerData
{
    public static PlayerInventory PlayerInventory;
}