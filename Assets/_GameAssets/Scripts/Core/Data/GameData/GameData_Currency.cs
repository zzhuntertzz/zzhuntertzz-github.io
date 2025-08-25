using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class CurrencyData
{
    public string id;
    public int start;
    public string name, desc;
}

[Serializable]
public class GameData_Currency : IDataGSheet, IInit
{
    public Dictionary<string, CurrencyData> currencyDatas = new();
    public static Dictionary<string, string> currencyNameDic = new();

    [Button]
    public void LoadGSheet()
    {
        currencyDatas.Clear();
        var table = CSVReader.ReadGSheet(
            MyKeys.GSheet.Sheet.GameData, MyKeys.GSheet.Page.Currency);
        foreach (var row in table)
        {
            var id = row["id"].Split("_")[^1];
            var data = CSVReader.Create<CurrencyData>(row);
            currencyDatas.Add(id, data);
        }
    }

    public static string GetCurrencyName(string id)
    {
        if (!currencyNameDic.ContainsKey(id)) return "";
        return currencyNameDic[id];
    }

    public void Init()
    {
        foreach (var currencyData in currencyDatas.Values)
        {
            currencyNameDic.Add(currencyData.id, currencyData.name);
        }
    }
}
    
public partial class GameData
{
    [OdinSerialize] public GameData_Currency Currency;

    public CurrencyData GetCurrencyData(string id)
    {
        return !Currency.currencyDatas.ContainsKey(id) ?
            null : Currency.currencyDatas[id];
    }
        
    public static async UniTask<bool> CheckEnoughCurrency(
        string id, int cost, Transform btnPos = null)
    {
        if (PlayerInventory.CanUse(id, cost)) return true;
        if (btnPos)
        {
            FunctionCommon.ShowNotiText(
                $"not enough {GameData.Instance.GetCurrencyData(id).name}",
                btnPos.position);
        }

        return false;
    }
}