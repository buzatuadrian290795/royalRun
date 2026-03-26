using System.Collections.Generic;
using UnityEngine;

// Reprezinta un segment de drum; la spawn genereaza obstacole, mere si monede
public class Chunk : MonoBehaviour
{
    public static readonly List<Chunk> All = new List<Chunk>();

    // Offset-uri locale (fata de pivot) catre inceputul si sfarsitul mesh-ului pe axa Z
    // Calculat la Awake cu chunk-ul la origin, deci reprezinta offsetul real fata de pivot
    public float LocalMinZ { get; private set; }
    public float LocalMaxZ { get; private set; }
    public float Length => LocalMaxZ - LocalMinZ;
    public GameObject SourcePrefab { get; set; }

    private void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            LocalMinZ = 0f;
            LocalMaxZ = 16f;
            return;
        }

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        // bounds sunt in world space; scadem pozitia curenta ca sa obtinem offset-ul local
        LocalMinZ = combined.min.z - transform.position.z;
        LocalMaxZ = combined.max.z - transform.position.z;
    }

    private void OnEnable()  => All.Add(this);
    private void OnDisable() => All.Remove(this);

    [Header("Obstacle Spawn")]
    [SerializeField, Range(0f, 1f)] private float obstacleDensity = 0.5f;
    [SerializeField] private int minObstaclesToSpawn = 0;
    [SerializeField] private int maxObstaclesToSpawn = 2;

    const int LaneCount = 3;

    private LevelGenerator m_LevelGenerator;
    private ChunkConfig m_Config;

    int availableLaneCount;
    readonly int[] availableLanes = new int[LaneCount] { 0, 1, 2 };

    Transform cachedTransform;
    private float chunkY, chunkZ;

    private readonly List<GameObject> m_SpawnedObjects = new List<GameObject>();

    // Punct unic de initializare; apelat de LevelGenerator dupa pozitionare
    public void Initialize(LevelGenerator levelGenerator, ChunkConfig config, float density)
    {
        cachedTransform = transform;
        m_LevelGenerator = levelGenerator;
        m_Config = config;
        obstacleDensity = density;

        CacheChunkPosition();
        ResetAvailableLanes();

        SpawnObstacles();
        SpawnApple();
        SpawnMagnet();
        SpawnRush();
        SpawnCoins();
    }

    public void Cleanup()
    {
        foreach (var obj in m_SpawnedObjects)
            if (obj != null && obj.transform.parent == transform)
                Destroy(obj);
        m_SpawnedObjects.Clear();
    }

    private void SpawnObject(GameObject prefab, float x, float y, float z)
    {
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, cachedTransform);
        m_SpawnedObjects.Add(obj);

        obj.GetComponent<Pickup>()?.Init(m_LevelGenerator, m_Config);
    }

    private void CacheChunkPosition()
    {
        chunkY = cachedTransform.position.y;
        chunkZ = cachedTransform.position.z;
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
        if (m_Config == null || m_Config.obstaclePrefabs == null || m_Config.obstaclePrefabs.Length == 0) return;

        int maxAllowed = Mathf.Min(maxObstaclesToSpawn, m_LevelGenerator.LanePositions.Length, availableLaneCount);
        int minAllowed = Mathf.Clamp(minObstaclesToSpawn, 0, maxAllowed);
        int count = Mathf.RoundToInt(Mathf.Lerp(minAllowed, maxAllowed, obstacleDensity));

        for (int i = 0; i < count; i++)
        {
            if (availableLaneCount == 0) return;

            int lane = SelectRandomAvailableLane();

            if (Time.time < 1f && lane == 1)
            {
                PutLaneBack(lane);
                continue;
            }

            GameObject obstaclePrefab = m_Config.obstaclePrefabs[Random.Range(0, m_Config.obstaclePrefabs.Length)];
            SpawnObject(obstaclePrefab, m_LevelGenerator.LanePositions[lane], chunkY, chunkZ);
        }
    }

    private void SpawnApple()
    {
        if (m_Config == null || m_Config.applePrefab == null || availableLaneCount == 0) return;
        if (Random.value > m_Config.appleSpawnChance) return;
        SpawnObject(m_Config.applePrefab, m_LevelGenerator.LanePositions[SelectRandomAvailableLane()], chunkY, chunkZ);
    }

    private void SpawnMagnet()
    {
        if (m_Config == null || m_Config.magnetPrefab == null || availableLaneCount == 0) return;
        if (Random.value > m_Config.magnetSpawnChance) return;
        SpawnObject(m_Config.magnetPrefab, m_LevelGenerator.LanePositions[SelectRandomAvailableLane()], chunkY, chunkZ);
    }

    private void SpawnRush()
    {
        if (m_Config == null || m_Config.rushPrefab == null || availableLaneCount == 0) return;
        if (RushEffect.Instance != null && RushEffect.Instance.IsActive) return;
        if (Random.value > m_Config.rushSpawnChance) return;
        SpawnObject(m_Config.rushPrefab, m_LevelGenerator.LanePositions[SelectRandomAvailableLane()], chunkY, chunkZ);
    }

    private void SpawnCoins()
    {
        if (m_Config == null || m_Config.coinPrefab == null || availableLaneCount == 0) return;
        if (Random.value > m_Config.coinSpawnChance) return;

        int lane = SelectRandomAvailableLane();
        int coinsToSpawn = Random.Range(m_Config.minCoinsToSpawn, m_Config.maxCoinsToSpawn + 1);
        float startZ = chunkZ + (m_Config.coinSeparationLength * 2f);
        float laneX = m_LevelGenerator.LanePositions[lane];

        for (int i = 0; i < coinsToSpawn; i++)
            SpawnObject(m_Config.coinPrefab, laneX, chunkY, startZ - (i * m_Config.coinSeparationLength));
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
        if (availableLaneCount >= LaneCount) return;
        availableLanes[availableLaneCount] = lane;
        availableLaneCount++;
    }
}
