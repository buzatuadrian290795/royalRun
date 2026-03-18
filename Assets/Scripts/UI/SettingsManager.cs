using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private float m_MusicVolume;
    private float m_SfxVolume;
    private bool m_IsMuted;
    private int m_QualityLevel;
    private int m_TargetFPS;
    private bool m_ShadowsEnabled;
    private float m_SwipeSensitivity;
    private bool m_VibrationEnabled;

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
        Load();
    }

    // ---------- AUDIO ----------
    public void SetMusicVolume(float value)
    {
        m_MusicVolume = value;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSfxVolume(float value)
    {
        m_SfxVolume = value;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSfxVolume(value);
        PlayerPrefs.SetFloat("SfxVolume", value);
    }

    public void SetMuted(bool value)
    {
        m_IsMuted = value;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMuted(value);
        PlayerPrefs.SetInt("Muted", value ? 1 : 0);
    }

    // ---------- GRAPHICS ----------
    public void SetQualityLevel(int value)
    {
        m_QualityLevel = value;
        QualitySettings.SetQualityLevel(value);
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
        QualitySettings.shadows = value ? ShadowQuality.All : ShadowQuality.Disable;
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
        PlayerPrefs.DeleteAll();
        Load();
    }

    // ---------- SAVE / LOAD ----------
    private void Load()
    {
        m_MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        m_SfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
        m_IsMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        m_QualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
        m_TargetFPS = PlayerPrefs.GetInt("TargetFPS", 60);
        m_ShadowsEnabled = PlayerPrefs.GetInt("Shadows", 1) == 1;
        m_SwipeSensitivity = PlayerPrefs.GetFloat("SwipeSensitivity", 35f);
        m_VibrationEnabled = PlayerPrefs.GetInt("Vibration", 1) == 1;
        Apply();
    }

    private void Apply()
    {
        Application.targetFrameRate = m_TargetFPS;
        QualitySettings.SetQualityLevel(m_QualityLevel);
        QualitySettings.shadows = m_ShadowsEnabled ? ShadowQuality.All : ShadowQuality.Disable;
    }
}