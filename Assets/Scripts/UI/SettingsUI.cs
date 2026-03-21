using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Gestioneaza interfata grafica a meniului de setari
public class SettingsUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject m_Overlay;        // Fundalul inchis al meniului
    [SerializeField] private GameObject m_MainPanel;      // Panoul principal (meniu categorii)
    [SerializeField] private GameObject m_PauseMenu;      // Meniul de pauza (ascuns cand Settings e deschis)
    [SerializeField] private GameObject m_AudioPanel;
    [SerializeField] private GameObject m_GraphicsPanel;
    [SerializeField] private GameObject m_GameplayPanel;
    [SerializeField] private GameObject m_GeneralPanel;
    [SerializeField] private GameObject m_DevToolsPanel;

    [Header("Dev Tools")]
    [SerializeField] private DevToolsUI m_DevToolsUI;

    [Header("General")]
    [SerializeField] private GameObject m_ConfirmResetPanel; // Dialog confirmare reset progres

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

    // Flag activ cat timp incarcam valorile in UI (previne salvarea accidentala)
    private bool m_IsLoading;

    private void Awake()
    {
        // Aboneaza fiecare element UI la metoda corespunzatoare
        if (m_MusicSlider != null) m_MusicSlider.onValueChanged.AddListener(OnMusicVolume);
        if (m_SfxSlider != null) m_SfxSlider.onValueChanged.AddListener(OnSfxVolume);
        if (m_MuteToggle != null) m_MuteToggle.onValueChanged.AddListener(OnMute);
        if (m_ShadowsToggle != null) m_ShadowsToggle.onValueChanged.AddListener(OnShadows);
        if (m_SensitivitySlider != null) m_SensitivitySlider.onValueChanged.AddListener(OnSensitivity);
        if (m_VibrationToggle != null) m_VibrationToggle.onValueChanged.AddListener(OnVibration);
        if (m_QualityDropdown != null) m_QualityDropdown.onValueChanged.AddListener(OnQuality);
        if (m_FpsDropdown != null) m_FpsDropdown.onValueChanged.AddListener(OnFPS);

        m_Overlay.SetActive(false); // Ascunde meniul la start
    }

    private void OnDestroy()
    {
        // Dezaboneaza listenerii pentru a evita memory leaks
        if (m_MusicSlider != null) m_MusicSlider.onValueChanged.RemoveListener(OnMusicVolume);
        if (m_SfxSlider != null) m_SfxSlider.onValueChanged.RemoveListener(OnSfxVolume);
        if (m_MuteToggle != null) m_MuteToggle.onValueChanged.RemoveListener(OnMute);
        if (m_ShadowsToggle != null) m_ShadowsToggle.onValueChanged.RemoveListener(OnShadows);
        if (m_SensitivitySlider != null) m_SensitivitySlider.onValueChanged.RemoveListener(OnSensitivity);
        if (m_VibrationToggle != null) m_VibrationToggle.onValueChanged.RemoveListener(OnVibration);
        if (m_QualityDropdown != null) m_QualityDropdown.onValueChanged.RemoveListener(OnQuality);
        if (m_FpsDropdown != null) m_FpsDropdown.onValueChanged.RemoveListener(OnFPS);
    }

    // Deschide meniul: afiseaza overlay-ul, panoul principal si incarca valorile curente
    public void Open()
    {
        AudioManager.Instance?.PlayUI();
        if (m_PauseMenu != null) m_PauseMenu.SetActive(false);
        m_Overlay.SetActive(true);
        ShowPanel(m_MainPanel);
        LoadValuesIntoUI();
    }

    // Inchide meniul si salveaza setarile pe disk
    public void Close()
    {
        AudioManager.Instance?.PlayUI();
        m_Overlay.SetActive(false);
        if (m_PauseMenu != null) m_PauseMenu.SetActive(true);
        PlayerPrefs.Save();
    }

    // Metode publice pentru navigarea intre panouri (apelate din butoanele din UI)
    public void ShowAudio() { AudioManager.Instance?.PlayUI(); ShowPanel(m_AudioPanel); }
    public void ShowGraphics() { AudioManager.Instance?.PlayUI(); ShowPanel(m_GraphicsPanel); }
    public void ShowGameplay() { AudioManager.Instance?.PlayUI(); ShowPanel(m_GameplayPanel); }
    public void ShowGeneral() { AudioManager.Instance?.PlayUI(); ShowPanel(m_GeneralPanel); }
    public void ShowMain() { AudioManager.Instance?.PlayUI(); ShowPanel(m_MainPanel); }
    public void ShowDevTools()
    {
        AudioManager.Instance?.PlayUI();
        m_DevToolsUI?.LoadValues();
        ShowPanel(m_DevToolsPanel);
    }

    // Ascunde toate panourile si afiseaza doar cel cerut
    private void ShowPanel(GameObject panel)
    {
        m_MainPanel.SetActive(false);
        m_AudioPanel.SetActive(false);
        m_GraphicsPanel.SetActive(false);
        m_GameplayPanel.SetActive(false);
        m_GeneralPanel.SetActive(false);
        if (m_DevToolsPanel != null) m_DevToolsPanel.SetActive(false);
        panel.SetActive(true);
    }

    // ---------- CALLBACKS UI ----------
    // Fiecare metoda: ignora apelul daca suntem in loading, reda sunet UI, salveaza in SettingsManager

    public void OnMusicVolume(float v)
    {
        if (m_IsLoading) return;
        AudioManager.Instance?.PlayUI();
        SettingsManager.Instance?.SetMusicVolume(v);
    }

    public void OnSfxVolume(float v)
    {
        if (m_IsLoading) return;
        AudioManager.Instance?.PlayUI();
        SettingsManager.Instance?.SetSfxVolume(v);
    }

    public void OnMute(bool v)
    {
        if (m_IsLoading) return;
        AudioManager.Instance?.PlayUI();
        SettingsManager.Instance?.SetMuted(v);
    }

    public void OnQuality(int v)
    {
        if (m_IsLoading) return;
        AudioManager.Instance?.PlayUI();
        SettingsManager.Instance?.SetQualityLevel(v);
    }

    public void OnFPS(int v)
    {
        if (m_IsLoading) return;
        AudioManager.Instance?.PlayUI();
        // Dropdown-ul are 4 optiuni fixe: index 0=30fps, 1=60fps, 2=120fps, 3=MAX(nelimitat)
        int[] options = { 30, 60, 120, 9999 };
        SettingsManager.Instance?.SetTargetFPS(options[v]);
    }

    public void OnShadows(bool v)
    {
        if (m_IsLoading) return;
        AudioManager.Instance?.PlayUI();
        SettingsManager.Instance?.SetShadows(v);
    }

    public void OnSensitivity(float v)
    {
        if (m_IsLoading) return;
        AudioManager.Instance?.PlayUI();
        SettingsManager.Instance?.SetSwipeSensitivity(v);
    }

    public void OnVibration(bool v)
    {
        if (m_IsLoading) return;
        AudioManager.Instance?.PlayUI();
        SettingsManager.Instance?.SetVibration(v);
    }

    // ---------- RESET PROGRES ----------

    // Butonul "Reset" din General: ascunde panoul si arata dialogul de confirmare
    public void OnResetProgress()
    {
        AudioManager.Instance?.PlayUI();
        if (m_ConfirmResetPanel != null)
        {
            m_GeneralPanel.SetActive(false);
            m_ConfirmResetPanel.SetActive(true);
        }
    }

    // Utilizatorul a confirmat resetul: executa reset, inchide dialogul si reincarca UI-ul
    public void OnConfirmReset()
    {
        AudioManager.Instance?.PlayUI();
        SettingsManager.Instance?.DoResetProgress();
        m_ConfirmResetPanel.SetActive(false);
        m_GeneralPanel.SetActive(true);
        LoadValuesIntoUI();
    }

    // Utilizatorul a anulat: inchide dialogul si revine la panoul General
    public void OnCancelReset()
    {
        AudioManager.Instance?.PlayUI();
        m_ConfirmResetPanel.SetActive(false);
        m_GeneralPanel.SetActive(true);
    }

    // Incarca valorile salvate din SettingsManager in elementele UI
    // Foloseste SetValueWithoutNotify / SetIsOnWithoutNotify pentru a nu declansa callback-urile
    private void LoadValuesIntoUI()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogWarning("SettingsUI: SettingsManager.Instance e null!");
            return;
        }

        m_IsLoading = true; // Blocheaza callback-urile pe durata incarcarii

        var s = SettingsManager.Instance;

        if (m_MusicSlider != null) m_MusicSlider.SetValueWithoutNotify(s.MusicVolume);
        if (m_SfxSlider != null) m_SfxSlider.SetValueWithoutNotify(s.SfxVolume);
        if (m_MuteToggle != null) m_MuteToggle.SetIsOnWithoutNotify(s.IsMuted);
        if (m_ShadowsToggle != null) m_ShadowsToggle.SetIsOnWithoutNotify(s.ShadowsEnabled);
        if (m_SensitivitySlider != null) m_SensitivitySlider.SetValueWithoutNotify(s.SwipeSensitivity);
        if (m_VibrationToggle != null) m_VibrationToggle.SetIsOnWithoutNotify(s.VibrationEnabled);
        if (m_QualityDropdown != null) m_QualityDropdown.SetValueWithoutNotify(s.QualityLevel);

        if (m_FpsDropdown != null)
        {
            // Gaseste indexul din dropdown care corespunde FPS-ului salvat (default: index 2 = 120fps)
            int[] fpsOptions = { 30, 60, 120, 9999 };
            int fpsIndex = System.Array.IndexOf(fpsOptions, s.TargetFPS);
            m_FpsDropdown.SetValueWithoutNotify(fpsIndex >= 0 ? fpsIndex : 2);
        }

        m_IsLoading = false; // Deblocheaza callback-urile
    }
}