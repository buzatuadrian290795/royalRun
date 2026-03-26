using TMPro;
using UnityEngine;

public class CoinCounterUI : MonoBehaviour
{
    public static CoinCounterUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI coinsText;

    private const string CoinsKey = "TotalCoins";

    private int coins;

    public int Coins => coins;

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
        int shopMultiplier = ShopManager.Instance != null ? ShopManager.Instance.Multiplier : 1;
        coins += amount * shopMultiplier;
        UpdateCoinsText();
    }

    // Adauga coins direct, fara multiplicator de shop (ex: recompensa offline)
    public void AddCoinsRaw(int amount)
    {
        coins += amount;
        UpdateCoinsText();
    }

    public void SpendCoins(int amount)
    {
        coins = Mathf.Max(0, coins - amount);
        SaveCoins();
        UpdateCoinsText();
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(CoinsKey, coins);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()                             => SaveCoins();
    private void OnApplicationPause(bool paused) { if (paused) SaveCoins(); }

    private void UpdateCoinsText()
    {
        coinsText.text = "Coins: " + FormatCoins(coins);
    }

    // Formateaza o valoare de monede cu sufix: K, M, B
    // Folosit si de alte UI-uri (costuri upgrade, recompense etc.)
    public static string FormatCoins(long amount)
    {
        if (amount >= 1_000_000_000L) return (amount / 1_000_000_000f).ToString("0.##") + "B";
        if (amount >= 1_000_000L)     return (amount / 1_000_000f).ToString("0.##") + "M";
        if (amount >= 1_000L)         return (amount / 1_000f).ToString("0.##") + "K";
        return amount.ToString();
    }
}