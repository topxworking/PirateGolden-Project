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

    [Header("Unlock Progress")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] protected TextMeshProUGUI progressLabel;

    [Header("Notification")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 2f;

    [Header("Upgrade Panel")]
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private UpgradeSlotUI upgradeSlotPrefab;

    [Header("Floating Text")]
    [SerializeField] private FloatingTextUI floatingTextPrefabs;
    [SerializeField] private RectTransform floatingTextParent;
    [SerializeField] private RectTransform shipClickArea;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    private readonly Dictionary<string, UpgradeSlotUI> _slots = new();
    private Coroutine _notifCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        Instance = this;
    }

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
        if (!GameManager.Instance) return;
    }

    private void UpdateCoins(double v)
    {
        if (coinsText) coinsText.text = $"${GameManager.FormatNumber(v)}";
        if (totalEarnedText) totalEarnedText.text = $"Total Earned: {GameManager.FormatNumber(GameManager.Instance.TotalCoinEarned)}";
        if (totalClicksText) totalClicksText.text = $"Click: {GameManager.FormatNumber(GameManager.Instance.TotalClicks)}";
        UpdateUnlockProgression();
    }

    private void UpdateCoinsPerClick(double v)
    {
        if (coinsPerClickText) coinsPerClickText.text = $"Per Click: {GameManager.FormatNumber(v)}";
    }

    private void UpdateCoinsPerSecond(double v)
    {
        if (coinsPerSecondText) coinsPerSecondText.text = $"Per Second: {GameManager.FormatNumber(v)}";
    }

    private void RefreshAllHUD()
    {
        UpdateCoins(GameManager.Instance.TotalCoins);
        UpdateCoinsPerClick(GameManager.Instance.CoinsPerClick);
        UpdateCoinsPerSecond(GameManager.Instance.CoinsPerSecond);
    }

    private void UpdateUnlockProgression()
    {
        if (!progressSlider || !progressLabel) return;

        var um = UpgradeManager.Instance;
        double earned = GameManager.Instance.TotalCoinEarned;
        var next = um.GetAllRuntimeUpgrades().Find(r => !r.unlocked);

        if (next == null)
        {
            progressLabel.text = "All upgrades unlocked!";
            progressSlider.value = 1f;
            return;
        }

        float pct = (float)(earned / next.definition.unlockAtCoinsEarned);
        progressSlider.value = Mathf.Clamp01(pct);
        progressLabel.text = $"Next: {next.definition.upgradeName} {(pct * 100f):F0}%";
    }

    private void RebuildUpgradeList()
    {
        foreach (Transform child in upgradeContainer) Destroy(child.gameObject);
        _slots.Clear();

        foreach (var rt in UpgradeManager.Instance.GetUnlockedUpgrades())
        {
            var slot = Instantiate(upgradeSlotPrefab, upgradeContainer);
            slot.Setup(rt);
            _slots[rt.definition.upgradeId] = slot;
        }
    }

    private void RefreshAllSlots()
    {
        foreach (var slot in _slots.Values) slot.Refresh();
    }

    private void OnUpgradeBought(RuntimeUpgrade rt)
    {
        if (_slots.TryGetValue(rt.definition.upgradeId, out var slot)) slot.Refresh();
    }

    public void ShowFloatingText(string text)
    {
        if (!floatingTextPrefabs || !floatingTextParent) return;
        var ft = Instantiate(floatingTextPrefabs, floatingTextParent);
        Vector2 rnd = new Vector2(Random.Range(-50f, 50f), Random.Range(-15f, 15f));
        if (shipClickArea)
        {
            ft.GetComponent<RectTransform>().anchoredPosition = shipClickArea.anchoredPosition + rnd;
            ft.Play(text);
        }
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

    public void ToggleSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void OnSaveButtonClicked() => GameManager.Instance.SaveGame();
    public void OnResetButtonClicked() => GameManager.Instance.ResetGame();
}
