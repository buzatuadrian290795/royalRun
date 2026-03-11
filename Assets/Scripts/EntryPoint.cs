using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private PlayerView playerView;
    [SerializeField] private RoadView roadView;

    private PlayerController m_PlayerController;

    private void Start()
    {
        m_PlayerController = new PlayerController(playerView, roadView);
    }

    private void Update()
    {
        m_PlayerController.Tick();
    }

    private void OnDestroy()
    {
        m_PlayerController.Dispose();
    }
}
