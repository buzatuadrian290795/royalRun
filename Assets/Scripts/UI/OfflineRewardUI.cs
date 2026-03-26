using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Afiseaza un popup la deschiderea jocului cu monedele castigate offline
// Necesita: un Panel cu TextMeshProUGUI pentru timp, coins si un Button de revendicare
public class OfflineRewardUI : MonoBehaviour
{
    [Header("Referinte")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Button claimButton;

    private void Start()
    {
        int pending = UpgradeManager.Instance?.PendingOfflineCoins ?? 0;

        if (pending <= 0)
        {
            panel.SetActive(false);
            return;
        }

        TimeSpan elapsed = UpgradeManager.Instance.PendingOfflineTime;
        timeText.text  = "Ai fost offline " + FormatTime(elapsed);
        coinsText.text = "+" + CoinCounterUI.FormatCoins(pending) + " coins";

        claimButton.onClick.AddListener(Claim);
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Claim()
    {
        UpgradeManager.Instance?.ClaimOfflineCoins();
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private static string FormatTime(TimeSpan t)
    {
        if (t.TotalHours >= 1)
            return $"{(int)t.TotalHours}h {t.Minutes:D2}m";
        return $"{t.Minutes}m {t.Seconds:D2}s";
    }
}
