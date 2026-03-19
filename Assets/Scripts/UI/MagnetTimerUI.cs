using TMPro;
using UnityEngine;

// Afiseaza in timp real timerul de Magnet; se ascunde cand Magnetul nu e activ
public class MagnetTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject panel; // Obiectul parinte de ascuns/aratat

    private void Awake()
    {
        if (timerText == null) Debug.LogError("MagnetTimerUI: timerText not set.");
    }

    private void Update()
    {
        if (MagnetEffect.Instance == null || !MagnetEffect.Instance.IsActive)
        {
            if (panel != null) panel.SetActive(false);
            return;
        }

        if (panel != null) panel.SetActive(true);

        if (timerText == null) return;
        float t = MagnetEffect.Instance.TimeRemaining;
        timerText.text = "Magnet " + t.ToString("F1") + "s";
    }
}
