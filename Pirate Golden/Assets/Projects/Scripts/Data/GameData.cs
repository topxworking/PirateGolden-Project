using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public double totalCoins;
    public double coinsPerClick;
    public double coinsPerSecond;
    public double totalCoinsEarned;
    public double totalClicks;
    public List<UpgradeSaveData> upgrades = new List<UpgradeSaveData>();
}

[Serializable]
public class UpgradeSaveData
{
    public string id;
    public int level;
    public bool unlocked;
}
