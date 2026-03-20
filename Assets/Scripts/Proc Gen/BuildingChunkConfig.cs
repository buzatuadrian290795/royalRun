using UnityEngine;

[CreateAssetMenu(fileName = "BuildingChunkConfig", menuName = "Royal Run/Building Chunk Config")]
public class BuildingChunkConfig : ScriptableObject
{
    [Header("Building Prefabs")]
    public GameObject[] buildingPrefabs;

    [Header("Positions")]
    public float leftX = -9f;
    public float rightX = 9f;
    public float buildingY = 0f;
    public float leftYRotation = 90f;
    public float rightYRotation = -90f;

    [Header("Filler (iarba / vegetatie)")]
    public GameObject[] fillerPrefabs;
    public int fillerCountPerChunk = 4;
    public float fillerY = 0f;
}
