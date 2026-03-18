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
        levelGenerator?.ResetMoveSpeed();
        //distanceMeterUI?.ResetDistance();

        if (CurrentPlayer != null)
            Destroy(CurrentPlayer);

        CurrentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        PlayerView playerView = CurrentPlayer.GetComponent<PlayerView>();
        PlayerCollisionHandler collisionHandler = CurrentPlayer.GetComponent<PlayerCollisionHandler>();
        RagdollController ragdoll = CurrentPlayer.GetComponent<RagdollController>();

        if (playerView != null && entryPoint != null)
            entryPoint.InitializePlayer(playerView);
        else
            Debug.LogError("PlayerRespawnManager: PlayerView or EntryPoint missing.");

        collisionHandler?.StartInvulnerability();

        ragdoll.Init(this);
    }
}