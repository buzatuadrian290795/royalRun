using System.Collections.Generic;
using UnityEngine;

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

    List<GameObject> chunks = new List<GameObject>();

    public float MoveSpeed => moveSpeed;

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

    void Start()
    {
        SpawnStartingChunks();
    }

    private void FixedUpdate()
    {
        MoveChunks();
    }

    public void ChangeChunkMoveSpeed(float speedAmount)
    {
        moveSpeed += speedAmount;

        if (moveSpeed > maxMoveSpeed)
        {
            moveSpeed = maxMoveSpeed;
        }

        if (cameraController != null)
        {
            cameraController.ChangeCameraFOV(speedAmount);
        }
    }

    public void ResetMoveSpeed()
    {
        moveSpeed = minMoveSpeed;

        if (cameraController != null)
        {
            cameraController.ResetFOV();
        }

        Debug.Log("ResetMoveSpeed called. moveSpeed = " + moveSpeed);
    }

    private void SpawnStartingChunks()
    {
        for (int i = 0; i < startingChunksAmount; i++)
        {
            SpawnChunk();
        }
    }

    private void SpawnChunk()
    {
        if (chunkPrefabs == null || chunkPrefabs.Count == 0)
        {
            Debug.LogError("LevelGenerator: chunkPrefabs list is empty.");
            return;
        }

        float spawnPositionZ = CalculateSpawnPositionZ();
        Vector3 chunkSpawnPos = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);

        GameObject selectedChunkPrefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Count)];
        GameObject newChunk = Instantiate(selectedChunkPrefab, chunkSpawnPos, Quaternion.identity, chunkParent);

        Chunk chunkScript = newChunk.GetComponent<Chunk>();
        if (chunkScript != null)
        {
            chunkScript.SetObstacleDensity(obstacleDensity);
            chunkScript.Init(this);
        }

        chunks.Add(newChunk);
    }

    private float CalculateSpawnPositionZ()
    {
        if (chunks.Count == 0)
        {
            return transform.position.z;
        }

        return chunks[chunks.Count - 1].transform.position.z + chunkLength;
    }

    private void MoveChunks()
    {
        for (int i = chunks.Count - 1; i >= 0; i--)
        {
            GameObject chunk = chunks[i];
            chunk.transform.Translate(-transform.forward * (moveSpeed * Time.deltaTime));

            if (chunk.transform.position.z <= Camera.main.transform.position.z - chunkLength)
            {
                chunks.RemoveAt(i);
                Destroy(chunk);
                SpawnChunk();
            }
        }
    }
}