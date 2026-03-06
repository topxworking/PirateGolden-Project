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

    [Header("Exit Panel")]
    [SerializeField] private GameObject exitPanel;

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private float slideSpeed = 800f;

    [Header("Rebirth")]
    [SerializeField] private GameObject rebirthButton;
    [SerializeField] private TextMeshProUGUI rebirthCountText;

    private Coroutine _notifCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        Instance = this;
    }

    private void Start()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(true);

        if (mainMenuPanel)
        {
            var rt = mainMenuPanel.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }

        var gm = GameManager.Instance;
        gm.OnCoinChanged += UpdateCoins;
        gm.OnCoinPerClickChanged += UpdateCoinsPerClick;
        gm.OnCoinPerSecondChanged += UpdateCoinsPerSecond;
        gm.OnUpgradesChanged += RefreshAllSlots;

        var um = UpgradeManager.Instance;
        um.OnUnlocksChanged += RefreshAllSlots;
        um.OnUpgradePurchased += OnUpgradeBought;

        if (exitPanel) exitPanel.SetActive(false);

        GameManager.Instance.OnRebirthChanged += OnRebirthUpdated;
        if (rebirthButton) rebirthButton.SetActive(false);

        InitSlots();
    }

    private void OnDestroy()
    {
        if (!GameManager.Instance) return;
        GameManager.Instance.OnCoinChanged -= UpdateCoins;
        GameManager.Instance.OnCoinPerClickChanged -= UpdateCoinsPerClick;
        GameManager.Instance.OnCoinPerSecondChanged -= UpdateCoinsPerSecond;
        GameManager.Instance.OnUpgradesChanged -= RefreshAllSlots;

        if (GameManager.Instance)
            GameManager.Instance.OnRebirthChanged -= OnRebirthUpdated;
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
        UpdateRebirthButton();
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
        Debug.Log($"ShowFloatingText called: {text}");
        if (!floatingTextPrefab || !floatingTextParent)
        {
            Debug.LogWarning("FloatingText prefab or parent is null!");
            return;
        }
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

    public void OnExitButtonClicked()
    {
        if (exitPanel) exitPanel.SetActive(true);
    }

    public void OnExitConfirm()
    {
        GameManager.Instance.SaveGame();
        Application.Quit();
    }

    public void OnExitCancel()
    {
        if (exitPanel) exitPanel.SetActive(false);
    }

    public void OnPlayButtonClicked()
    {
        StartCoroutine(SlideUpAndHide());
    }

    private IEnumerator SlideUpAndHide()
    {
        RectTransform rt = mainMenuPanel.GetComponent<RectTransform>();
        float targetY = Screen.height * 1.2f;

        while (rt.anchoredPosition.y < targetY)
        {
            rt.anchoredPosition += Vector2.up * slideSpeed * Time.deltaTime;
            yield return null;
        }

        mainMenuPanel.SetActive(false);
        RefreshAll();
    }

    private void OnRebirthUpdated()
    {
        UpdateRebirthButton();
        RefreshAll();
    }

    private void UpdateRebirthButton()
    {
        if (!rebirthButton) return;
        bool canRebirth = GameManager.Instance.CanRebirth;
        rebirthButton.SetActive(canRebirth);

        if (rebirthCountText && canRebirth)
        {
            int next = GameManager.Instance.RebirthCount + 1;
            double nextMult = 1.0 + (next * 0.5);
            rebirthCountText.text = $"REBIRTH #{next}\n×{nextMult:F1} All";
        }
    }

    public void OnRebirthButtonClicked()
    {
        GameManager.Instance.Rebirth();
    }
}
