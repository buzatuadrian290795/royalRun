using UnityEngine;

public class Chunk : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject[] obstaclePrefabs;
    [SerializeField] GameObject applePrefab;
    [SerializeField] GameObject coinPrefab;

    [Header("Obstacle Spawn")]
    [SerializeField] private int minObstaclesToSpawn = 0;
    [SerializeField] private int maxObstaclesToSpawn = 2;

    [Header("Spawn Chances")]
    [SerializeField, Range(0f, 1f)] float appleSpawnChance = 0.3f;
    [SerializeField, Range(0f, 1f)] float coinSpawnChance = 0.5f;

    [Header("Coins")]
    [SerializeField] float coinSeparationLength = 2f;
    [SerializeField] int minCoinsToSpawn = 1;
    [SerializeField] int maxCoinsToSpawn = 6;

    [Header("Lanes")]
    [SerializeField] float[] obstacleLanes = { -3f, 0f, 3f };
    [SerializeField] float[] pickupLanes = { -3f, 0f, 3f };

    const int LaneCount = 3;

    int availableLaneCount;
    readonly int[] availableLanes = new int[LaneCount] { 0, 1, 2 };

    Transform cachedTransform;
    private float chunkX;
    private float chunkY;
    private float chunkZ;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Start()
    {
        CacheChunkPosition();
        ResetAvailableLanes();

        SpawnObstacles();
        SpawnApple();
        SpawnCoins();
    }

    private void CacheChunkPosition()
    {
        Vector3 position = cachedTransform.position;
        chunkX = position.x;
        chunkY = position.y;
        chunkZ = position.z;
    }

    private void ResetAvailableLanes()
    {
        availableLanes[0] = 0;
        availableLanes[1] = 1;
        availableLanes[2] = 2;
        availableLaneCount = LaneCount;
    }

    private void SpawnObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return;

        int maxSpawnAllowed = Mathf.Min(maxObstaclesToSpawn, obstacleLanes.Length, availableLaneCount);
        int minSpawnAllowed = Mathf.Clamp(minObstaclesToSpawn, 0, maxSpawnAllowed);

        int obstaclesToSpawn = Random.Range(minSpawnAllowed, maxSpawnAllowed + 1);

        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            if (availableLaneCount == 0)
                return;

            int lane = SelectRandomAvailableLane();

            if (Time.time < 1f && lane == 1)
            {
                PutLaneBack(lane);
                continue;
            }

            GameObject obstaclePrefab = GetRandomObstaclePrefab();
            if (obstaclePrefab == null)
                continue;

            SpawnObject(obstaclePrefab, obstacleLanes[lane], chunkY, chunkZ);
        }
    }

    private GameObject GetRandomObstaclePrefab()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return null;

        int randomIndex = Random.Range(0, obstaclePrefabs.Length);
        return obstaclePrefabs[randomIndex];
    }

    private void SpawnApple()
    {
        if (applePrefab == null)
            return;

        if (availableLaneCount == 0 || Random.value > appleSpawnChance)
            return;

        int lane = SelectRandomAvailableLane();
        SpawnObject(applePrefab, pickupLanes[lane], chunkY, chunkZ);
    }

    private void SpawnCoins()
    {
        if (availableLaneCount == 0 || Random.value > coinSpawnChance)
            return;

        int lane = SelectRandomAvailableLane();
        int coinsToSpawn = Random.Range(minCoinsToSpawn, maxCoinsToSpawn + 1);

        float startZ = chunkZ + (coinSeparationLength * 2f);
        float laneX = pickupLanes[lane];

        for (int i = 0; i < coinsToSpawn; i++)
        {
            float coinZ = startZ - (i * coinSeparationLength);
            SpawnObject(coinPrefab, laneX, chunkY, coinZ);
        }
    }

    private int SelectRandomAvailableLane()
    {
        int randomIndex = Random.Range(0, availableLaneCount);
        int lane = availableLanes[randomIndex];

        availableLaneCount--;
        availableLanes[randomIndex] = availableLanes[availableLaneCount];

        return lane;
    }

    private void PutLaneBack(int lane)
    {
        if (availableLaneCount >= LaneCount)
            return;

        availableLanes[availableLaneCount] = lane;
        availableLaneCount++;
    }

    private void SpawnObject(GameObject prefab, float x, float y, float z)
    {
        if (prefab == null)
            return;

        Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, cachedTransform);
    }
}