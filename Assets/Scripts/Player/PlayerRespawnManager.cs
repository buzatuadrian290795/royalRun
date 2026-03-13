using UnityEngine;

public class PlayerRespawnManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private EntryPoint entryPoint;

    public GameObject CurrentPlayer { get; private set; }

    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (CurrentPlayer != null)
        {
            Destroy(CurrentPlayer);
        }

        CurrentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        PlayerView playerView = CurrentPlayer.GetComponent<PlayerView>();

        if (playerView != null && entryPoint != null)
        {
            entryPoint.InitializePlayer(playerView);
        }
        else
        {
            Debug.LogError("PlayerRespawnManager: PlayerView or EntryPoint missing.");
        }
    }
}