public class PlayerDataTemp : BasePlayerData<PlayerDaily>
{
    public int levelCurrent;
}
    
public static partial class PlayerData
{
    public static PlayerDataTemp PlayerDataTemp = new();
}