using UnityEngine;

public class PlayerRespawnManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private EntryPoint entryPoint;
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private DistanceMeterUI distanceMeterUI;

    public GameObject CurrentPlayer { get; private set; }

    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (levelGenerator != null)
        {
            levelGenerator.ResetMoveSpeed();
        }

        if (distanceMeterUI != null)
        {
            distanceMeterUI.ResetDistance();
        }

        if (CurrentPlayer != null)
        {
            Destroy(CurrentPlayer);
        }

        CurrentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        PlayerView playerView = CurrentPlayer.GetComponent<PlayerView>();
        PlayerCollisionHandler collisionHandler = CurrentPlayer.GetComponent<PlayerCollisionHandler>();

        if (playerView != null && entryPoint != null)
        {
            entryPoint.InitializePlayer(playerView);
        }
        else
        {
            Debug.LogError("PlayerRespawnManager: PlayerView or EntryPoint missing.");
        }

        if (collisionHandler != null)
        {
            collisionHandler.StartInvulnerability();
        }
        else
        {
            Debug.LogError("PlayerRespawnManager: PlayerCollisionHandler missing.");
        }
    }
}