using UnityEngine;

public class Apple : Pickup
{
    [SerializeField] private float adjustChangeMoveSpeedAmount = 2f;

    protected override void OnPickup()
    {
        AudioManager.Instance.PlayApple();
        m_LevelGenerator?.ChangeChunkMoveSpeed(adjustChangeMoveSpeedAmount);
    }
}