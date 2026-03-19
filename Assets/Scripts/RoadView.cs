using UnityEngine;

// Container de date pentru drum: expune durata schimbului de banda si pozitiile benzilor
public class RoadView : MonoBehaviour
{
    [field: SerializeField] public float LaneChangeDuration { get; private set; } = 0.25f;
    public float[] LanePositions { get; private set; } = { -2.5f, 0f, 2.5f };
}