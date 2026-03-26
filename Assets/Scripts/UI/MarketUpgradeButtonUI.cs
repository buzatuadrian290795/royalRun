using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Script pe prefab-ul de buton din Market Panel — se configureaza singur in functie de tipul de upgrade
public class MarketUpgradeButtonUI : MonoBehaviour
{
    [Header("Vizual")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI effectText;   // ex: "x3 coins" sau "+50%"
    [SerializeField] private TextMeshProUGUI levelText;    // ex: "Nivel 2"

    [Header("Cumparare")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Button buyButton;

    private UpgradeType m_Type;

    private void Awake()
    {
        if (buyButton != null) buyButton.onClick.AddListener(OnBuy);
    }

    // Apelat de MarketPanelSpawner imediat dupa instantiere
    public void Init(UpgradeType type)
    {
        m_Type = type;
        Refresh();
    }

    private void OnEnable() => Refresh();

    public void OnBuy()
    {
        if (UpgradeManager.Instance == null) return;
        AudioManager.Instance?.PlayUI();
        if (UpgradeManager.Instance.TryBuy(m_Type))
            Refresh();
    }

    private void Refresh()
    {
        if (UpgradeManager.Instance == null) return;

        var def  = UpgradeManager.Instance.GetDef(m_Type);
        int level = UpgradeManager.Instance.GetLevel(m_Type);
        int cost  = UpgradeManager.Instance.GetNextCost(m_Type);
        int coins = CoinCounterUI.Instance != null ? CoinCounterUI.Instance.Coins : 0;

        if (iconImage != null)
        {
            iconImage.enabled = def?.icon != null;
            if (def?.icon != null) iconImage.sprite = def.icon;
        }

        if (nameText != null)   nameText.text   = def?.displayName ?? m_Type.ToString();
        if (levelText != null)  levelText.text  = level == 0 ? "Neachizitionat" : $"Nivel {level}";
        if (effectText != null) effectText.text = UpgradeManager.Instance.GetEffectText(m_Type);
        if (costText != null)   costText.text   = CoinCounterUI.FormatCoins(cost);
        if (coinsText != null)  coinsText.text  = CoinCounterUI.FormatCoins(coins);

        if (buyButton != null)
            buyButton.interactable = coins >= cost;
    }
}
