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
    [SerializeField] int startingChunksAmount = 12;
    [SerializeField] float chunkLength = 16f;
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float minMoveSpeed = 8f;
    [SerializeField] float maxMoveSpeed = 44f;
    [SerializeField, Range(0f, 1f)] float obstacleDensity = 0.25f;

    private List<Chunk> m_Chunks = new List<Chunk>();
    private List<GameObject> m_Buildings = new List<GameObject>();
    private Camera m_MainCamera;
    private int m_LastChunkIndex = -1;
    private int m_LastBuildingIndex = -1;

    public float MoveSpeed => moveSpeed;
    public float ObstacleDensity => obstacleDensity;
    public float ChunkLength => chunkLength;
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
    }

    private void Start()
    {
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
        foreach (var chunk in m_Chunks) { chunk.Cleanup(); Destroy(chunk.gameObject); }
        m_Chunks.Clear();
        m_LastChunkIndex = -1;

        foreach (var building in m_Buildings) { if (building != null) Destroy(building); }
        m_Buildings.Clear();
        m_LastBuildingIndex = -1;

        SpawnStartingChunks();
    }

    // --- Chunks ---

    private void SpawnStartingChunks()
    {
        for (int i = 0; i < startingChunksAmount; i++)
            SpawnChunk();
    }

    private void SpawnChunk()
    {
        if (chunkConfig == null || chunkConfig.chunkPrefabs == null || chunkConfig.chunkPrefabs.Length == 0)
        {
            Debug.LogError("LevelGenerator: ChunkConfig nu are chunkPrefabs.");
            return;
        }

        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, CalculateSpawnPositionZ());

        int index = PickRandom(chunkConfig.chunkPrefabs.Length, ref m_LastChunkIndex);
        GameObject prefab = chunkConfig.chunkPrefabs[index];
        if (prefab == null) { Debug.LogError($"LevelGenerator: chunkPrefab null la index {index}."); return; }

        GameObject chunkGO = Instantiate(prefab, spawnPos, Quaternion.identity, chunkParent);
        Chunk chunk = chunkGO.GetComponent<Chunk>() ?? chunkGO.AddComponent<Chunk>();

        chunk.Initialize(this, chunkConfig, obstacleDensity);
        m_Chunks.Add(chunk);

        SpawnBuildingPair(spawnPos.z);
    }

    private void MoveChunks()
    {
        float cameraZ = m_MainCamera.transform.position.z;

        for (int i = m_Chunks.Count - 1; i >= 0; i--)
        {
            Chunk chunk = m_Chunks[i];
            chunk.transform.position += Vector3.back * (moveSpeed * Time.deltaTime);

            if (chunk.transform.position.z <= cameraZ - chunkLength)
            {
                m_Chunks.RemoveAt(i);
                chunk.Cleanup();
                Destroy(chunk.gameObject);
                SpawnChunk();
            }
        }
    }

    private float CalculateSpawnPositionZ()
    {
        if (m_Chunks.Count == 0) return transform.position.z;
        return m_Chunks[m_Chunks.Count - 1].transform.position.z + chunkLength;
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

        int index = PickRandom(buildingChunkConfig.buildingPrefabs.Length, ref m_LastBuildingIndex);
        GameObject prefab = buildingChunkConfig.buildingPrefabs[index];

        SpawnBuilding(buildingChunkConfig.leftX, buildingChunkConfig.leftYRotation, z, prefab);
        SpawnBuilding(buildingChunkConfig.rightX, buildingChunkConfig.rightYRotation, z, prefab);

        SpawnFillers(z);
    }

    private void SpawnFillers(float z)
    {
        if (buildingChunkConfig.fillerPrefabs == null || buildingChunkConfig.fillerPrefabs.Length == 0) return;

        for (int i = 0; i < buildingChunkConfig.fillerCountPerChunk; i++)
        {
            float fillerZ = z + Random.Range(0f, chunkLength);
            GameObject prefab = buildingChunkConfig.fillerPrefabs[Random.Range(0, buildingChunkConfig.fillerPrefabs.Length)];
            if (prefab == null) continue;

            float randomRotY = Random.Range(0f, 360f);

            GameObject left = Instantiate(prefab,
                new Vector3(buildingChunkConfig.leftX, buildingChunkConfig.fillerY, fillerZ),
                Quaternion.Euler(0f, randomRotY, 0f), buildingParent);
            m_Buildings.Add(left);

            GameObject right = Instantiate(prefab,
                new Vector3(buildingChunkConfig.rightX, buildingChunkConfig.fillerY, fillerZ),
                Quaternion.Euler(0f, randomRotY, 0f), buildingParent);
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

        GameObject obj = Instantiate(
            prefab,
            new Vector3(x, buildingChunkConfig.buildingY, z),
            Quaternion.Euler(0f, yRotation, 0f),
            buildingParent
        );

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

            if (building.transform.position.z <= cameraZ - chunkLength)
            {
                m_Buildings.RemoveAt(i);
                Destroy(building);
            }
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
