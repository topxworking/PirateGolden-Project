using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RuntimeUpgrade
{
    public UpgradeDefinition definition;
    public int level;
    public bool unlocked;
    public bool IsMaxLevel => definition.maxLevel > 0 && level >= definition.maxLevel;
    public double NextCost => definition.GetCostForLevel(level);
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("All Upgrades")]
    [SerializeField] private List<UpgradeDefinition> upgradeDefinitions;

    private readonly List<RuntimeUpgrade> _runtimeUpgrades = new();

    public event Action<RuntimeUpgrade> OnUpgradePurchased;
    public event Action OnUnlocksChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeRuntime();
    }

    private void InitializeRuntime()
    {
        _runtimeUpgrades.Clear();
        foreach (var def in upgradeDefinitions)
        {
            _runtimeUpgrades.Add(new RuntimeUpgrade
            {
                definition = def,
                level = 0,
                unlocked = def.unlockAtCoinsEarned <= 0
            });
        }
    }

    public bool TryBuyUpgrade(string upgradeId)
    {
        var rt = GetRuntime(upgradeId);
        if (rt == null || !rt.unlocked || rt.IsMaxLevel) return false;

        if (!GameManager.Instance.SpendCoins(rt.NextCost)) return false;

        rt.level++;
        GameManager.Instance.RecalculateStats();
        GameManager.Instance.InvokeUpgradesChanged();
        OnUpgradePurchased?.Invoke(rt);
        Debug.Log($"[UpgradeManager] '{rt.definition.upgradeName}' > level {rt.level}");
        return true;
    }

    public void CheckUnlocks()
    {
        bool change = false;
        double earned = GameManager.Instance.TotalCoinEarned;

        foreach (var rt in _runtimeUpgrades)
        {
            if (!rt.unlocked && earned >= rt.definition.unlockAtCoinsEarned)
            {
                rt.unlocked = true;
                change = true;
                Debug.Log($"[UpgradeManager] Unlocked: {rt.definition.upgradeName}");
            }
        }
        if (change) OnUnlocksChanged?.Invoke();
    }

    public List<RuntimeUpgrade> GetAllRuntimeUpgrades() => _runtimeUpgrades;
    public List<RuntimeUpgrade> GetUnlockedUpgrades() => _runtimeUpgrades.FindAll(u => u.unlocked);
    public RuntimeUpgrade GetRuntime(string id) => _runtimeUpgrades.Find(u => u.definition.upgradeId == id);

    public List<UpgradeSaveData> GetSaveData()
    {
        var list = new List<UpgradeSaveData>();
        foreach (var rt in _runtimeUpgrades)
            list.Add(new UpgradeSaveData { id = rt.definition.upgradeId, level = rt.level, unlocked = rt.unlocked });
        return list;
    }

    public void LoadSaveData(List<UpgradeSaveData> saveData)
    {
        if (saveData == null) return;
        foreach (var saved in saveData)
        {
            var rt = GetRuntime(saved.id);
            if (rt != null) { rt.level = saved.level; rt.unlocked = saved.unlocked; }
        }
    }
}
