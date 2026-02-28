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

    private double _clickMultipliers = 1.0;
    private double _passiveMultipliers = 1.0;

    [Header("Auto Save")]
    [SerializeField] private float autoSaveInterval = 30f;

    [Header("Passive Tick")]
    [SerializeField] private float passiveTickRate = 0.1f;

    private float _passiveTimer;
    private float _autoSaveTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
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
        if (_passiveTimer >= passiveTickRate)
        {
            double earned = CoinsPerSecond * passiveTickRate;
            if (earned > 0) AddCoins(earned);
            _passiveTimer = 0f;
        }

        _autoSaveTimer += Time.deltaTime;
        if (_autoSaveTimer >= autoSaveInterval)
        {
            SaveGame();
            _autoSaveTimer = 0f;
        }
    }

    private void OnApplicationPause(bool pause) { if (pause) SaveGame(); }
    private void OnApplicationQuit() { SaveGame(); }

    public void OnShipClicked()
    {
        double earned = Math.Floor(CoinsPerClick * _clickMultipliers);
        TotalClicks++;
        AddCoins(earned);
        UIManager.Instance?.ShowFloatingText($"+{FormatNumber(earned)}");
    }

    private void AddCoins(double amount)
    {
        if (amount < 0) return;
        TotalCoins += amount;
        TotalCoinEarned += amount;
        OnCoinChanged?.Invoke(TotalCoins);
        UpgradeManager.Instance?.CheckUnlocks();
    }

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
                if (rt.level <= 0) continue;
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

        CoinsPerClick = Math.Max(1, baseClick);
        CoinsPerSecond = Math.Max(0, basePassive * _passiveMultipliers);

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
            upgrades = UpgradeManager.Instance?.GetSaveData()
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

        UpgradeManager.Instance?.LoadSaveData(data.upgrades);
        RecalculateStats();

        OnCoinChanged?.Invoke(TotalCoins);
    }

    public void ResetGame()
    {
        SaveManager.Instance?.DeleteSave();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public static string FormatNumber(double value)
    {
        if (value >= 1_000_000_000_000) return $"{value / 1_000_000_000_000:F2}T";
        if (value >= 1_000_000_000) return $"{value / 1_000_000_000:F2}B";
        if (value >= 1_000_000) return $"{value / 1_000_000:F2}M";
        if (value >= 1_000) return $"{value / 1_000:F2}K";
        return $"{value:F0}";
    }
}
