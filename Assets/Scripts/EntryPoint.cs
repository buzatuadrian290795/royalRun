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
        Application.targetFrameRate = 60;
    }

    // Apelat de PlayerRespawnManager la fiecare (re)spawn; inlocuieste controllerul vechi
    public void InitializePlayer(PlayerView playerView)
    {
        m_PlayerController?.Dispose();
        m_PlayerController = new PlayerController(playerView, roadView, m_LevelGenerator);
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