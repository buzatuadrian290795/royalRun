using UnityEngine;

// Singleton care gestioneaza toata muzica si efectele sonore din joc
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource m_SfxSource;    // Sursa pentru efecte sonore
    [SerializeField] private AudioSource m_MusicSource;  // Sursa pentru muzica de fundal

    [Header("Music")]
    [SerializeField] private AudioClip[] m_MusicClips;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip[] m_AppleClips;
    [SerializeField] private AudioClip[] m_CoinClips;
    [SerializeField] private AudioClip[] m_AuchClips;
    [SerializeField] private AudioClip[] m_LeftRightClips;
    [SerializeField] private AudioClip[] m_JumpClips;
    [SerializeField] private AudioClip[] m_RollClips;

    [Header("UI SFX")]
    [SerializeField] private AudioClip[] m_UIClips;

    // Retine ultimul index redat pentru fiecare categorie (evita repetitii consecutive)
    private int m_LastAppleIndex = -1;
    private int m_LastCoinIndex = -1;
    private int m_LastAuchIndex = -1;
    private int m_LastSwipeIndex = -1;
    private int m_LastJumpIndex = -1;
    private int m_LastRollIndex = -1;
    private int m_LastMusicIndex = -1;
    private int m_LastUIIndex = -1;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; } // Distruge duplicatele
        Instance = this;
        transform.SetParent(null);        // Scoate din ierarhie (sa nu fie sters de parinte)
        DontDestroyOnLoad(gameObject);    // Persista intre scene
    }

    private void Start()
    {
        ApplySettings();    // Aplica volumul/mute din setari
        PlayRandomMusic();  // Porneste muzica de fundal
    }

    // Preia volumul si starea mute din SettingsManager si le aplica pe ambele surse
    private void ApplySettings()
    {
        if (SettingsManager.Instance == null) return;
        m_MusicSource.volume = SettingsManager.Instance.MusicVolume;
        m_SfxSource.volume = SettingsManager.Instance.SfxVolume;
        m_MusicSource.mute = SettingsManager.Instance.IsMuted;
        m_SfxSource.mute = SettingsManager.Instance.IsMuted;
    }

    // ---------- MUSIC ----------

    // Alege un clip de muzica aleatoriu (diferit de cel anterior) si il reda in loop
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
    // Fiecare metoda publica apeleaza PlayRandom cu array-ul si indexul corespunzator

    public void PlayApple() => PlayRandom(m_AppleClips, ref m_LastAppleIndex);
    public void PlayCoin() => PlayRandom(m_CoinClips, ref m_LastCoinIndex);
    public void PlayAuch() => PlayRandom(m_AuchClips, ref m_LastAuchIndex);
    public void PlaySwipe() => PlayRandom(m_LeftRightClips, ref m_LastSwipeIndex);
    public void PlayJump() => PlayRandom(m_JumpClips, ref m_LastJumpIndex);
    public void PlayRoll() => PlayRandom(m_RollClips, ref m_LastRollIndex);
    public void PlayUI() => PlayRandom(m_UIClips, ref m_LastUIIndex);

    // Reda un clip aleatoriu din array (diferit de ultimul), folosind PlayOneShot
    // (PlayOneShot permite suprapunerea sunetelor, spre deosebire de Play)
    private void PlayRandom(AudioClip[] clips, ref int lastIndex)
    {
        if (clips == null || clips.Length == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"AudioManager: clips array gol!"); // Vizibil doar in Editor
#endif
            return;
        }
        lastIndex = GetRandomIndex(clips.Length, lastIndex);
        m_SfxSource.PlayOneShot(clips[lastIndex]);
    }

    // Genereaza un index aleatoriu diferit de cel anterior (evita acelasi sunet de doua ori la rand)
    private static int GetRandomIndex(int length, int lastIndex)
    {
        if (length == 1) return 0; // Un singur clip disponibil, returneaza direct
        int index;
        do { index = Random.Range(0, length); }
        while (index == lastIndex);
        return index;
    }
}