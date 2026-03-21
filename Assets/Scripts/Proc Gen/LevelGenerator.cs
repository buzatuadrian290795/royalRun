using System.Collections.Generic;
using UnityEngine;

// Genereaza, muta si distruge chunk-urile de drum si cladirile laterale; controleaza viteza si dificultatea
public class LevelGenerator : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] ChunkConfig chunkConfig;
    [SerializeField] BuildingChunkConfig buildingChunkConfig;

    [Header("References")]
    [SerializeField] CameraController cameraController;
    [SerializeField] RoadView roadView;
    [SerializeField] Transform chunkParent;
    [SerializeField] Transform buildingParent;

    [Header("Settings")]
    [SerializeField] int startingChunksAmount = 16;
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float minMoveSpeed = 8f;
    [SerializeField] float maxMoveSpeed = 44f;
    [SerializeField, Range(0f, 1f)] float obstacleDensity = 0.25f;

    private List<Chunk> m_Chunks = new List<Chunk>();
    private List<GameObject> m_Buildings = new List<GameObject>();
    private Camera m_MainCamera;
    private int m_LastChunkIndex = -1;
    private int m_LastBuildingIndexLeft = -1;
    private int m_LastBuildingIndexRight = -1;
    private float m_LastChunkLength = 16f; // fallback pana la primul chunk spawnat

    // Object pools
    private readonly Dictionary<GameObject, Queue<Chunk>> m_ChunkPool = new Dictionary<GameObject, Queue<Chunk>>();
    private readonly Dictionary<GameObject, Queue<GameObject>> m_BuildingPool = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> m_BuildingToPrefab = new Dictionary<GameObject, GameObject>();

    public float MoveSpeed => moveSpeed;
    public float ObstacleDensity => obstacleDensity;
    public float ChunkLength => m_LastChunkLength;
    public int StartingChunksAmount => startingChunksAmount;
    public RoadView RoadView => roadView;
    public float MaxMoveSpeed => maxMoveSpeed;
    public float[] LanePositions => roadView != null ? roadView.LanePositions : new float[] { -2.5f, 0f, 2.5f };

    public int CoinMultiplier
    {
        get
        {
            int speedMultiplier;
            if (moveSpeed >= 44f) speedMultiplier = 10;
            else if (moveSpeed >= 40f) speedMultiplier = 9;
            else if (moveSpeed >= 36f) speedMultiplier = 8;
            else if (moveSpeed >= 32f) speedMultiplier = 7;
            else if (moveSpeed >= 28f) speedMultiplier = 6;
            else if (moveSpeed >= 24f) speedMultiplier = 5;
            else if (moveSpeed >= 20f) speedMultiplier = 4;
            else if (moveSpeed >= 16f) speedMultiplier = 3;
            else if (moveSpeed >= 12f) speedMultiplier = 2;
            else speedMultiplier = 1;

            return speedMultiplier + RushEffect.ObstacleHitBonus;
        }
    }

    private void Awake()
    {
        m_MainCamera = Camera.main;
        if (roadView == null) roadView = FindFirstObjectByType<RoadView>();
        SpawnLeadChunk();
        SpawnStartingChunks();
    }

    private void FixedUpdate()
    {
        MoveChunks();
        MoveBuildings();
    }

    public void SetObstacleDensity(float value) => obstacleDensity = Mathf.Clamp01(value);
    public void SetStartingChunksAmount(int value) => startingChunksAmount = Mathf.Max(1, value);

    public void ChangeChunkMoveSpeed(float speedAmount)
    {
        moveSpeed = Mathf.Min(moveSpeed + speedAmount, maxMoveSpeed);
        cameraController?.ChangeCameraFOV(speedAmount);
    }

    public void SetMoveSpeedToMax()
    {
        moveSpeed = maxMoveSpeed;
        cameraController?.ChangeCameraFOV(maxMoveSpeed);
    }

    public void SetMoveSpeed(float value)
    {
        moveSpeed = Mathf.Clamp(value, minMoveSpeed, maxMoveSpeed);
        cameraController?.ResetFOV();
    }

    public void ResetMoveSpeed()
    {
        moveSpeed = minMoveSpeed;
        cameraController?.ResetFOV();
    }

    public void RespawnChunks()
    {
        foreach (var chunk in m_Chunks) ReturnChunkToPool(chunk);
        m_Chunks.Clear();
        m_LastChunkIndex = -1;

        foreach (var building in m_Buildings) { if (building != null) ReturnBuildingToPool(building); }
        m_Buildings.Clear();
        m_LastBuildingIndexLeft = -1;
        m_LastBuildingIndexRight = -1;

        SpawnStartingChunks();
    }

    // --- Chunks ---

    // Spawneaza un chunk inainte de toate celelalte, lipit in spatele pozitiei de start
    private void SpawnLeadChunk()
    {
        if (chunkConfig == null || chunkConfig.chunkPrefabs == null || chunkConfig.chunkPrefabs.Length == 0) return;

        int index = PickRandom(chunkConfig.chunkPrefabs.Length, ref m_LastChunkIndex);
        GameObject prefab = chunkConfig.chunkPrefabs[index];
        if (prefab == null) return;

        Chunk chunk = GetChunkFromPool(prefab);

        float spawnZ = transform.position.z - chunk.LocalMaxZ;
        chunk.transform.position = new Vector3(transform.position.x, transform.position.y, spawnZ);
        chunk.gameObject.SetActive(true);
        chunk.Initialize(this, chunkConfig, obstacleDensity);
        m_Chunks.Add(chunk);
        m_LastChunkLength = chunk.Length;

        SpawnBuildingPair(spawnZ);
    }

    private void SpawnStartingChunks()
    {
        float spawnThreshold = m_MainCamera.transform.position.z + m_MainCamera.farClipPlane;

        do { SpawnChunk(); }
        while (m_Chunks.Count < startingChunksAmount ||
               m_Chunks[m_Chunks.Count - 1].transform.position.z + m_Chunks[m_Chunks.Count - 1].LocalMaxZ < spawnThreshold);
    }

    private void SpawnChunk()
    {
        if (chunkConfig == null || chunkConfig.chunkPrefabs == null || chunkConfig.chunkPrefabs.Length == 0)
        {
            Debug.LogError("LevelGenerator: ChunkConfig nu are chunkPrefabs.");
            return;
        }

        int index = PickRandom(chunkConfig.chunkPrefabs.Length, ref m_LastChunkIndex);
        GameObject prefab = chunkConfig.chunkPrefabs[index];
        if (prefab == null) { Debug.LogError($"LevelGenerator: chunkPrefab null la index {index}."); return; }

        Chunk chunk = GetChunkFromPool(prefab);

        float spawnZ = CalculateSpawnPositionZ(chunk);
        chunk.transform.position = new Vector3(transform.position.x, transform.position.y, spawnZ);
        chunk.gameObject.SetActive(true);
        chunk.Initialize(this, chunkConfig, obstacleDensity);
        m_Chunks.Add(chunk);
        m_LastChunkLength = chunk.Length;

        SpawnBuildingPair(spawnZ);
    }

    private void MoveChunks()
    {
        float cameraZ = m_MainCamera.transform.position.z;
        bool needsRefill = false;

        for (int i = m_Chunks.Count - 1; i >= 0; i--)
        {
            Chunk chunk = m_Chunks[i];
            chunk.transform.position += Vector3.back * (moveSpeed * Time.deltaTime);

            if (chunk.transform.position.z <= cameraZ - chunk.Length)
            {
                m_Chunks.RemoveAt(i);
                ReturnChunkToPool(chunk);
                needsRefill = true;
            }
        }

        // Spawneaza doar cand un chunk a fost scos, pana lantul depaseste far clip plane
        if (needsRefill)
        {
            float spawnThreshold = cameraZ + m_MainCamera.farClipPlane;
            while (m_Chunks.Count == 0 ||
                   m_Chunks[m_Chunks.Count - 1].transform.position.z + m_Chunks[m_Chunks.Count - 1].LocalMaxZ < spawnThreshold)
                SpawnChunk();
        }
    }

    // Calculeaza Z-ul pivot-ului noului chunk astfel incat mesh-ul sau sa inceapa exact
    // acolo unde se termina mesh-ul ultimului chunk existent
    private float CalculateSpawnPositionZ(Chunk newChunk)
    {
        if (m_Chunks.Count == 0)
            return transform.position.z - newChunk.LocalMinZ;

        Chunk last = m_Chunks[m_Chunks.Count - 1];
        float lastWorldEndZ = last.transform.position.z + last.LocalMaxZ;
        return lastWorldEndZ - newChunk.LocalMinZ;
    }

    // --- Buildings ---

    private void SpawnBuildingPair(float z)
    {
        if (buildingChunkConfig == null)
        {
            Debug.LogWarning("LevelGenerator: BuildingChunkConfig nu este asignat.");
            return;
        }
        if (buildingChunkConfig.buildingPrefabs == null || buildingChunkConfig.buildingPrefabs.Length == 0)
        {
            Debug.LogWarning("LevelGenerator: BuildingChunkConfig.buildingPrefabs este gol.");
            return;
        }

        int leftIndex = PickRandom(buildingChunkConfig.buildingPrefabs.Length, ref m_LastBuildingIndexLeft);
        int rightIndex = PickRandom(buildingChunkConfig.buildingPrefabs.Length, ref m_LastBuildingIndexRight);

        // Garantam ca stanga si dreapta sunt prefab-uri diferite
        if (buildingChunkConfig.buildingPrefabs.Length > 1 && rightIndex == leftIndex)
            rightIndex = (rightIndex + 1) % buildingChunkConfig.buildingPrefabs.Length;

        SpawnBuilding(buildingChunkConfig.leftX, buildingChunkConfig.leftYRotation, z, buildingChunkConfig.buildingPrefabs[leftIndex]);
        SpawnBuilding(buildingChunkConfig.rightX, buildingChunkConfig.rightYRotation, z, buildingChunkConfig.buildingPrefabs[rightIndex]);

        SpawnFillers(z);
    }

    private void SpawnFillers(float z)
    {
        if (buildingChunkConfig.fillerPrefabs == null || buildingChunkConfig.fillerPrefabs.Length == 0) return;

        for (int i = 0; i < buildingChunkConfig.fillerCountPerChunk; i++)
        {
            float fillerZ = z + Random.Range(0f, m_LastChunkLength);
            GameObject prefab = buildingChunkConfig.fillerPrefabs[Random.Range(0, buildingChunkConfig.fillerPrefabs.Length)];
            if (prefab == null) continue;

            float randomRotY = Random.Range(0f, 360f);

            GameObject left = GetBuildingFromPool(
                prefab,
                new Vector3(buildingChunkConfig.leftX, buildingChunkConfig.fillerY, fillerZ),
                Quaternion.Euler(0f, randomRotY, 0f));
            m_Buildings.Add(left);

            GameObject right = GetBuildingFromPool(
                prefab,
                new Vector3(buildingChunkConfig.rightX, buildingChunkConfig.fillerY, fillerZ),
                Quaternion.Euler(0f, randomRotY, 0f));
            m_Buildings.Add(right);
        }
    }

    private void SpawnBuilding(float x, float yRotation, float z, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("LevelGenerator: buildingPrefab este null.");
            return;
        }

        GameObject obj = GetBuildingFromPool(
            prefab,
            new Vector3(x, buildingChunkConfig.buildingY, z),
            Quaternion.Euler(0f, yRotation, 0f));

        m_Buildings.Add(obj);
    }

    private void MoveBuildings()
    {
        float cameraZ = m_MainCamera.transform.position.z;

        for (int i = m_Buildings.Count - 1; i >= 0; i--)
        {
            GameObject building = m_Buildings[i];
            if (building == null) { m_Buildings.RemoveAt(i); continue; }

            building.transform.position += Vector3.back * (moveSpeed * Time.deltaTime);

            if (building.transform.position.z <= cameraZ - m_LastChunkLength)
            {
                m_Buildings.RemoveAt(i);
                ReturnBuildingToPool(building);
            }
        }
    }

    // --- Object Pools ---

    private Chunk GetChunkFromPool(GameObject prefab)
    {
        if (!m_ChunkPool.TryGetValue(prefab, out Queue<Chunk> pool))
        {
            pool = new Queue<Chunk>();
            m_ChunkPool[prefab] = pool;
        }

        if (pool.Count > 0)
            return pool.Dequeue();

        // Prima instantiere: la origin ca Awake sa calculeze bounds corect
        GameObject chunkGO = Instantiate(prefab, Vector3.zero, Quaternion.identity, chunkParent);
        Chunk chunk = chunkGO.GetComponent<Chunk>() ?? chunkGO.AddComponent<Chunk>();
        chunk.SourcePrefab = prefab;
        chunkGO.SetActive(false); // dezactiveaza imediat dupa ce Awake a rulat
        return chunk;
    }

    private void ReturnChunkToPool(Chunk chunk)
    {
        chunk.Cleanup();
        chunk.gameObject.SetActive(false);

        if (!m_ChunkPool.TryGetValue(chunk.SourcePrefab, out Queue<Chunk> pool))
        {
            pool = new Queue<Chunk>();
            m_ChunkPool[chunk.SourcePrefab] = pool;
        }

        pool.Enqueue(chunk);
    }

    private GameObject GetBuildingFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!m_BuildingPool.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            m_BuildingPool[prefab] = pool;
        }

        if (pool.Count > 0)
        {
            GameObject pooled = pool.Dequeue();
            pooled.transform.SetPositionAndRotation(position, rotation);
            pooled.SetActive(true);
            return pooled;
        }

        GameObject obj = Instantiate(prefab, position, rotation, buildingParent);
        m_BuildingToPrefab[obj] = prefab;
        return obj;
    }

    private void ReturnBuildingToPool(GameObject building)
    {
        building.SetActive(false);

        if (m_BuildingToPrefab.TryGetValue(building, out GameObject prefab))
        {
            if (!m_BuildingPool.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                m_BuildingPool[prefab] = pool;
            }
            pool.Enqueue(building);
        }
        else
        {
            Destroy(building);
        }
    }

    // --- Helpers ---

    private int PickRandom(int count, ref int lastIndex)
    {
        if (count == 1) return 0;

        int picked;
        do { picked = Random.Range(0, count); }
        while (picked == lastIndex);

        lastIndex = picked;
        return picked;
    }
}
