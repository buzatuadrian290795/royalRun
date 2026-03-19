using UnityEngine;

// Punctul de intrare al jocului: initializeaza ecranul si gestioneaza PlayerController
public class EntryPoint : MonoBehaviour
{
    [SerializeField] private RoadView roadView;
    [SerializeField] private LevelGenerator m_LevelGenerator;

    private PlayerController m_PlayerController;

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // Ecranul nu se stinge in timpul jocului
        QualitySettings.vSyncCount = 0;

        if (!PlayerPrefs.HasKey("GraphicsInitialized"))
        {
            SetDefaultGraphics();
            PlayerPrefs.SetInt("GraphicsInitialized", 1);
            PlayerPrefs.Save();
        }
    }

    private void SetDefaultGraphics()
    {
        // Quality High
        string[] names = QualitySettings.names;
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i].ToLower().Contains("high"))
            {
                QualitySettings.SetQualityLevel(i, true);
                break;
            }
        }

        Application.targetFrameRate = 120;
        PlayerPrefs.SetInt("TargetFPS", 120);
        QualitySettings.vSyncCount = 0;
        QualitySettings.shadows = ShadowQuality.All;
    }

    // Apelat de PlayerRespawnManager la fiecare (re)spawn; inlocuieste controllerul vechi
    public void InitializePlayer(PlayerView playerView)
    {
        m_PlayerController?.Dispose();
        RoadView rv = roadView != null ? roadView : m_LevelGenerator?.RoadView;
        m_PlayerController = new PlayerController(playerView, rv, m_LevelGenerator);
    }

    private void Update()
    {
        m_PlayerController?.Tick();
    }

    private void OnDestroy()
    {
        m_PlayerController?.Dispose();
    }
}