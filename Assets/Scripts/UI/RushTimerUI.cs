using TMPro;
using UnityEngine;

// Afiseaza in timp real timerul de Rush; se ascunde cand Rush nu e activ
public class RushTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject panel;

    private bool m_PanelActive = false;
    private float m_LastDisplayedTime = -1f;

    private void Awake()
    {
        if (timerText == null) Debug.LogError("RushTimerUI: timerText not set.");
    }

    private void Update()
    {
        bool isActive = RushEffect.Instance != null && RushEffect.Instance.IsActive;

        if (isActive != m_PanelActive)
        {
            m_PanelActive = isActive;
            if (panel != null) panel.SetActive(isActive);
            if (!isActive) { m_LastDisplayedTime = -1f; return; }
        }

        if (!isActive || timerText == null) return;

        float t = RushEffect.Instance.TimeRemaining;
        float rounded = Mathf.Floor(t * 10f) / 10f;

        if (rounded == m_LastDisplayedTime) return;
        m_LastDisplayedTime = rounded;
        timerText.text = "Rush " + rounded.ToString("F1") + "s";
    }
}
