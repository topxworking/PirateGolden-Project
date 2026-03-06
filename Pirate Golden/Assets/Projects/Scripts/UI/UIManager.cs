using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI coinsPerClickText;
    [SerializeField] private TextMeshProUGUI coinsPerSecondText;
    [SerializeField] private TextMeshProUGUI totalClicksText;
    [SerializeField] private TextMeshProUGUI totalEarnedText;

    [Header("Next Unlock")]
    [SerializeField] private TextMeshProUGUI nextUnlockText;
    [SerializeField] private TextMeshProUGUI percentText;
    [SerializeField] private Image progressFillImage;

    [Header("Notification")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 2f;

    [Header("Upgrade Panel")]
    [SerializeField] private UpgradeSlotUI[] upgradeSlots;

    [Header("Floating Text")]
    [SerializeField] private FloatingTextUI floatingTextPrefab;
    [SerializeField] private RectTransform floatingTextParent;
    [SerializeField] private RectTransform shipClickArea;

    [Header("Stats Panel")]
    [SerializeField] private TextMeshProUGUI upgradesCountText;

    private Coroutine _notifCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        Instance = this;
    }

    private void Start()
    {
        var gm = GameManager.Instance;
        gm.OnCoinChanged += UpdateCoins;
        gm.OnCoinPerClickChanged += UpdateCoinsPerClick;
        gm.OnCoinPerSecondChanged += UpdateCoinsPerSecond;
        gm.OnUpgradesChanged += RefreshAllSlots;

        var um = UpgradeManager.Instance;
        um.OnUnlocksChanged += RefreshAllSlots;
        um.OnUpgradePurchased += OnUpgradeBought;

        InitSlots();
    }

    private void OnDestroy()
    {
        if (!GameManager.Instance) return;
        GameManager.Instance.OnCoinChanged -= UpdateCoins;
        GameManager.Instance.OnCoinPerClickChanged -= UpdateCoinsPerClick;
        GameManager.Instance.OnCoinPerSecondChanged -= UpdateCoinsPerSecond;
        GameManager.Instance.OnUpgradesChanged -= RefreshAllSlots;
    }

    private void UpdateCoins(double v)
    {
        if (coinsText) coinsText.text = $"${GameManager.FormatNumber(v)}";
        if (totalEarnedText) totalEarnedText.text = $"{GameManager.FormatNumber(GameManager.Instance.TotalCoinEarned)}";
        if (totalClicksText) totalClicksText.text = $"{GameManager.FormatNumber(GameManager.Instance.TotalClicks)}";
        UpdateNextUnlockText();
    }

    private void UpdateCoinsPerClick(double v)
    {
        if (coinsPerClickText) coinsPerClickText.text = $"{GameManager.FormatNumber(v)}";
    }

    private void UpdateCoinsPerSecond(double v)
    {
        if (coinsPerSecondText) coinsPerSecondText.text = $"{GameManager.FormatNumber(v)}";
    }

    private void RefreshAllHUD()
    {
        UpdateCoins(GameManager.Instance.TotalCoins);
        UpdateCoinsPerClick(GameManager.Instance.CoinsPerClick);
        UpdateCoinsPerSecond(GameManager.Instance.CoinsPerSecond);
    }

    private void UpdateNextUnlockText()
    {
        if (!nextUnlockText) return;

        double earned = GameManager.Instance.TotalCoinEarned;
        var next = UpgradeManager.Instance.GetAllRuntimeUpgrades()
            .Find(r => !r.unlocked);

        if (next == null)
        {
            nextUnlockText.text = "All upgrades unlocked!";
            if (progressFillImage) progressFillImage.fillAmount = 1f;
            return;
        }

        float pct = Mathf.Clamp01(
            (float)(earned / next.definition.unlockAtCoinsEarned));

        nextUnlockText.text = $"Next: {next.definition.upgradeName}";
        percentText.text = $"{pct * 100f:F0}%";

        if (progressFillImage) progressFillImage.fillAmount = pct;
    }

    private void InitSlots()
    {
        if (upgradeSlots == null) return;

        var all = UpgradeManager.Instance.GetAllRuntimeUpgrades();

        for (int i = 0; i < upgradeSlots.Length; i++)
        {
            var slot = upgradeSlots[i];
            if (slot == null) continue;

            if (i < all.Count)
            {
                slot.Setup(all[i]);
                slot.gameObject.SetActive(all[i].unlocked);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }

    private void RefreshAllSlots()
    {
        if (upgradeSlots == null) return;

        var all = UpgradeManager.Instance.GetAllRuntimeUpgrades();

        for (int i = 0; i < upgradeSlots.Length; i++)
        {
            var slot = upgradeSlots[i];
            if (slot == null) continue;

            if (i < all.Count)
            {
                slot.gameObject.SetActive(all[i].unlocked);
                slot.Refresh();
            }
        }

        UpdateNextUnlockText();
        UpdateUpgradesCount();
    }

    private void OnUpgradeBought(RuntimeUpgrade rt)
    {
        if (upgradeSlots == null) return;
        foreach (var slot in upgradeSlots)
        {
            if (slot != null && slot.BoundUpgradeId == rt.definition.upgradeId)
            {
                slot.Refresh();
                break;
            }
        }
    }

    public void ShowFloatingText(string text)
    {
        if (!floatingTextPrefab || !floatingTextParent) return;
        var ft = Instantiate(floatingTextPrefab, floatingTextParent);
        Vector2 rnd = new Vector2(Random.Range(-50f, 50f), Random.Range(-15f, 15f));
        if (shipClickArea)
            ft.GetComponent<RectTransform>().anchoredPosition =
                shipClickArea.anchoredPosition + rnd;
        ft.Play(text);
    }

    public void ShowNotification(string message)
    {
        if (!notificationText) return;
        if (_notifCoroutine != null) StopCoroutine(_notifCoroutine);
        _notifCoroutine = StartCoroutine(NotifRoutine(message));
    }

    private IEnumerator NotifRoutine(string msg)
    {
        notificationText.text = msg;
        notificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(notificationDuration);
        notificationText.gameObject.SetActive(false);
    }

    public void OnSaveButtonClicked() => GameManager.Instance.SaveGame();

    public void RefreshAll()
    {
        RefreshAllHUD();
        InitSlots();
        RefreshAllSlots();
    }

    public void OnResetButtonClicked()
    {
        GameManager.Instance.ResetGame();
        RefreshAll();
    }

    private void UpdateUpgradesCount()
    {
        if (!upgradesCountText) return;
        int unlocked = UpgradeManager.Instance.GetUnlockedUpgrades().Count;
        int total = UpgradeManager.Instance.GetAllRuntimeUpgrades().Count;
        upgradesCountText.text = $"{unlocked}/{total}";
    }
}
