using System;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private readonly LevelGenerator m_LevelGenerator;

    private int m_CurrentLane = 1;
    private bool m_IsChangingLane = false;

    private Vector2 m_SwipeStartPosition;
    private bool m_IsSwipeTracking;

    private const float SwipeThreshold = 35f;

    public PlayerController(PlayerView playerView, RoadView roadView, LevelGenerator levelGenerator)
    {
        m_PlayerView = playerView;
        m_RoadView = roadView;
        m_LevelGenerator = levelGenerator;

        Init();
    }

    private void Init()
    {
        Vector3 startPos = m_PlayerView.transform.position;
        startPos.x = m_RoadView.LanePositions[m_CurrentLane];
        m_PlayerView.transform.position = startPos;

        EnhancedTouchSupport.Enable();

#if UNITY_EDITOR
        TouchSimulation.Enable();
#endif
    }

    public async void Tick()
    {
        if (m_LevelGenerator != null)
        {
            m_PlayerView.Animator.SetFloat("MoveSpeed", m_LevelGenerator.MoveSpeed);
        }

        Movement movement = ReadInput();
        if (movement == Movement.None)
        {
            return;
        }

        await HandleMovementAsync(movement);
    }

    public void Dispose()
    {
#if UNITY_EDITOR
        TouchSimulation.Disable();
#endif
        EnhancedTouchSupport.Disable();
    }

    private Movement ReadInput()
    {
        if (m_IsChangingLane)
        {
            return Movement.None;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        Movement mouseMovement = ReadMouseSwipe();
        if (mouseMovement != Movement.None)
        {
            return mouseMovement;
        }
#endif

        return ReadTouchSwipe();
    }

    private Movement ReadTouchSwipe()
    {
        if (Touch.activeTouches.Count == 0)
        {
            return Movement.None;
        }

        var touch = Touch.activeTouches[0];

        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            m_SwipeStartPosition = touch.screenPosition;
            m_IsSwipeTracking = true;
            return Movement.None;
        }

        if (!m_IsSwipeTracking)
        {
            return Movement.None;
        }

        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
            touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            Vector2 swipeDelta = touch.screenPosition - m_SwipeStartPosition;

            if (Mathf.Abs(swipeDelta.x) < SwipeThreshold)
            {
                return Movement.None;
            }

            if (Mathf.Abs(swipeDelta.x) < Mathf.Abs(swipeDelta.y))
            {
                return Movement.None;
            }

            m_IsSwipeTracking = false;

            if (swipeDelta.x < 0f && m_CurrentLane > 0)
            {
                return Movement.Left;
            }

            if (swipeDelta.x > 0f && m_CurrentLane < m_RoadView.LanePositions.Length - 1)
            {
                return Movement.Right;
            }
        }

        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            m_IsSwipeTracking = false;
        }

        return Movement.None;
    }

    private Movement ReadMouseSwipe()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return Movement.None;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            m_SwipeStartPosition = mouse.position.ReadValue();
            m_IsSwipeTracking = true;
            return Movement.None;
        }

        if (!m_IsSwipeTracking)
        {
            return Movement.None;
        }

        Vector2 currentPosition = mouse.position.ReadValue();
        Vector2 swipeDelta = currentPosition - m_SwipeStartPosition;

        if (mouse.leftButton.isPressed)
        {
            if (Mathf.Abs(swipeDelta.x) >= SwipeThreshold &&
                Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                m_IsSwipeTracking = false;

                if (swipeDelta.x < 0f && m_CurrentLane > 0)
                {
                    return Movement.Left;
                }

                if (swipeDelta.x > 0f && m_CurrentLane < m_RoadView.LanePositions.Length - 1)
                {
                    return Movement.Right;
                }
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            m_IsSwipeTracking = false;
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

        if (movement == Movement.Left)
        {
            m_PlayerView.Animator.ResetTrigger("SwipeRight");
            m_PlayerView.Animator.SetTrigger("SwipeLeft");
        }
        else if (movement == Movement.Right)
        {
            m_PlayerView.Animator.ResetTrigger("SwipeLeft");
            m_PlayerView.Animator.SetTrigger("SwipeRight");
        }

        await MovePlayerAsync();

        m_IsChangingLane = false;
    }

    private async Awaitable MovePlayerAsync()
    {
        float time = 0f;
        float startX = m_PlayerView.RigidBody.position.x;
        float destinationX = m_RoadView.LanePositions[m_CurrentLane];
        float duration = m_RoadView.LaneChangeDuration;

        while (time < duration && !m_PlayerView.destroyCancellationToken.IsCancellationRequested)
        {
            float newX = Mathf.Lerp(startX, destinationX, time / duration);

            m_PlayerView.RigidBody.MovePosition(new Vector3(
                newX,
                m_PlayerView.RigidBody.position.y,
                m_PlayerView.RigidBody.position.z
            ));

            await Awaitable.NextFrameAsync();
            time += Time.deltaTime;
        }

        m_PlayerView.RigidBody.MovePosition(new Vector3(
            destinationX,
            m_PlayerView.RigidBody.position.y,
            m_PlayerView.RigidBody.position.z
        ));
    }
}