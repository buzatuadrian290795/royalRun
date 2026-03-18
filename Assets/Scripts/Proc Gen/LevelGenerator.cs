using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// Genereaza, muta si recicleaza chunk-urile de drum; controleaza viteza si dificultatea
public class LevelGenerator : MonoBehaviour
{
    [SerializeField] CameraController cameraController;
    [SerializeField] List<GameObject> chunkPrefabs = new List<GameObject>();
    [SerializeField] int startingChunksAmount = 12;
    [SerializeField] Transform chunkParent;
    [SerializeField] float chunkLength = 10f;
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float minMoveSpeed = 8f;
    [SerializeField] float maxMoveSpeed = 44f;
    [SerializeField, Range(0f, 1f)] float obstacleDensity = 0.5f;

    private List<Chunk> m_Chunks = new List<Chunk>();
    private Camera m_MainCamera;

    // Un pool per prefab; evita Instantiate/Destroy la fiecare reciclare de chunk
    private readonly Dictionary<GameObject, ObjectPool<Chunk>> m_Pools = new();
    // Mapeaza instanta -> prefab-ul din care provine (necesar pentru a returna la pool-ul corect)
    private readonly Dictionary<Chunk, GameObject> m_InstanceToPrefab = new();

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

    // Multiplicatorul de monede creste treptat cu viteza (folosit de CoinMultiplierUI)
    public int CoinMultiplier
    {
        get
        {
            if (moveSpeed >= 44f) return 10;
            if (moveSpeed >= 40f) return 9;
            if (moveSpeed >= 36f) return 8;
            if (moveSpeed >= 32f) return 7;
            if (moveSpeed >= 28f) return 6;
            if (moveSpeed >= 24f) return 5;
            if (moveSpeed >= 20f) return 4;
            if (moveSpeed >= 16f) return 3;
            if (moveSpeed >= 12f) return 2;
            return 1;
        }
    }

    private void Awake()
    {
        m_MainCamera = Camera.main;

        foreach (var prefab in chunkPrefabs)
        {
            if (prefab == null) continue;
            if (prefab.GetComponent<Chunk>() == null)
            {
                Debug.LogError($"LevelGenerator: prefab '{prefab.name}' nu are componenta Chunk, ignorat.");
                continue;
            }

            var captured = prefab;
            m_Pools[prefab] = new ObjectPool<Chunk>(
                createFunc: () => Instantiate(captured, chunkParent).GetComponent<Chunk>(),
                actionOnGet: c => c.gameObject.SetActive(true),
                actionOnRelease: c => c.gameObject.SetActive(false),
                actionOnDestroy: c => Destroy(c.gameObject),
                defaultCapacity: startingChunksAmount
            );
        }
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

        GameObject prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Count)];

        if (!m_Pools.TryGetValue(prefab, out var pool))
        {
            Debug.LogError($"LevelGenerator: nu exista pool pentru prefab-ul '{prefab.name}'.");
            return;
        }

        Chunk chunkScript = pool.Get();
        chunkScript.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        m_InstanceToPrefab[chunkScript] = prefab;

        chunkScript.SetObstacleDensity(obstacleDensity);
        chunkScript.Init(this);
        chunkScript.Initialize();

        m_Chunks.Add(chunkScript);
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
            chunk.transform.Translate(-transform.forward * (moveSpeed * Time.deltaTime));

            // Chunk-ul a trecut de camera -> curata-l si returneaza-l la pool
            if (chunk.transform.position.z <= cameraZ - chunkLength)
            {
                m_Chunks.RemoveAt(i);

                chunk.Cleanup();

                if (m_InstanceToPrefab.TryGetValue(chunk, out GameObject prefab))
                {
                    m_Pools[prefab].Release(chunk);
                    m_InstanceToPrefab.Remove(chunk);
                }

                SpawnChunk();
            }
        }
    }
}
