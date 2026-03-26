using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UI pentru Coin Multiplier din Market Panel — reutilizeaza ShopManager
public class MarketCoinMultiplierUI : MonoBehaviour
{
    [Header("Texte")]
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Buton")]
    [SerializeField] private Button buyButton;

    private void Awake()
    {
        if (multiplierText == null) Debug.LogWarning("MarketCoinMultiplierUI: multiplierText nu e asignat.");
        if (costText == null) Debug.LogWarning("MarketCoinMultiplierUI: costText nu e asignat.");
        if (coinsText == null) Debug.LogWarning("MarketCoinMultiplierUI: coinsText nu e asignat.");
        if (buyButton == null) Debug.LogWarning("MarketCoinMultiplierUI: buyButton nu e asignat.");
        else buyButton.onClick.AddListener(OnBuy);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void OnBuy()
    {
        if (ShopManager.Instance == null) return;
        AudioManager.Instance?.PlayUI();
        if (ShopManager.Instance.TryBuy())
            Refresh();
    }

    private void Refresh()
    {
        if (ShopManager.Instance == null) return;

        if (multiplierText != null)
            multiplierText.text = $"x{ShopManager.Instance.Multiplier}";

        if (costText != null)
            costText.text = $"{ShopManager.Instance.NextCost}";

        if (coinsText != null && CoinCounterUI.Instance != null)
            coinsText.text = $"{CoinCounterUI.Instance.Coins}";

        if (buyButton != null && CoinCounterUI.Instance != null)
            buyButton.interactable = CoinCounterUI.Instance.Coins >= ShopManager.Instance.NextCost;
    }
}
