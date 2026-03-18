using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject m_Overlay;
    [SerializeField] private GameObject m_MainPanel;
    [SerializeField] private GameObject m_AudioPanel;
    [SerializeField] private GameObject m_GraphicsPanel;
    [SerializeField] private GameObject m_GameplayPanel;
    [SerializeField] private GameObject m_GeneralPanel;

    [Header("Audio")]
    [SerializeField] private Slider m_MusicSlider;
    [SerializeField] private Slider m_SfxSlider;
    [SerializeField] private Toggle m_MuteToggle;

    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown m_QualityDropdown;
    [SerializeField] private TMP_Dropdown m_FpsDropdown;
    [SerializeField] private Toggle m_ShadowsToggle;

    [Header("Gameplay")]
    [SerializeField] private Slider m_SensitivitySlider;
    [SerializeField] private Toggle m_VibrationToggle;

    private bool m_IsLoading;

    private void Awake()
    {
        // Conecteaza sliderii prin cod, nu prin Inspector
        if (m_MusicSlider != null)
            m_MusicSlider.onValueChanged.AddListener(OnMusicVolume);
        if (m_SfxSlider != null)
            m_SfxSlider.onValueChanged.AddListener(OnSfxVolume);
        if (m_MuteToggle != null)
            m_MuteToggle.onValueChanged.AddListener(OnMute);
        if (m_ShadowsToggle != null)
            m_ShadowsToggle.onValueChanged.AddListener(OnShadows);
        if (m_SensitivitySlider != null)
            m_SensitivitySlider.onValueChanged.AddListener(OnSensitivity);
        if (m_VibrationToggle != null)
            m_VibrationToggle.onValueChanged.AddListener(OnVibration);

        m_Overlay.SetActive(false);
    }

    public void Open()
    {
        Debug.Log($"SettingsUI.Open | SettingsManager: {SettingsManager.Instance} | AudioManager: {AudioManager.Instance}");
        m_Overlay.SetActive(true);
        ShowPanel(m_MainPanel);
        LoadValuesIntoUI();
    }

    public void Close()
    {
        m_Overlay.SetActive(false);
        PlayerPrefs.Save();
    }

    public void ShowAudio() => ShowPanel(m_AudioPanel);
    public void ShowGraphics() => ShowPanel(m_GraphicsPanel);
    public void ShowGameplay() => ShowPanel(m_GameplayPanel);
    public void ShowGeneral() => ShowPanel(m_GeneralPanel);
    public void ShowMain() => ShowPanel(m_MainPanel);

    // Audio callbacks
    //public void OnMusicVolume(float v)
    //{
    //    if (SettingsManager.Instance != null)
    //        SettingsManager.Instance.SetMusicVolume(v);
    //}
    public void OnMusicVolume(float v)
    {
        if (m_IsLoading) return;
        Debug.Log($"OnMusicVolume: {v} | SettingsManager: {SettingsManager.Instance}");
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetMusicVolume(v);
    }

    public void OnSfxVolume(float v)
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetSfxVolume(v);
    }

    public void OnMute(bool v)
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetMuted(v);
    }

    // Graphics callbacks
    public void OnQuality(int v) => SettingsManager.Instance.SetQualityLevel(v);
    public void OnFPS(int v)
    {
        int[] options = { 30, 60, 120 };
        SettingsManager.Instance.SetTargetFPS(options[v]);
    }
    public void OnShadows(bool v) => SettingsManager.Instance.SetShadows(v);

    // Gameplay callbacks
    public void OnSensitivity(float v) => SettingsManager.Instance.SetSwipeSensitivity(v);
    public void OnVibration(bool v) => SettingsManager.Instance.SetVibration(v);

    // General callbacks
    public void OnResetProgress()
    {
        SettingsManager.Instance.DoResetProgress();
        LoadValuesIntoUI();
    }

    private void ShowPanel(GameObject panel)
    {
        m_MainPanel.SetActive(false);
        m_AudioPanel.SetActive(false);
        m_GraphicsPanel.SetActive(false);
        m_GameplayPanel.SetActive(false);
        m_GeneralPanel.SetActive(false);
        panel.SetActive(true);
    }

    private void LoadValuesIntoUI()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogWarning("SettingsManager.Instance e null!");
            return;
        }

        m_IsLoading = true;

        var s = SettingsManager.Instance;

        if (m_MusicSlider != null) m_MusicSlider.SetValueWithoutNotify(s.MusicVolume);
        if (m_SfxSlider != null) m_SfxSlider.SetValueWithoutNotify(s.SfxVolume);
        if (m_MuteToggle != null) m_MuteToggle.SetIsOnWithoutNotify(s.IsMuted);
        if (m_ShadowsToggle != null) m_ShadowsToggle.SetIsOnWithoutNotify(s.ShadowsEnabled);
        if (m_SensitivitySlider != null) m_SensitivitySlider.SetValueWithoutNotify(s.SwipeSensitivity);
        if (m_VibrationToggle != null) m_VibrationToggle.SetIsOnWithoutNotify(s.VibrationEnabled);

        m_IsLoading = false;
    }

    private void OnDestroy()
    {
        if (m_MusicSlider != null)
            m_MusicSlider.onValueChanged.RemoveListener(OnMusicVolume);
        if (m_SfxSlider != null)
            m_SfxSlider.onValueChanged.RemoveListener(OnSfxVolume);
        if (m_MuteToggle != null)
            m_MuteToggle.onValueChanged.RemoveListener(OnMute);
        if (m_ShadowsToggle != null)
            m_ShadowsToggle.onValueChanged.RemoveListener(OnShadows);
        if (m_SensitivitySlider != null)
            m_SensitivitySlider.onValueChanged.RemoveListener(OnSensitivity);
        if (m_VibrationToggle != null)
            m_VibrationToggle.onValueChanged.RemoveListener(OnVibration);
    }
}