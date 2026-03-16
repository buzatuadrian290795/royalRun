using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private RoadView roadView;

    private PlayerController m_PlayerController;

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    public void InitializePlayer(PlayerView playerView)
    {
        if (m_PlayerController != null)
        {
            m_PlayerController.Dispose();
        }

        m_PlayerController = new PlayerController(playerView, roadView);
    }

    private void Update()
    {
        if (m_PlayerController != null)
        {
            m_PlayerController.Tick();
        }
    }

    private void OnDestroy()
    {
        if (m_PlayerController != null)
        {
            m_PlayerController.Dispose();
        }
    }
}