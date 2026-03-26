using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Interfata grafica pentru Shop — afiseaza nivelul, multiplier-ul si costul urmatorului upgrade
public class ShopUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Button buyButton;

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

        multiplierText.text = $"x{ShopManager.Instance.Multiplier}";
        costText.text       = $"{ShopManager.Instance.NextCost}";

        if (coinsText != null && CoinCounterUI.Instance != null)
            coinsText.text = $"{CoinCounterUI.Instance.Coins}";

        if (buyButton != null && CoinCounterUI.Instance != null)
            buyButton.interactable = CoinCounterUI.Instance.Coins >= ShopManager.Instance.NextCost;
    }
}
