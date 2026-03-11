using UnityEngine;

public class RoadView : MonoBehaviour
{
    [field: SerializeField] 
    public float LaneChangeDuration { get; private set; } = 0.25f;
    
    [field: SerializeField] 
    public float[] LanePositions { get; private set; } = { -3f, 0f, 3f };
}