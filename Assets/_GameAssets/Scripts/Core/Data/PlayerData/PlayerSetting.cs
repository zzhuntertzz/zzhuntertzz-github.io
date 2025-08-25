using System.Collections.Generic;

public class PlayerSetting : BasePlayerData<PlayerSetting>
{
    public bool music = true, sound = true, vibrate = true;
    public void InitSoundSetting()
    {
        ResourceController.Instance.AddQueue(delegate
        {
            SoundController.SetMuteGroup(SoundController.GroupSoundFx, !sound);
            SoundController.SetMuteGroup(SoundController.GroupSoundBG, !music);
        });
    }
    
    public List<string> packagePurchased = new();
    public void PurchasePkg(string pkg)
    {
        if (packagePurchased.Contains(pkg)) return;
        packagePurchased.Add(pkg);
    }
}

public static partial class PlayerData
{
    public static PlayerSetting PlayerSetting = new();
}