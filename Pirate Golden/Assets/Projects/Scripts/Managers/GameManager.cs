using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<double> OnCoinChanged;
    public event Action<double> OnCoinPerClickChanged;
    public event Action<double> OnCoinPerSecondChanged;
    public event Action OnUpgradesChanged;

    public double TotalCoins { get; private set; }
    public double CoinsPerClick { get; private set; } = 1;
    public double CoinsPerSecond { get; private set; } = 0;
    public double TotalCoinEarned { get; private set; }
    public double TotalClicks { get; private set; }

    public int RebirthCount { get; private set; }
    public double RebirthMultiplier { get; private set; } = 1.0;
    public bool CanRebirth
    {
        get
        {
            if (TotalCoins < NextRebirthCost) return false;
            return UpgradeManager.Instance.GetAllRuntimeUpgrades()
                .TrueForAll(r => r.IsMaxLevel);
        }
    }

    public event Action OnRebirthChanged;

    private const double BASE_REBIRTH_COST = 100000;
    public double NextRebirthCost => BASE_REBIRTH_COST * Math.Pow(10, RebirthCount);

    private double _clickMultipliers = 1.0;
    private double _passiveMultipliers = 1.0;

    private float _passiveTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        LoadGame();
    }

    private void Update()
    {
        _passiveTimer += Time.deltaTime;
        if (_passiveTimer >= 1f)
        {
            if (CoinsPerSecond > 0) AddCoins(CoinsPerSecond);
            _passiveTimer -= 1f;
        }
    }

    private void OnApplicationPause(bool pause) { if (pause) SaveGame(); }
    private void OnApplicationQuit() { SaveGame(); }

    public void OnShipClicked()
    {
        double earned = Math.Floor(CoinsPerClick);
        TotalClicks++;
        AddCoins(earned);
        UIManager.Instance?.ShowFloatingText($"+{FormatNumber(earned)}");
    }

    private void AddCoins(double amount)
    {
        if (amount <= 0) return;
        TotalCoins += amount;
        TotalCoinEarned += amount;
        OnCoinChanged?.Invoke(TotalCoins);
        UpgradeManager.Instance?.CheckUnlocks();
    }

    public void InvokeUpgradesChanged() => OnUpgradesChanged?.Invoke();

    public bool SpendCoins(double amount)
    {
        if (TotalCoins < amount) return false;
        TotalCoins -= amount;
        OnCoinChanged?.Invoke(TotalCoins);
        return true;
    }

    public void RecalculateStats()
    {
        double baseClick = 1;
        double basePassive = 0;
        _clickMultipliers = 1.0;
        _passiveMultipliers = 1.0;

        if (UpgradeManager.Instance != null)
        {
            foreach (var rt in UpgradeManager.Instance.GetAllRuntimeUpgrades())
            {
                if (rt.level < 1) continue;
                double val = rt.definition.GetValueAtLevel(rt.level);

                switch (rt.definition.upgradeType)
                {
                    case UpgradeType.ClickPower: baseClick += val; break;
                    case UpgradeType.PassiveIncome: basePassive += val; break;
                    case UpgradeType.ClickMultiplier: _clickMultipliers += val; break;
                    case UpgradeType.PassiveMultiplier: _passiveMultipliers += val; break;
                }
            }
        }

        CoinsPerClick = Math.Floor(Math.Max(1, baseClick * _clickMultipliers * RebirthMultiplier));
        CoinsPerSecond = Math.Floor(Math.Max(0, basePassive * _passiveMultipliers * RebirthMultiplier));

        OnCoinPerClickChanged?.Invoke(CoinsPerClick);
        OnCoinPerSecondChanged?.Invoke(CoinsPerSecond);
    }

    public void SaveGame()
    {
        if (SaveManager.Instance == null) return;
        var data = new GameData
        {
            totalCoins = TotalCoins,
            coinsPerClick = CoinsPerClick,
            coinsPerSecond = CoinsPerSecond,
            totalCoinsEarned = TotalCoinEarned,
            totalClicks = TotalClicks,
            upgrades = UpgradeManager.Instance?.GetSaveData(),
            rebirthCount = RebirthCount,
            rebirthMultiplier = RebirthMultiplier
        };
        SaveManager.Instance.Save(data);
        UIManager.Instance?.ShowNotification("Game Saved.");
    }

    public void LoadGame()
    {
        if (SaveManager.Instance == null) return;
        GameData data = SaveManager.Instance.Load();

        TotalCoins = data.totalCoins;
        TotalCoinEarned = data.totalCoinsEarned;
        TotalClicks = data.totalClicks;
        RebirthCount = data.rebirthCount;
        RebirthMultiplier = data.rebirthMultiplier > 0 ? data.rebirthMultiplier : 1.0;

        UpgradeManager.Instance?.LoadSaveData(data.upgrades);
        RecalculateStats();

        OnCoinChanged?.Invoke(TotalCoins);
        OnCoinPerClickChanged?.Invoke(CoinsPerClick);
        OnCoinPerSecondChanged?.Invoke(CoinsPerSecond);
        OnUpgradesChanged?.Invoke();
    }

    public void ResetGame()
    {
        UpgradeManager.Instance?.ResetAllUpgrades();
        SaveManager.Instance?.ResetSave();

        TotalCoins = 0;
        TotalCoinEarned = 0;
        TotalClicks = 0;
        RebirthCount = 0;
        RebirthMultiplier = 1.0;

        RecalculateStats();

        OnCoinChanged?.Invoke(TotalCoins);
        OnCoinPerClickChanged?.Invoke(CoinsPerClick);
        OnCoinPerSecondChanged?.Invoke(CoinsPerSecond);
        OnUpgradesChanged?.Invoke();
        OnRebirthChanged?.Invoke();
    }

    public static string FormatNumber(double value)
    {
        if (value >= 1_000_000_000_000) return $"{value / 1_000_000_000_000:F2}T";
        if (value >= 1_000_000_000) return $"{value / 1_000_000_000:F2}B";
        if (value >= 1_000_000) return $"{value / 1_000_000:F2}M";
        if (value >= 1_000) return $"{value / 1_000:F2}K";
        return $"{value:F0}";
    }

    public void Rebirth()
    {
        Debug.Log($"TotalCoins: {TotalCoins} | NextRebirthCost: {NextRebirthCost}");
        foreach (var rt in UpgradeManager.Instance.GetAllRuntimeUpgrades())
            Debug.Log($"{rt.definition.upgradeName} | Lv:{rt.level}/{rt.definition.maxLevel} | IsMaxLevel:{rt.IsMaxLevel}");

        if (!CanRebirth) return;

        SpendCoins(NextRebirthCost);

        RebirthCount++;
        RebirthMultiplier = Math.Pow(2, RebirthCount);

        UpgradeManager.Instance?.ResetAllUpgrades();

        TotalCoins = 0;
        TotalCoinEarned = 0;
        TotalClicks = 0;

        RecalculateStats();

        OnCoinChanged?.Invoke(TotalCoins);
        OnCoinPerClickChanged?.Invoke(CoinsPerClick);
        OnCoinPerSecondChanged?.Invoke(CoinsPerSecond);
        OnUpgradesChanged?.Invoke();
        OnRebirthChanged?.Invoke();

        SaveGame();
        Debug.Log($"Rebirth #{RebirthCount} | Multiplier: x{RebirthMultiplier} | Next cost: {FormatNumber(NextRebirthCost)}");
    }

    public void CheatAddCoins(double amount)
    {
        if (amount <= 0) return;
        AddCoins(amount);
        Debug.Log($"[Cheat] Added {FormatNumber(amount)} coins");
    }
}
