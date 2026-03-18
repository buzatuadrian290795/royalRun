using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

// Singleton care stocheaza, aplica si salveaza toate setarile jocului
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    // Valorile curente ale setarilor (private, expuse doar prin proprietati read-only)
    private float m_MusicVolume;
    private float m_SfxVolume;
    private bool m_IsMuted;
    private int m_QualityLevel;
    private int m_TargetFPS;
    private bool m_ShadowsEnabled;
    private float m_SwipeSensitivity;
    private bool m_VibrationEnabled;

    // Proprietati read-only accesibile din alte scripturi
    public float MusicVolume => m_MusicVolume;
    public float SfxVolume => m_SfxVolume;
    public bool IsMuted => m_IsMuted;
    public int QualityLevel => m_QualityLevel;
    public int TargetFPS => m_TargetFPS;
    public bool ShadowsEnabled => m_ShadowsEnabled;
    public float SwipeSensitivity => m_SwipeSensitivity;
    public bool VibrationEnabled => m_VibrationEnabled;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load(); // Incarca setarile salvate imediat la pornire
    }

    // ---------- AUDIO ----------
    // Fiecare Set*: actualizeaza campul intern, aplica in engine, salveaza in PlayerPrefs

    public void SetMusicVolume(float value)
    {
        m_MusicVolume = value;
        AudioManager.Instance?.SetMusicVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSfxVolume(float value)
    {
        m_SfxVolume = value;
        AudioManager.Instance?.SetSfxVolume(value);
        PlayerPrefs.SetFloat("SfxVolume", value);
    }

    public void SetMuted(bool value)
    {
        m_IsMuted = value;
        AudioManager.Instance?.SetMuted(value);
        PlayerPrefs.SetInt("Muted", value ? 1 : 0); // bool salvat ca 0/1
    }

    // ---------- GRAPHICS ----------

    public void SetQualityLevel(int value)
    {
        m_QualityLevel = value;
        QualitySettings.SetQualityLevel(value); // Aplica preset Unity
        PlayerPrefs.SetInt("QualityLevel", value);
    }

    public void SetTargetFPS(int value)
    {
        m_TargetFPS = value;
        Application.targetFrameRate = value;
        PlayerPrefs.SetInt("TargetFPS", value);
    }

    public void SetShadows(bool value)
    {
        m_ShadowsEnabled = value;
        // Activeaza/dezactiveaza umbrele modificand distanta de randare in URP
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
            urpAsset.shadowDistance = value ? 50f : 0f;
        PlayerPrefs.SetInt("Shadows", value ? 1 : 0);
    }

    // ---------- GAMEPLAY ----------

    public void SetSwipeSensitivity(float value)
    {
        m_SwipeSensitivity = value;
        PlayerPrefs.SetFloat("SwipeSensitivity", value);
    }

    public void SetVibration(bool value)
    {
        m_VibrationEnabled = value;
        PlayerPrefs.SetInt("Vibration", value ? 1 : 0);
    }

    // ---------- GENERAL ----------

    public void DoResetProgress()
    {
        PlayerPrefs.DeleteAll(); // Sterge toate datele salvate

        // Reseteaza distanta parcursa daca componenta exista in scena
        var distanceMeter = FindFirstObjectByType<DistanceMeterUI>();
        if (distanceMeter != null)
            distanceMeter.ResetDistance();

        Load(); // Reincarca valorile default
    }

    // ---------- SAVE / LOAD ----------

    // Citeste valorile din PlayerPrefs (al doilea parametru = valoarea default daca nu exista)
    private void Load()
    {
        m_MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        m_SfxVolume = PlayerPrefs.GetFloat("SfxVolume", 0.5f);
        m_IsMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        m_QualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
        m_TargetFPS = PlayerPrefs.GetInt("TargetFPS", 60);
        m_ShadowsEnabled = PlayerPrefs.GetInt("Shadows", 1) == 1;
        m_SwipeSensitivity = PlayerPrefs.GetFloat("SwipeSensitivity", 35f);
        m_VibrationEnabled = PlayerPrefs.GetInt("Vibration", 1) == 1;
        Apply();
    }

    // Aplica setarile de grafica in engine (AudioManager nu e inca disponibil la Load din Awake)
    private void Apply()
    {
        Application.targetFrameRate = m_TargetFPS;
        QualitySettings.SetQualityLevel(m_QualityLevel);

        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
            urpAsset.shadowDistance = m_ShadowsEnabled ? 50f : 0f;
    }
}