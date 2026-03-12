using TMPro;
using UnityEngine;
using System.Globalization;

public class CoinCounterUI : MonoBehaviour
{
    public static CoinCounterUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI coinsText;

    private int coins;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateCoinsText();
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        UpdateCoinsText();
    }

    private void UpdateCoinsText()
    {
        coinsText.text = "Coins: " + coins.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}