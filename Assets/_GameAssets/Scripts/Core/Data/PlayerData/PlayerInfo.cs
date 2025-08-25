using System;
using System.Collections.Generic;

[Serializable]
public class PlayerInfo : BasePlayerData<PlayerInfo>
{
    public int levelPassed = 0;
    public int highestLevelPlayed = 0;
    public int playedCount = 0;
    
    public int loseCount = 0;
    public List<string> lstTut = new();
}

public static partial class PlayerData
{
    public static PlayerInfo PlayerInfo = new();
}