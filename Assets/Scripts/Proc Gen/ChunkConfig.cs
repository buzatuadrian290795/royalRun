using UnityEngine;

[CreateAssetMenu(fileName = "ChunkConfig", menuName = "Royal Run/Chunk Config")]
public class ChunkConfig : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject[] obstaclePrefabs;
    public GameObject applePrefab;
    public GameObject coinPrefab;

    [Header("Obstacle Spawn")]
    public int minObstaclesToSpawn = 0;
    public int maxObstaclesToSpawn = 2;

    [Header("Spawn Chances")]
    [Range(0f, 1f)] public float appleSpawnChance = 0.2f;
    [Range(0f, 1f)] public float coinSpawnChance = 0.5f;
    [Range(0f, 1f)] public float magnetSpawnChance = 0.05f;

    [Header("Magnet")]
    public GameObject magnetPrefab;
    public float magnetDuration = 10f;
    public float magnetAttractSpeed = 5f;
    public float magnetAttractRadius = 20f;

    [Header("Rush")]
    public GameObject rushPrefab;
    public float rushDuration = 10f;
    [Range(0f, 1f)] public float rushSpawnChance = 0.02f;

    [Header("Coins")]
    public float coinSeparationLength = 2f;
    public int minCoinsToSpawn = 1;
    public int maxCoinsToSpawn = 8;

}
