public class PlayerLanguage : BasePlayerData<PlayerLanguage>
{
    public string language = "English";
}

public static partial class PlayerData
{
    public static PlayerLanguage PlayerLanguage = new();
}