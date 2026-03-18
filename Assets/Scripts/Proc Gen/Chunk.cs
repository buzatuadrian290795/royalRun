using System.Collections.Generic;
using UnityEngine;

// Reprezinta un segment de drum; la spawn genereaza obstacole, mere si monede
public class Chunk : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject[] obstaclePrefabs;
    [SerializeField] GameObject applePrefab;
    [SerializeField] GameObject coinPrefab;

    [Header("Obstacle Spawn")]
    [SerializeField, Range(0f, 1f)] private float obstacleDensity = 0.5f; // 0=minim, 1=maxim
    [SerializeField] private int minObstaclesToSpawn = 0;
    [SerializeField] private int maxObstaclesToSpawn = 2;

    [Header("Spawn Chances")]
    [SerializeField, Range(0f, 1f)] float appleSpawnChance = 0.3f;
    [SerializeField, Range(0f, 1f)] float coinSpawnChance = 0.5f;

    [Header("Coins")]
    [SerializeField] float coinSeparationLength = 2f; // Distanta intre monede consecutive
    [SerializeField] int minCoinsToSpawn = 1;
    [SerializeField] int maxCoinsToSpawn = 6;

    [Header("Lanes")]
    [SerializeField] float[] obstacleLanes = { -3f, 0f, 3f };
    [SerializeField] float[] pickupLanes = { -3f, 0f, 3f };

    const int LaneCount = 3;

    private LevelGenerator m_LevelGenerator;

    // Pool de benzi disponibile pentru spawn (se scad pe masura ce sunt ocupate)
    int availableLaneCount;
    readonly int[] availableLanes = new int[LaneCount] { 0, 1, 2 };

    Transform cachedTransform;
    private float chunkX, chunkY, chunkZ; // Pozitia chunk-ului, cache-uita la Initialize

    // Lista obiectelor spawned dinamic; folosita de Cleanup pentru a le distruge la reciclare
    private readonly List<GameObject> m_SpawnedObjects = new List<GameObject>();

    // Apelat din LevelGenerator pentru a ajusta dificultatea dinamic
    public void SetObstacleSpawnRange(int minObstacles, int maxObstacles)
    {
        minObstaclesToSpawn = Mathf.Max(0, minObstacles);
        maxObstaclesToSpawn = Mathf.Max(minObstaclesToSpawn, maxObstacles);
    }

    private void Awake()
    {
        cachedTransform = transform;
    }

    // Injecteaza referinta la LevelGenerator (necesar pentru Pickup-uri)
    public void Init(LevelGenerator levelGenerator)
    {
        m_LevelGenerator = levelGenerator;
    }

    // Initializeaza chunk-ul: pozitioneaza, genereaza obstacole si pickup-uri
    // Apelat de LevelGenerator dupa ce chunk-ul a fost pozitionat (inclusiv la refolosire din pool)
    public void Initialize()
    {
        CacheChunkPosition();
        ResetAvailableLanes();

        SpawnObstacles();
        SpawnApple();
        SpawnCoins();
    }

    // Distruge toate obiectele spawned dinamic si goleste lista (pregateste chunk-ul pentru refolosire)
    public void Cleanup()
    {
        foreach (var obj in m_SpawnedObjects)
            if (obj != null) Destroy(obj);
        m_SpawnedObjects.Clear();
    }

    // Instantiaza un prefab la pozitia data, il urmareste pentru Cleanup si ii transmite LevelGenerator daca e Pickup
    private void SpawnObject(GameObject prefab, float x, float y, float z)
    {
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, new Vector3(x, y, z),
                                     Quaternion.identity, cachedTransform);
        m_SpawnedObjects.Add(obj);

        Pickup pickup = obj.GetComponent<Pickup>();
        if (pickup != null)
            pickup.Init(m_LevelGenerator);
    }

    private void CacheChunkPosition()
    {
        Vector3 position = cachedTransform.position;
        chunkX = position.x;
        chunkY = position.y;
        chunkZ = position.z;
    }

    // Reseteaza lista de benzi disponibile la toate 3 benzile
    private void ResetAvailableLanes()
    {
        availableLanes[0] = 0;
        availableLanes[1] = 1;
        availableLanes[2] = 2;
        availableLaneCount = LaneCount;
    }

    private void SpawnObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        // Calculeaza numarul de obstacole prin lerp intre min si max dupa densitate
        int maxSpawnAllowed = Mathf.Min(maxObstaclesToSpawn, obstacleLanes.Length, availableLaneCount);
        int minSpawnAllowed = Mathf.Clamp(minObstaclesToSpawn, 0, maxSpawnAllowed);
        int obstaclesToSpawn = Mathf.RoundToInt(
            Mathf.Lerp(minSpawnAllowed, maxSpawnAllowed, obstacleDensity)
        );

        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            if (availableLaneCount == 0) return;

            int lane = SelectRandomAvailableLane();

            // La inceput (primele secunde), evita banda din mijloc (lane 1) → start mai usor
            if (Time.time < 1f && lane == 1)
            {
                PutLaneBack(lane);
                continue;
            }

            GameObject obstaclePrefab = GetRandomObstaclePrefab();
            if (obstaclePrefab == null) continue;

            SpawnObject(obstaclePrefab, obstacleLanes[lane], chunkY, chunkZ);
        }
    }

    private GameObject GetRandomObstaclePrefab()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return null;
        return obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
    }

    private void SpawnApple()
    {
        if (applePrefab == null || availableLaneCount == 0) return;
        if (Random.value > appleSpawnChance) return;

        int lane = SelectRandomAvailableLane();
        SpawnObject(applePrefab, pickupLanes[lane], chunkY, chunkZ);
    }

    private void SpawnCoins()
    {
        if (availableLaneCount == 0 || Random.value > coinSpawnChance) return;

        int lane = SelectRandomAvailableLane();
        int coinsToSpawn = Random.Range(minCoinsToSpawn, maxCoinsToSpawn + 1);

        // Monedele pornesc din fata chunk-ului si merg spre spate cu coinSeparationLength intre ele
        float startZ = chunkZ + (coinSeparationLength * 2f);
        float laneX = pickupLanes[lane];

        for (int i = 0; i < coinsToSpawn; i++)
            SpawnObject(coinPrefab, laneX, chunkY, startZ - (i * coinSeparationLength));
    }

    // Alege o banda aleatorie din cele disponibile si o scoate din pool (Fisher-Yates partial)
    private int SelectRandomAvailableLane()
    {
        int randomIndex = Random.Range(0, availableLaneCount);
        int lane = availableLanes[randomIndex];

        // Inlocuieste banda aleasa cu ultima din lista, apoi scurteaza lista
        availableLaneCount--;
        availableLanes[randomIndex] = availableLanes[availableLaneCount];

        return lane;
    }

    // Returneaza o banda in pool (folosit cand banda aleasa e respinsa)
    private void PutLaneBack(int lane)
    {
        if (availableLaneCount >= LaneCount) return;
        availableLanes[availableLaneCount] = lane;
        availableLaneCount++;
    }

    public void SetObstacleDensity(float density)
    {
        obstacleDensity = Mathf.Clamp01(density);
    }
}
