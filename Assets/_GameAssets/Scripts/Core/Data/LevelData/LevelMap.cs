using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

public class LevelMap : SinglePrivaton<LevelMap>, ISetData
{
    private string Path => $"Assets/_GameAssets/_AddressableAssets/Data/Level/";
    
#if UNITY_EDITOR
    [Button]
    private void SaveMap()
    {
        var map = transform.GetChild(0).gameObject;
        var mapName = map.name;
        var data = ScriptableObjectCreator.CreateAsset<LevelMapSO>(Path, $"Level {mapName}");
        data.SaveData(map);
    }
#endif
    
    [Button]
    public async UniTask LoadMap(string mapName)
    {
        var data = await A.Get<LevelMapSO>($"Level {mapName}");
        await data.LoadData(transform);
    }

    public async void SetData(object obj)
    {
        if (obj is not string json) return;
        
        SetupMap();
    }

    void SetupMap()
    {
        ResourceController.Instance.AddQueue(async delegate
        {
        });
    }
}