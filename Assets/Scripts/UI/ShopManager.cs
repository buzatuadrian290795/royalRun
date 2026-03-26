using UnityEngine;

// Wrapper de compatibilitate — redirecteaza catre UpgradeManager.
// ShopUI si alte componente vechi continua sa functioneze fara modificari in scena.
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public int Multiplier => UpgradeManager.Instance != null ? UpgradeManager.Instance.GetCoinShopMultiplier() : 1;
    public int NextCost   => UpgradeManager.Instance != null ? UpgradeManager.Instance.GetNextCost(UpgradeType.CoinMultiplier) : 0;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool TryBuy()        => UpgradeManager.Instance?.TryBuy(UpgradeType.CoinMultiplier) ?? false;
    public void ResetUpgrades() => Debug.LogWarning("ShopManager.ResetUpgrades: foloseste UpgradeManager direct.");
}
