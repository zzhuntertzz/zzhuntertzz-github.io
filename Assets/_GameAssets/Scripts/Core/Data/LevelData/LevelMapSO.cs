using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Searchable]
[CreateAssetMenu(fileName = "LevelMapSO", menuName = "ScriptableObjects/LevelMapSO")]
public class LevelMapSO : ScriptableObject
{
    public string mapName;
    [SerializeReference]
    public List<LevelSavableData> lstData;

    public void SaveData(GameObject mapObj)
    {
#if UNITY_EDITOR
        mapName = mapObj.name;
        var lstSavable = mapObj.GetComponentsInChildren<ISavable>();
        lstData = new();
        foreach (var savable in lstSavable)
        {
            // if (savable is Type savableType)
            //     lstData.Add(savableType.GetData<Type>(savableType.gameObject));
        }
        
        // Mark the ScriptableObject as dirty
        EditorUtility.SetDirty(this);

        // Explicitly save the asset
        string assetPath = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogError("SaveData: ScriptableObject is not associated with an asset file. Please save it first.");
            return;
        }

        AssetDatabase.SaveAssetIfDirty(this);
        Debug.Log($"SaveData: Saved {mapName} to {assetPath} with {lstData.Count} items.");

        // Optional: Force a refresh only if necessary (avoid unless needed)
        AssetDatabase.Refresh();
#endif
    }

    public async UniTask LoadData(Transform parent = null)
    {
        var mapObj = new GameObject(mapName).transform;
        mapObj.SetParent(parent);
        mapObj.localScale = Vector3.one;
        var lstTask = new List<LevelSavableData>();
        var dicGoParent = new Dictionary<GameObject, string>();
        lstData.ForEach(async x =>
        {
            lstTask.Add(x);
            
            GameObject go = default;
            switch (x)
            {
                case LevelSavableSpriteRenderer saveSpr:
                    go = new GameObject(saveSpr.name);
                    break;
                
                default:
                    go = new GameObject(x.name);
                    break;
            }

            go.transform.SetParent(mapObj);
            go.transform.position = x.position;
            go.transform.rotation = x.rotation;
            go.transform.localScale = x.scale;
            
            if (x is LevelSavableSpriteRenderer dataSpr)
            {
                var sr = go.GetOrAddComponent<SpriteRenderer>();
                sr.sprite = await A.Get<Sprite>(dataSpr.spriteName);
                sr.color = dataSpr.color;
            }
            
            lstTask.Remove(x);
            dicGoParent.Add(go, x.parentName);
        });
        await UniTask.Delay(30, DelayType.DeltaTime);
        await UniTask.WaitUntil(() => lstTask.Count == 0);
        foreach (var keyValuePair in dicGoParent)
        {
            var go = keyValuePair.Key;
            var goParent = mapObj.Find(keyValuePair.Value);
            if (goParent) go.transform.SetParent(goParent.transform);
            else go.transform.SetParent(mapObj);
        }
    }
}