using System;

[Serializable]
public class RewardData
{
    public string rewardId;
    public int rewardQuantity;

    public virtual async void Claim(string source = "")
    {
        PlayerData.PlayerInventory.AddQuantity(
            rewardId, rewardQuantity, source);
    }
}