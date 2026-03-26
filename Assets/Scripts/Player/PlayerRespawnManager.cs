using UnityEngine;

// Gestioneaza spawn-ul si respawn-ul jucatorului dupa fiecare moarte
public class PlayerRespawnManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private EntryPoint entryPoint;
    [SerializeField] private LevelGenerator levelGenerator;

    public GameObject CurrentPlayer { get; private set; }

    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        levelGenerator?.ResetMoveSpeed(); // Reseteaza viteza la fiecare respawn
        RushEffect.ResetObstacleBonus(); // Reseteaza bonusul de obstacole lovite in Rush

        if (CurrentPlayer != null)
            Destroy(CurrentPlayer);

        // Instantiaza jucatorul la punctul de spawn
        CurrentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        PlayerView playerView = CurrentPlayer.GetComponent<PlayerView>();
        PlayerCollisionHandler collisionHandler = CurrentPlayer.GetComponent<PlayerCollisionHandler>();
        RagdollController ragdoll = CurrentPlayer.GetComponent<RagdollController>();

        if (playerView != null && entryPoint != null)
            entryPoint.InitializePlayer(playerView);
        else
            Debug.LogError("PlayerRespawnManager: PlayerView or EntryPoint missing.");

        collisionHandler?.StartInvulnerability(); // Invulnerabilitate scurta dupa spawn
        ragdoll.Init(this); // Injecteaza referinta pentru a putea declansa urmatorul respawn
    }
}
