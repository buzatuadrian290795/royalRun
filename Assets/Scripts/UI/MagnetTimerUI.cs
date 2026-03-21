using TMPro;
using UnityEngine;

// Afiseaza in timp real timerul de Magnet; se ascunde cand Magnetul nu e activ
public class MagnetTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject panel;

    private bool m_PanelActive = false;
    private float m_LastDisplayedTime = -1f;

    private void Awake()
    {
        if (timerText == null) Debug.LogError("MagnetTimerUI: timerText not set.");
    }

    private void Update()
    {
        bool isActive = MagnetEffect.Instance != null && MagnetEffect.Instance.IsActive;

        if (isActive != m_PanelActive)
        {
            m_PanelActive = isActive;
            if (panel != null) panel.SetActive(isActive);
            if (!isActive) { m_LastDisplayedTime = -1f; return; }
        }

        if (!isActive || timerText == null) return;

        float t = MagnetEffect.Instance.TimeRemaining;
        float rounded = Mathf.Floor(t * 10f) / 10f;

        if (rounded == m_LastDisplayedTime) return;
        m_LastDisplayedTime = rounded;
        timerText.text = "Magnet " + rounded.ToString("F1") + "s";
    }
}
