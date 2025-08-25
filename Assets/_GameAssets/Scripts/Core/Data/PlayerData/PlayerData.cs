public class BasePlayerData<T>
{
    public void Save()
    {
        this.SaveData<T>();
    }
}

public static partial class PlayerData
{
    static PlayerData()
    {
        Load();
    }

    public static void Load()
    {
        PlayerInventory = FunctionCommon.LoadData<PlayerInventory>();
        PlayerSetting = FunctionCommon.LoadData<PlayerSetting>();
        PlayerInfo = FunctionCommon.LoadData<PlayerInfo>();
        PlayerShoppingData = FunctionCommon.LoadData<PlayerShoppingData>();
        PlayerRemoteData = FunctionCommon.LoadData<PlayerRemoteData>();
        PlayerDaily = FunctionCommon.LoadData<PlayerDaily>();
        
        PlayerInventory = FunctionCommon.LoadData(delegate
            (PlayerInventory inventory)
        {
            ResourceController.Instance.AddQueue(delegate
            {
                foreach (var currencyDatas in GameData.Instance.Currency
                    .currencyDatas.Values)
                {
                    inventory.AddQuantity(currencyDatas.id, currencyDatas.start);
                }
            });
        }, delegate(PlayerInventory inventory)
        {
        });

        PlayerShoppingData = FunctionCommon.LoadData<PlayerShoppingData>();
        PlayerRemoteData = FunctionCommon.LoadData<PlayerRemoteData>();
        PlayerLanguage = FunctionCommon.LoadData<PlayerLanguage>();
    }
}
