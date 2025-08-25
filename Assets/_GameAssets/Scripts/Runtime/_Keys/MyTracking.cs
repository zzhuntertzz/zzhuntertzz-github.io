public class MyTracking
{
    public static string HighestLevel
    {
        get
        {
            return $"{PlayerData.PlayerInfo.highestLevelPlayed:000}";
        }
    }
    public static string CurrentLevel
    {
        get
        {
            return $"{PlayerData.PlayerDataTemp.levelCurrent:000}";
        }
    }
}