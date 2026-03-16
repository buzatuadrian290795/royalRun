using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] int startingChunksAmount = 12;
    [SerializeField] Transform chunkParent;
    [SerializeField] float chunkLength = 10f;
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float minMoveSpeed = 8f;
    [SerializeField] float maxMoveSpeed = 16f;

    List<GameObject> chunks = new List<GameObject>();

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
    }

    public void ResetMoveSpeed()
    {
        moveSpeed = minMoveSpeed;
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
        float spawnPositionZ = CalculateSpawnPositionZ();

        Vector3 chunkSpawnPos = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);
        GameObject newChunk = Instantiate(chunkPrefab, chunkSpawnPos, Quaternion.identity, chunkParent);
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