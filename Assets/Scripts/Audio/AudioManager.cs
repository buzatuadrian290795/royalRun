using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource m_SfxSource;
    [SerializeField] private AudioSource m_MusicSource;

    [Header("Music")]
    [SerializeField] private AudioClip[] m_MusicClips;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip[] m_AppleClips;
    [SerializeField] private AudioClip[] m_CoinClips;
    [SerializeField] private AudioClip[] m_AuchClips;
    [SerializeField] private AudioClip[] m_LeftRightClips;
    [SerializeField] private AudioClip[] m_JumpClips;
    [SerializeField] private AudioClip[] m_RollClips;

    // Cached last index per array to avoid repeating the same clip twice in a row
    private int m_LastAppleIndex = -1;
    private int m_LastCoinIndex = -1;
    private int m_LastAuchIndex = -1;
    private int m_LastSwipeIndex = -1;
    private int m_LastJumpIndex = -1;
    private int m_LastRollIndex = -1;
    private int m_LastMusicIndex = -1;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplySettings();
        PlayRandomMusic();
    }

    // Applies saved volume/mute settings on startup
    private void ApplySettings()
    {
        if (SettingsManager.Instance == null) return;
        m_MusicSource.volume = SettingsManager.Instance.MusicVolume;
        m_SfxSource.volume = SettingsManager.Instance.SfxVolume;
        m_MusicSource.mute = SettingsManager.Instance.IsMuted;
        m_SfxSource.mute = SettingsManager.Instance.IsMuted;
    }

    // ---------- MUSIC ----------
    private void PlayRandomMusic()
    {
        if (m_MusicClips == null || m_MusicClips.Length == 0) return;
        m_LastMusicIndex = GetRandomIndex(m_MusicClips.Length, m_LastMusicIndex);
        m_MusicSource.clip = m_MusicClips[m_LastMusicIndex];
        m_MusicSource.loop = true;
        m_MusicSource.Play();
    }

    // ---------- VOLUME CONTROL ----------
    public void SetMusicVolume(float value) => m_MusicSource.volume = value;
    public void SetSfxVolume(float value) => m_SfxSource.volume = value;
    public void SetMuted(bool value)
    {
        m_MusicSource.mute = value;
        m_SfxSource.mute = value;
    }

    // ---------- SFX ----------
    public void PlayApple() => PlayRandom(m_AppleClips, ref m_LastAppleIndex);
    public void PlayCoin() => PlayRandom(m_CoinClips, ref m_LastCoinIndex);
    public void PlayAuch() => PlayRandom(m_AuchClips, ref m_LastAuchIndex);
    public void PlaySwipe() => PlayRandom(m_LeftRightClips, ref m_LastSwipeIndex);
    public void PlayJump() => PlayRandom(m_JumpClips, ref m_LastJumpIndex);
    public void PlayRoll() => PlayRandom(m_RollClips, ref m_LastRollIndex);

    private void PlayRandom(AudioClip[] clips, ref int lastIndex)
    {
        if (clips == null || clips.Length == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"AudioManager: clips array gol!");
#endif
            return;
        }

        lastIndex = GetRandomIndex(clips.Length, lastIndex);
        m_SfxSource.PlayOneShot(clips[lastIndex]);
    }

    // Returns a random index different from the last one (avoids immediate repeats)
    private static int GetRandomIndex(int length, int lastIndex)
    {
        if (length == 1) return 0;
        int index;
        do { index = Random.Range(0, length); }
        while (index == lastIndex);
        return index;
    }
}