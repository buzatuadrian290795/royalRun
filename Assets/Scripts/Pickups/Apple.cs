using UnityEngine;

public class Apple : Pickup
{
    [SerializeField] private float adjustChangeMoveSpeedAmount = 2f;

    LevelGenerator levelGenerator;

    private void Start()
    {
        levelGenerator = FindFirstObjectByType<LevelGenerator>();
    }
    protected override void OnPickup()
    {
        Debug.Log("Add Vitamins");
        levelGenerator.ChangeChunkMoveSpeed(adjustChangeMoveSpeedAmount);
    }
}
