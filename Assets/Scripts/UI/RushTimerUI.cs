using TMPro;
using UnityEngine;

// Afiseaza in timp real timerul de Rush; se ascunde cand Rush nu e activ
public class RushTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject panel; // Obiectul parinte de ascuns/aratat

    private void Awake()
    {
        if (timerText == null) Debug.LogError("RushTimerUI: timerText not set.");
    }

    private void Update()
    {
        if (RushEffect.Instance == null || !RushEffect.Instance.IsActive)
        {
            if (panel != null) panel.SetActive(false);
            return;
        }

        if (panel != null) panel.SetActive(true);

        if (timerText == null) return;
        float t = RushEffect.Instance.TimeRemaining;
        timerText.text = "Rush " + t.ToString("F1") + "s";
    }
}
