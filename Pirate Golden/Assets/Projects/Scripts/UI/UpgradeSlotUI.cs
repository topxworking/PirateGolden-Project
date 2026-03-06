using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject maxBadge;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip upgradeSound;

    private RuntimeUpgrade _runtime;

    public string BoundUpgradeId => _runtime?.definition?.upgradeId;

    private void Awake()
    {
        buyButton?.onClick.AddListener(OnBuyClicked);
    }

    public void Setup(RuntimeUpgrade runtime)
    {
        _runtime = runtime;

        if (iconImage && runtime.definition.icon)
            iconImage.sprite = runtime.definition.icon;

        if (nameText) nameText.text = runtime.definition.upgradeName;
        if (descText) descText.text = runtime.definition.description;

        Refresh();
    }

    public void Refresh()
    {
        if (_runtime == null) return;

        bool maxed = _runtime.IsMaxLevel;

        if (levelText)
        {
            levelText.text = $"Lv.{_runtime.level}";
        }

        if (maxBadge) maxBadge.SetActive(maxed);

        if (maxed)
        {
            if (costText) costText.text = "MAX";
            if (buyButton) buyButton.interactable = false;
        }
        else
        {
            double cost = _runtime.NextCost;
            bool canAfford = GameManager.Instance.TotalCoins >= cost;

            if (costText) costText.text = $"${GameManager.FormatNumber(cost)}";
            if (buyButton) buyButton.interactable = canAfford;
        }

        if (effectText)
        {
            var def = _runtime.definition;
            double curVal = def.GetValueAtLevel(_runtime.level);
            double nextVal = maxed ? curVal : def.GetValueAtLevel(_runtime.level + 1);

            string typeLabel = def.upgradeType switch
            {
                UpgradeType.ClickPower => "Click +",
                UpgradeType.PassiveIncome => "Income/s +",
                UpgradeType.ClickMultiplier => "Click ×",
                UpgradeType.PassiveMultiplier => "Income ×+",
                _ => ""
            };

            if (def.upgradeType == UpgradeType.ClickMultiplier || def.upgradeType == UpgradeType.PassiveMultiplier)
            {
                effectText.text = maxed
                    ? $"{typeLabel}{GameManager.FormatNumber(1 + curVal)}"
                    : $"{typeLabel}{GameManager.FormatNumber(1 + curVal)} > {GameManager.FormatNumber(1 + nextVal)}";
            }
            else
            {
                effectText.text = maxed
                    ? $"{typeLabel}{GameManager.FormatNumber(curVal)}"
                    : $"{typeLabel}{GameManager.FormatNumber(curVal)} > {GameManager.FormatNumber(nextVal)}";
            }
        }
    }

    private void Update()
    {
        if (_runtime == null || _runtime.IsMaxLevel || buyButton == null) return;
        if (GameManager.Instance == null) return;
        buyButton.interactable = GameManager.Instance.TotalCoins >= _runtime.NextCost;
    }

    private void OnBuyClicked()
    {
        if (_runtime == null) return;
        if (UpgradeManager.Instance.TryBuyUpgrade(_runtime.definition.upgradeId))
        {
            if (audioSource && upgradeSound) audioSource.PlayOneShot(upgradeSound);
            Refresh();
        }
    }
}
