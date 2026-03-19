using System.Collections.Generic;
using UnityEngine;

// Genereaza, muta si distruge chunk-urile de drum; controleaza viteza si dificultatea
public class LevelGenerator : MonoBehaviour
{
    [SerializeField] CameraController cameraController;
    [SerializeField] RoadView roadView;
    [SerializeField] List<GameObject> chunkPrefabs = new List<GameObject>();
    [SerializeField] int startingChunksAmount = 12;
    [SerializeField] Transform chunkParent;
    [SerializeField] float chunkLength = 10f;
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float minMoveSpeed = 8f;
    [SerializeField] float maxMoveSpeed = 44f;
    [SerializeField, Range(0f, 1f)] float obstacleDensity = 0.25f;

    private List<Chunk> m_Chunks = new List<Chunk>();
    private Camera m_MainCamera;
    private GameObject m_LastSpawnedPrefab;

    public float MoveSpeed => moveSpeed;
    public float ObstacleDensity => obstacleDensity;
    public int StartingChunksAmount => startingChunksAmount;

    public void SetObstacleDensity(float value)
    {
        obstacleDensity = Mathf.Clamp01(value);
    }

    public void SetStartingChunksAmount(int value)
    {
        startingChunksAmount = Mathf.Max(1, value);
    }

    public void RespawnChunks()
    {
        for (int i = m_Chunks.Count - 1; i >= 0; i--)
        {
            m_Chunks[i].Cleanup();
            Destroy(m_Chunks[i].gameObject);
        }
        m_Chunks.Clear();
        m_LastSpawnedPrefab = null;
        SpawnStartingChunks();
    }

    // Multiplicatorul de monede creste treptat cu viteza + bonus per obstacol lovit in Rush
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

    void Start()
    {
        SpawnStartingChunks();
    }

    private void FixedUpdate()
    {
        MoveChunks();
    }

    // Mareste viteza (apelat de Apple) si notifica camera sa ajusteze FOV
    public void ChangeChunkMoveSpeed(float speedAmount)
    {
        moveSpeed = Mathf.Min(moveSpeed + speedAmount, maxMoveSpeed);
        cameraController?.ChangeCameraFOV(speedAmount);
    }

    public float MaxMoveSpeed => maxMoveSpeed;
    public RoadView RoadView => roadView;
    public float[] LanePositions => roadView != null ? roadView.LanePositions : new float[] { -2.5f, 0f, 2.5f };

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

    // Reseteaza viteza la minim (apelat la moartea jucatorului)
    public void ResetMoveSpeed()
    {
        moveSpeed = minMoveSpeed;
        cameraController?.ResetFOV();
    }

    private void SpawnStartingChunks()
    {
        for (int i = 0; i < startingChunksAmount; i++)
            SpawnChunk();
    }

    private void SpawnChunk()
    {
        if (chunkPrefabs == null || chunkPrefabs.Count == 0)
        {
            Debug.LogError("LevelGenerator: chunkPrefabs list is empty.");
            return;
        }

        Vector3 spawnPos = new Vector3(
            transform.position.x,
            transform.position.y,
            CalculateSpawnPositionZ()
        );

        GameObject prefab = PickRandomPrefab();
        m_LastSpawnedPrefab = prefab;

        Chunk chunkScript = Instantiate(prefab, spawnPos, Quaternion.identity, chunkParent)
                                .GetComponent<Chunk>();

        chunkScript.SetObstacleDensity(obstacleDensity);
        chunkScript.Init(this);
        chunkScript.Initialize();

        m_Chunks.Add(chunkScript);
    }

    // Alege un prefab random diferit de cel anterior (daca exista mai mult de unul)
    private GameObject PickRandomPrefab()
    {
        if (chunkPrefabs.Count == 1) return chunkPrefabs[0];

        GameObject picked;
        do
        {
            picked = chunkPrefabs[Random.Range(0, chunkPrefabs.Count)];
        }
        while (picked == m_LastSpawnedPrefab);

        return picked;
    }

    // Spawn-ul urmatorului chunk incepe imediat dupa ultimul din lista
    private float CalculateSpawnPositionZ()
    {
        if (m_Chunks.Count == 0) return transform.position.z;
        return m_Chunks[m_Chunks.Count - 1].transform.position.z + chunkLength;
    }

    private void MoveChunks()
    {
        float cameraZ = m_MainCamera.transform.position.z;

        for (int i = m_Chunks.Count - 1; i >= 0; i--)
        {
            Chunk chunk = m_Chunks[i];
            chunk.transform.position += Vector3.back * (moveSpeed * Time.deltaTime);

            // Chunk-ul a trecut de camera -> curata-l, distruge-l si spawneaza unul nou
            if (chunk.transform.position.z <= cameraZ - chunkLength)
            {
                m_Chunks.RemoveAt(i);
                chunk.Cleanup();
                Destroy(chunk.gameObject);
                SpawnChunk();
            }
        }
    }
}
