using System;
using UnityEngine;

public enum UpgradeType { CoinMultiplier, AppleSpeed, MagnetPower, RushDuration, OfflineCoins }

// Gestioneaza toate upgrade-urile din Market Panel si persista progresul intre sesiuni
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Serializable]
    public class UpgradeDef
    {
        public UpgradeType type;
        public string displayName;
        public Sprite icon;
        public int baseCost = 50;
        [Tooltip("Cat de mult creste pretul per nivel (ex: 1.8 = +80% per nivel)")]
        public float costGrowthRate = 1.8f;
        [Tooltip("Cat adauga fiecare nivel la efect. Coin/Apple/Magnet/Rush: procent (0.5 = +50%). OfflineCoins: coins/minut per nivel.")]
        public float valuePerLevel = 0.5f;
    }

    [SerializeField] private UpgradeDef[] upgrades;

    private const string LastQuitTimeKey = "LastQuitTime";
    private const string CoinsKey        = "TotalCoins";
    private const double MaxOfflineHours = 8.0;

    // Setate in Awake dupa calculul offline; revendicate de OfflineRewardUI
    public int      PendingOfflineCoins { get; private set; }
    public TimeSpan PendingOfflineTime  { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        MigrateShopManager();
        ApplyOfflineCoins();
    }

    // Migreaza progresul vechi din ShopManager
    private void MigrateShopManager()
    {
        const string oldKey = "CoinMultiplierLevel";
        if (!PlayerPrefs.HasKey(oldKey)) return;

        int oldLevel = PlayerPrefs.GetInt(oldKey, 0);
        string newKey = PrefKey(UpgradeType.CoinMultiplier);
        if (!PlayerPrefs.HasKey(newKey) && oldLevel > 0)
            PlayerPrefs.SetInt(newKey, oldLevel);

        PlayerPrefs.DeleteKey(oldKey);
        PlayerPrefs.Save();
    }

    // Calculeaza monedele castigate offline si le stocheaza in PendingOfflineCoins
    // (fara a le adauga inca — OfflineRewardUI le va revendica dupa ce CoinCounterUI e gata)
    private void ApplyOfflineCoins()
    {
        string tickStr = PlayerPrefs.GetString(LastQuitTimeKey, "");
        if (string.IsNullOrEmpty(tickStr) || !long.TryParse(tickStr, out long ticks)) return;

        int offlineLevel = GetLevel(UpgradeType.OfflineCoins);
        if (offlineLevel == 0) return;

        double elapsedMinutes = (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalMinutes;
        elapsedMinutes = Math.Min(elapsedMinutes, MaxOfflineHours * 60.0);

        float valuePerLevel = GetDef(UpgradeType.OfflineCoins)?.valuePerLevel ?? 10f;
        int earned = Mathf.RoundToInt((float)(elapsedMinutes * offlineLevel * valuePerLevel));
        if (earned <= 0) return;

        PendingOfflineCoins = earned;
        PendingOfflineTime  = TimeSpan.FromMinutes(elapsedMinutes);
    }

    // Apelat de OfflineRewardUI cand jucatorul apasa "Colecteaza"
    public void ClaimOfflineCoins()
    {
        if (PendingOfflineCoins <= 0) return;
        CoinCounterUI.Instance?.AddCoinsRaw(PendingOfflineCoins);
        PendingOfflineCoins = 0;
    }

    private void OnApplicationQuit()    => SaveQuitTime();
    private void OnApplicationPause(bool paused) { if (paused) SaveQuitTime(); }

    private void SaveQuitTime()
    {
        PlayerPrefs.SetString(LastQuitTimeKey, DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();
    }

    // --- API ---

    public int GetLevel(UpgradeType type) => PlayerPrefs.GetInt(PrefKey(type), 0);

    // Returneaza multiplicatorul ca float: 1 + nivel * valuePerLevel
    // Exceptie: CoinMultiplier returneaza 1 + nivel (integer logic, fara valuePerLevel)
    public float GetMultiplier(UpgradeType type)
    {
        int level = GetLevel(type);
        if (level == 0) return 1f;
        if (type == UpgradeType.CoinMultiplier) return 1f + level;
        float perLevel = GetDef(type)?.valuePerLevel ?? 0f;
        if (perLevel <= 0f) perLevel = 0.5f;
        return 1f + level * perLevel;
    }

    // Versiune integer pentru coin multiplier (folosit in CoinCounterUI)
    public int GetCoinShopMultiplier() => 1 + GetLevel(UpgradeType.CoinMultiplier);

    public int GetNextCost(UpgradeType type)
    {
        UpgradeDef def = GetDef(type);
        if (def == null) return 0;
        return Mathf.RoundToInt(def.baseCost * Mathf.Pow(def.costGrowthRate, GetLevel(type)));
    }

    public bool CanBuy(UpgradeType type) =>
        CoinCounterUI.Instance != null && CoinCounterUI.Instance.Coins >= GetNextCost(type);

    public bool TryBuy(UpgradeType type)
    {
        int cost = GetNextCost(type);
        if (CoinCounterUI.Instance == null || CoinCounterUI.Instance.Coins < cost) return false;

        CoinCounterUI.Instance.SpendCoins(cost);
        PlayerPrefs.SetInt(PrefKey(type), GetLevel(type) + 1);
        PlayerPrefs.Save();
        return true;
    }

    // Returneaza textul efectului curent pentru un tip (pentru afisare in UI)
    public string GetEffectText(UpgradeType type)
    {
        int level = GetLevel(type);
        if (level == 0) return "Neactiv";

        if (type == UpgradeType.CoinMultiplier)
            return $"x{GetCoinShopMultiplier()} coins";

        if (type == UpgradeType.OfflineCoins)
        {
            float perMin = level * (GetDef(type)?.valuePerLevel ?? 10f);
            return $"{perMin:0}/min offline";
        }

        float bonus = (GetMultiplier(type) - 1f) * 100f;
        return $"+{bonus:0}%";
    }

    // Backward compatibility pentru ShopManager
    public int Multiplier  => GetCoinShopMultiplier();
    public int NextCost    => GetNextCost(UpgradeType.CoinMultiplier);
    public bool TryBuy()   => TryBuy(UpgradeType.CoinMultiplier);

    public UpgradeDef[] Upgrades => upgrades;

    public UpgradeDef GetDef(UpgradeType type)
    {
        if (upgrades == null) return null;
        foreach (var def in upgrades)
            if (def.type == type) return def;
        return null;
    }

    private static string PrefKey(UpgradeType type) => $"UpgradeLevel_{type}";
}
