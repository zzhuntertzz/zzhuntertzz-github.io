using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class LocalizeData
{
    public Dictionary<string, string> dicLanguage = new();

    public void AddKey(KeyValuePair<string, string> pair)
    {
        if (dicLanguage.ContainsKey(pair.Key))
        {
            Debug.LogWarning($"> key exist: {pair.Key}");
            return;
        }
        dicLanguage.Add(pair.Key, pair.Value);
    }

    public string GetKey(string language)
    {
        if (!dicLanguage.ContainsKey(language)) return "";
        return dicLanguage[language];
    }
}

[Serializable]
public class GameData_Localize : IDataGSheet, IDataPublic
{
    public static event Action<string> OnChangeLanguage = delegate { };
    
    public Dictionary<string, LocalizeData> dicLocalize = new();
    public static Dictionary<string, string> localize = new();

    [Button]
    public void LoadGSheet()
    {
        dicLocalize.Clear();
        var table = CSVReader.ReadGSheet(
            MyKeys.GSheet.Sheet.GameData, MyKeys.GSheet.Page.Localize);
        foreach (var row in table)
        {
            var key = row["key"];
            var data = new LocalizeData();
            foreach (var pair in row)
            {
                data.AddKey(pair);
            }
            dicLocalize.Add(key, data);
        }
    }

    public void ChangeLanguage(string language)
    {
        localize.Clear();
        foreach (var pair in dicLocalize)
        {
            localize.Add(pair.Key, pair.Value.GetKey(language));
        }

        PlayerData.PlayerLanguage.language = language;
        PlayerData.PlayerLanguage.Save();
        OnChangeLanguage(language);
    }

    public static string GetKey(string key)
    {
        if (!localize.ContainsKey(key) || string.IsNullOrEmpty(key))
        {
            Debug.LogWarning($"> empty localize key {key}");
            return "";
        }
        return localize[key];
    }

    public void InitData()
    {
        ChangeLanguage(PlayerData.PlayerLanguage.language);
    }
}

public partial class GameData
{
    [OdinSerialize] public GameData_Localize Localize;

    public List<string> GetAllLanguageSupport()
    {
        return Localize.dicLocalize.Values.First().dicLanguage.Keys
            .Where(x => !string.IsNullOrEmpty(x) && x != "key").ToList();
    }
}