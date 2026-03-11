using UnityEngine;

public class Chunk : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject fencePrefab;
    [SerializeField] GameObject applePrefab;
    [SerializeField] GameObject coinPrefab;

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
    float chunkX;
    float chunkY;
    float chunkZ;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Start()
    {
        CacheChunkPosition();
        ResetAvailableLanes();

        SpawnFences();
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

    private void SpawnFences()
    {
        int fencesToSpawn = Random.Range(0, obstacleLanes.Length);

        for (int i = 0; i < fencesToSpawn; i++)
        {
            if (availableLaneCount == 0)
                return;

            int lane = SelectRandomAvailableLane();

            // Evită obstacol pe mijloc chiar la începutul jocului.
            // Punem lane-ul la loc ca să nu-l "pierdem".
            if (Time.time < 1f && lane == 1)
            {
                PutLaneBack(lane);
                continue;
            }

            SpawnObject(fencePrefab, obstacleLanes[lane], chunkY, chunkZ);
        }
    }

    private void SpawnApple()
    {
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
        Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, cachedTransform);
    }
}