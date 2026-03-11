using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class PlayerController : IDisposable
{
    private enum Movement
    {
        None = 0,
        Left,
        Right
    }

    private readonly PlayerView m_PlayerView;
    private readonly RoadView m_RoadView;

    private float m_HalfScreenWidth;
    private int m_CurrentLane = 1;
    private bool m_IsChangingLane = false;

    public PlayerController(PlayerView playerView, RoadView roadView)
    {
        m_PlayerView = playerView;
        m_RoadView = roadView;

        Init();
    }

    private void Init()
    {
        m_HalfScreenWidth = Screen.width * 0.5f;
        m_PlayerView.transform.position = Vector3.zero;

        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
    }

    public void Tick()
    {
        var movement = ReadInput();
        if (movement == Movement.None)
        {
            return;
        }
    
        HandleMovementAsync(movement);
    }

    public void Dispose()
    {
        TouchSimulation.Disable();
        EnhancedTouchSupport.Disable();
    }

    private Movement ReadInput()
    {
        if (m_IsChangingLane || Touch.activeTouches.Count == 0)
        {
            return Movement.None;
        }

        var touch = Touch.activeTouches[0];
        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
        {
            return Movement.None;
        }

        if (touch.screenPosition.x <= m_HalfScreenWidth && m_CurrentLane != 0)
        {
            return Movement.Left;
        }

        if (touch.screenPosition.x >= m_HalfScreenWidth && m_CurrentLane != 2)
        {
            return Movement.Right;
        }

        return Movement.None;
    }

    private async Awaitable HandleMovementAsync(Movement movement)
    {
        if (movement == Movement.None)
        {
            return;
        }

        m_IsChangingLane = true;
        m_CurrentLane = movement switch
        {
            Movement.Left => m_CurrentLane - 1,
            Movement.Right => m_CurrentLane + 1,
            _ => m_CurrentLane
        };

        m_PlayerView.Animator.SetBool("IsJumping", true);

        await MovePlayerAsync();

        m_PlayerView.Animator.SetBool("IsJumping", false);
        m_IsChangingLane = false;
    }

    private async Awaitable MovePlayerAsync()
    {
        var time = 0f;
        var startPosition = m_PlayerView.RigidBody.position.x;
        var destination = m_RoadView.LanePositions[m_CurrentLane];
        var duration = m_RoadView.LaneChangeDuration;

        while (time < duration && !m_PlayerView.destroyCancellationToken.IsCancellationRequested)
        {
            var position = Mathf.Lerp(startPosition, destination, time / duration);
            m_PlayerView.RigidBody.MovePosition(new Vector3
            {
                x = position,
                y = m_PlayerView.RigidBody.position.y,
                z = m_PlayerView.RigidBody.position.z
            });

            await Awaitable.NextFrameAsync();
            time += Time.deltaTime;
        }
    }
}
