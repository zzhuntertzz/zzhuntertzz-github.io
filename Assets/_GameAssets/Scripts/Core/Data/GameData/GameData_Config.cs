using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class ConfigData
{
    public string id;
    public object value;
    public string name, desc;
}

[Serializable]
public class GameData_Config : IDataGSheet
{
    public Dictionary<string, ConfigData> dataConfigs = new();
    
    [Button]
    public void LoadGSheet()
    {
        dataConfigs.Clear();
        var table = CSVReader.ReadGSheet(
            MyKeys.GSheet.Sheet.GameData, MyKeys.GSheet.Page.Config);
        foreach (var row in table)
        {
            var data = CSVReader.Create<ConfigData>(row);
            if (float.TryParse(data.value.ToString(), out var valueFloat))
                data.value = valueFloat;
            dataConfigs.Add(data.id, data);
        }
    }
}
    
public partial class GameData
{
    [OdinSerialize] public GameData_Config Config;

    public void GetConfig<T>(string id, out T value)
    {
        value = default(T); // Gán giá trị mặc định trướcvalue = default(T); // Gán giá trị mặc định trước
        if (Config.dataConfigs.TryGetValue(id, out ConfigData config))
        {
            if (config != null && config.value != null)
            {
                // Thử cast giá trị bên trong ConfigData.value
                if (config.value is T castedValue)
                {
                    value = castedValue;
                }
                else if (typeof(T) == typeof(float) && config.value is double doubleValue)
                {
                    value = (T)(object)(float)doubleValue;
                }
                else if (typeof(T) == typeof(string) && config.value is string stringValue)
                {
                    value = (T)(object)stringValue;
                }
                else if (typeof(T) == typeof(int) && config.value is int intValue)
                {
                    value = (T)(object)intValue;
                }
                // Thêm logic cast cho các kiểu khác nếu cần
            }
        }
    }
}