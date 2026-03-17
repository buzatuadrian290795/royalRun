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
        Right,
        Jump,
        Roll
    }

    private readonly PlayerView m_PlayerView;
    private readonly RoadView m_RoadView;
    private readonly LevelGenerator m_LevelGenerator;
    private readonly CapsuleCollider m_Capsule;
    private readonly Rigidbody m_Rigidbody;
    private readonly Animator m_Animator;

    private static readonly int s_MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int s_SwipeLeftHash = Animator.StringToHash("SwipeLeft");
    private static readonly int s_SwipeRightHash = Animator.StringToHash("SwipeRight");
    private static readonly int s_JumpHash = Animator.StringToHash("Jump");
    private static readonly int s_RollHash = Animator.StringToHash("Roll");

    private readonly float m_GroundY;
    private readonly float m_OriginalCapsuleHeight;
    private readonly Vector3 m_OriginalCapsuleCenter;

    private readonly float m_RollCapsuleHeight;
    private readonly Vector3 m_RollCapsuleCenter;

    private const float JumpHeight = 2.5f;
    private const float JumpDuration = 0.6f;
    private const float RollDepth = 0.9f;
    private const float RollDuration = 0.8f;
    private const float RollDownTime = 0.15f;
    private const float RollUpTime = 0.2f;
    private const float RollHoldTime = RollDuration - RollDownTime - RollUpTime;
    private const float SwipeThreshold = 35f;

    private int m_CurrentLane;
    private bool m_IsChangingLane;
    private bool m_IsSwipeTracking;
    private Vector2 m_SwipeStartPosition;

    public PlayerController(PlayerView playerView, RoadView roadView, LevelGenerator levelGenerator)
    {
        m_PlayerView = playerView;
        m_RoadView = roadView;
        m_LevelGenerator = levelGenerator;

        m_Rigidbody = m_PlayerView.RigidBody;
        m_Animator = m_PlayerView.Animator;
        m_Capsule = m_PlayerView.GetComponent<CapsuleCollider>();

        if (m_Capsule != null)
        {
            m_OriginalCapsuleHeight = m_Capsule.height;
            m_OriginalCapsuleCenter = m_Capsule.center;

            m_RollCapsuleHeight = m_OriginalCapsuleHeight * 0.5f;
            m_RollCapsuleCenter = new Vector3(
                m_OriginalCapsuleCenter.x,
                m_OriginalCapsuleCenter.y * 0.5f,
                m_OriginalCapsuleCenter.z
            );
        }

        m_CurrentLane = 1;
        m_GroundY = m_Rigidbody.position.y;

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
            m_Animator.SetFloat(s_MoveSpeedHash, m_LevelGenerator.MoveSpeed);

        Movement movement = ReadInput();
        if (movement == Movement.None)
            return;

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
            return Movement.None;

        return ReadInputDirect();
    }

    private Movement ReadInputDirect()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        Movement mouseMovement = ReadMouseSwipe();
        if (mouseMovement != Movement.None)
            return mouseMovement;
#endif
        return ReadTouchSwipe();
    }

    private Movement ReadTouchSwipe()
    {
        if (Touch.activeTouches.Count == 0)
            return Movement.None;

        var touch = Touch.activeTouches[0];
        var phase = touch.phase;

        if (phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            m_SwipeStartPosition = touch.screenPosition;
            m_IsSwipeTracking = true;
            return Movement.None;
        }

        if (!m_IsSwipeTracking)
            return Movement.None;

        if (phase == UnityEngine.InputSystem.TouchPhase.Moved ||
            phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            Movement result = EvaluateSwipe(touch.screenPosition - m_SwipeStartPosition);
            if (result != Movement.None)
            {
                m_IsSwipeTracking = false;
                return result;
            }
        }

        if (phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            m_IsSwipeTracking = false;

        return Movement.None;
    }

    private Movement ReadMouseSwipe()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return Movement.None;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            m_SwipeStartPosition = mouse.position.ReadValue();
            m_IsSwipeTracking = true;
            return Movement.None;
        }

        if (!m_IsSwipeTracking)
            return Movement.None;

        if (mouse.leftButton.isPressed)
        {
            Movement result = EvaluateSwipe(mouse.position.ReadValue() - m_SwipeStartPosition);
            if (result != Movement.None)
            {
                m_IsSwipeTracking = false;
                return result;
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame)
            m_IsSwipeTracking = false;

        return Movement.None;
    }

    private Movement EvaluateSwipe(Vector2 delta)
    {
        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);

        if (absY >= SwipeThreshold && absY > absX)
            return delta.y > 0f ? Movement.Jump : Movement.Roll;

        if (absX >= SwipeThreshold && absX > absY)
        {
            if (delta.x < 0f && m_CurrentLane > 0)
                return Movement.Left;
            if (delta.x > 0f && m_CurrentLane < m_RoadView.LanePositions.Length - 1)
                return Movement.Right;
        }

        return Movement.None;
    }

    private async Awaitable HandleMovementAsync(Movement movement)
    {
        m_IsChangingLane = true;

        switch (movement)
        {
            case Movement.Left:
                m_CurrentLane = Mathf.Max(0, m_CurrentLane - 1);
                m_Animator.ResetTrigger(s_SwipeRightHash);
                m_Animator.SetTrigger(s_SwipeLeftHash);
                await MovePlayerAsync();
                break;

            case Movement.Right:
                m_CurrentLane = Mathf.Min(m_RoadView.LanePositions.Length - 1, m_CurrentLane + 1);
                m_Animator.ResetTrigger(s_SwipeLeftHash);
                m_Animator.SetTrigger(s_SwipeRightHash);
                await MovePlayerAsync();
                break;

            case Movement.Jump:
                m_Animator.SetTrigger(s_JumpHash);
                await JumpAsync();
                break;

            case Movement.Roll:
                m_Animator.SetTrigger(s_RollHash);
                await RollAsync();
                break;
        }

        m_IsChangingLane = false;
    }

    private async Awaitable MovePlayerAsync()
    {
        float time = 0f;
        float startX = m_Rigidbody.position.x;
        float destinationX = m_RoadView.LanePositions[m_CurrentLane];
        float duration = m_RoadView.LaneChangeDuration;
        float invDuration = 1f / duration; 

        while (time < duration && !m_PlayerView.destroyCancellationToken.IsCancellationRequested)
        {
            MoveX(Mathf.Lerp(startX, destinationX, time * invDuration));
            await Awaitable.NextFrameAsync();
            time += Time.deltaTime;
        }

        MoveX(destinationX);
    }

    private async Awaitable JumpAsync()
    {
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();

        float time = 0f;
        float invDuration = 1f / JumpDuration;

        while (time < JumpDuration && !m_PlayerView.destroyCancellationToken.IsCancellationRequested)
        {
            MoveY(m_GroundY + JumpHeight * Mathf.Sin(Mathf.PI * (time * invDuration)));
            await Awaitable.NextFrameAsync();
            time += Time.deltaTime;
        }

        MoveY(m_GroundY);
    }

    private async Awaitable RollAsync()
    {
        m_Rigidbody.useGravity = false;
        m_Rigidbody.isKinematic = true;

        if (m_Capsule != null)
        {
            m_Capsule.height = m_RollCapsuleHeight;
            m_Capsule.center = m_RollCapsuleCenter;
        }

        float targetY = m_GroundY - RollDepth;
        bool jumpQueued = false;

        await LerpYAsync(m_GroundY, targetY, RollDownTime);

        float elapsed = 0f;
        while (elapsed < RollHoldTime && !m_PlayerView.destroyCancellationToken.IsCancellationRequested)
        {
            if (ReadInputDirect() == Movement.Jump)
            {
                jumpQueued = true;
                break;
            }
            await Awaitable.NextFrameAsync();
            elapsed += Time.deltaTime;
        }

        await LerpYAsync(targetY, m_GroundY, RollUpTime);
        MoveY(m_GroundY);

        if (m_Capsule != null)
        {
            m_Capsule.height = m_OriginalCapsuleHeight;
            m_Capsule.center = m_OriginalCapsuleCenter;
        }

        m_Rigidbody.isKinematic = false;
        m_Rigidbody.useGravity = false;
        m_Rigidbody.linearVelocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;

        if (jumpQueued)
        {
            m_Rigidbody.position = new Vector3(
                m_Rigidbody.position.x,
                m_GroundY,
                m_Rigidbody.position.z
            );
            m_Animator.SetTrigger(s_JumpHash);
            await JumpAsync();
        }
    }

    private void MoveX(float x) =>
        m_Rigidbody.MovePosition(new Vector3(x, m_Rigidbody.position.y, m_Rigidbody.position.z));

    private void MoveY(float y) =>
        m_Rigidbody.MovePosition(new Vector3(m_Rigidbody.position.x, y, m_Rigidbody.position.z));

    private async Awaitable LerpYAsync(float from, float to, float duration)
    {
        float elapsed = 0f;
        float invDuration = 1f / duration;

        while (elapsed < duration && !m_PlayerView.destroyCancellationToken.IsCancellationRequested)
        {
            MoveY(Mathf.Lerp(from, to, elapsed * invDuration));
            await Awaitable.NextFrameAsync();
            elapsed += Time.deltaTime;
        }
    }
}