using Unity.Mathematics.Geometry;
using UnityEngine;

public enum UpgradeType
{
    ClickPower,
    PassiveIncome,
    ClickMultiplier,
    PassiveMultiplier
}

[CreateAssetMenu(fileName = "Upgrade", menuName = "Pirate Golden/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    [Header("Identity")]
    public string upgradeId;
    public string upgradeName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Type & Value")]
    public UpgradeType upgradeType;
    public double baseValue;
    public double baseCost;
    [Range(1.01f, 2f)]
    public float costMultiplier = 1.15f;
    public int maxLevel = 50;

    [Header("Unlock Condition")]
    public double unlockAtCoinsEarned;

    public double GetCostForLevel(int currentLevel)
    {
        return Math.Round(baseCost * Math.Pow(costMultiplier, currentLevel));
    }

    public double GetValueAtLevel(int level)
    {
        return baseValue * level;
    }

    private static class Math
    {
        public static double Round(double v) => System.Math.Round(v);
        public static double Pow(double b, double e) => System.Math.Pow(b, e);
    }
}
