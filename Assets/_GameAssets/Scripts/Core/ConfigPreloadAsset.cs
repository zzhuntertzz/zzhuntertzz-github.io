using Cysharp.Threading.Tasks;
using UnityEngine;

public class ConfigPreloadAsset : SinglePrivaton<ConfigPreloadAsset>
{
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        Debug.Log($">>Init Data {ConfigPreloadAsset.Instance}");
    }
}