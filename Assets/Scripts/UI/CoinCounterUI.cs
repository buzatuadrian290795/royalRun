using TMPro;
using UnityEngine;
using System.Globalization;

public class CoinCounterUI : MonoBehaviour
{
    public static CoinCounterUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI coinsText;

    private const string CoinsKey = "TotalCoins";

    private int coins;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        coins = PlayerPrefs.GetInt(CoinsKey, 0);
        UpdateCoinsText();
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt(CoinsKey, coins);
        UpdateCoinsText();
    }

    private void UpdateCoinsText()
    {
        coinsText.text = "Coins: " + coins.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}