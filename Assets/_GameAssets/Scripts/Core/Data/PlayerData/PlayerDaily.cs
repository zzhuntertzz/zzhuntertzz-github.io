using System;
using UnityEngine;

public class PlayerDaily : BasePlayerData<PlayerDaily>
{
    public int retention = 0;
    
    public int daysPlayed = 0;
    public int dayInMonth = 0;
    public int lastDay = 0;
    public bool isDailyClaimed = false;

    public int DayPlayedAtSeven()
    {
        var value = daysPlayed % 7;
        return value == 0 ? 7 : value;
    }

    public int GetDaysPlayed()
    {
        var today = DateTime.Now.DayOfYear;

        if (dayInMonth > DateTime.Now.Day || today - lastDay > 31)
        {
            //new month
            // PlayerData.PlayerDailyReward.lstDayClaimed.Clear();
            // PlayerData.PlayerDailyReward.Save();
        }
        dayInMonth = DateTime.Now.Day;
        
        if (lastDay != today)
        {
            if (lastDay != 0)
            {
                if (today > lastDay)
                    retention += today - lastDay;
                else //new year
                    retention += today;
            }
            lastDay = today;
            daysPlayed++;
            
            NewDay();
            if (daysPlayed % 7 == 1)
                NewWeek();
            Save();
        }
        Debug.Log($">> day logged {daysPlayed}");

        return daysPlayed;
    }

    void NewDay()
    {
        isDailyClaimed = false;
    }

    void NewWeek()
    {
    }
}

public static partial class PlayerData
{
    public static PlayerDaily PlayerDaily = new();
}