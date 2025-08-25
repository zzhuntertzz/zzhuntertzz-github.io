using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

public static class A
{
   private static List<string> cacheAssets = new();

   public static void UnLoadUnUseResources()
   {
       Resources.UnloadUnusedAssets();
   }
   
   public static void ClearAllCache()
   {
       foreach (var asset in cacheAssets)
       {
           AddressablesManager.ReleaseAsset(asset);
           // Debug.Log($"<<< clear cache {asset}");
       }

       UnLoadUnUseResources();
       cacheAssets.Clear();
   }

   public static async UniTask<T> Instantiate<T>(string key, Transform parent = null) where T : Object
   {
       var obj = await Get<T>(key);
       return Object.Instantiate(obj, parent);
   }

   public static async UniTask<Sprite> GetSprite(string atlasKey, string spriteKey)
   {
       SpriteAtlas atlas = await Get<SpriteAtlas>($"{atlasKey}");
       return atlas.GetSprite(spriteKey);
   }

   private static void CacheAsset(string key)
   {
       if (cacheAssets.Contains(key)) return;
       cacheAssets.Add(key);
       // Debug.Log($">>>> cache {key}");
   }
   
    public static async UniTask<T> Get<T>(string key) where T : Object
    {
        if (!AddressablesManager.ContainsKey(key) || !Application.isPlaying)
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"{key} t:{typeof(T).Name}");

            foreach (string guid in guids)
            { 
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid); 
                T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset && asset.name == key) return asset;
            }
#endif
            Debug.LogWarning($"Cannot find any addressable asset with prefab_key={key}");
            return null;
        }
        
        if (string.IsNullOrEmpty(key))
            return null;

        if (Application.isPlaying && ResourceController.initState != ResourceController.InitState.Done)
        {
            await UniTask.WaitUntil(() => ResourceController.initState == ResourceController.InitState.Done);
        }

        var type = typeof(T);
        try
        {
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                var result = await AddressablesManager.LoadAssetAsync<GameObject>(key);
                var finalResult = result.Value.GetComponent<T>();

                if (finalResult == null)
                {
                    Debug.LogError($"Cannot find any addressable asset with prefab_key={key}");
                    return null;
                }

                CacheAsset(key);
                return finalResult;
            }
            else
            {
                var result = await AddressablesManager.LoadAssetAsync<T>(key);
                CacheAsset(key);
                return result;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Cannot find any addressable asset with prefab_key={key} and Exception: " + e);
            return null;
        }
    }
}