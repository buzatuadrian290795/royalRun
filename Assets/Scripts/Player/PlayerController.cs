using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

// Gestioneaza inputul si miscarea jucatorului (schimb de banda, saritura, rostogolire)
// Nu e MonoBehaviour - e creat si distrus manual de PlayerRespawnManager
public class PlayerController : IDisposable
{
    // Tipurile de miscare posibile
    private enum Movement { None = 0, Left, Right, Jump, Roll }

    // Starea unui schimb de banda in desfasurare (folosita si in aer)
    private sealed class LaneChangeState
    {
        public float StartX;
        public float TargetX;
        public float Elapsed;
        public bool Active;
    }

    // Referinte la componentele necesare
    private readonly PlayerView m_PlayerView;
    private readonly RoadView m_RoadView;
    private readonly LevelGenerator m_LevelGenerator;
    private readonly CapsuleCollider m_Capsule;
    private readonly Rigidbody m_Rigidbody;
    private readonly Animator m_Animator;

    // Hash-uri pre-calculate pentru parametrii Animator (mai rapid decat string-uri)
    private static readonly int s_MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int s_SwipeLeftHash = Animator.StringToHash("SwipeLeft");
    private static readonly int s_SwipeRightHash = Animator.StringToHash("SwipeRight");
    private static readonly int s_JumpHash = Animator.StringToHash("Jump");
    private static readonly int s_RollHash = Animator.StringToHash("Roll");

    // Layer-uri pre-calculate pentru IgnoreLayerCollision (mai rapid decat NameToLayer la fiecare roll)
    private readonly int m_PlayerLayer;
    private readonly int m_GroundLayer;

    // Pozitia Y a solului (capturata dupa spawn)
    private float GroundY = 0f;

    // Pozitia Y a jucatorului in timpul rostogolirii
    private const float RollY = -0.75f;

    // Dimensiunile originale si cele reduse ale capsulei pentru rostogolire
    private readonly float m_OriginalCapsuleHeight;
    private readonly Vector3 m_OriginalCapsuleCenter;
    private readonly float m_RollCapsuleHeight;
    private readonly Vector3 m_RollCapsuleCenter;

    // Constante pentru saritura
    private const float JumpHeight = 2.5f;
    private const float JumpDuration = 0.6f;

    // Constante pentru rostogolire (impartita in 3 faze: coborare, mentinere, ridicare)
    private const float RollDuration = 0.8f;
    private const float RollDownTime = 0.15f;
    private const float RollUpTime = 0.2f;
    private const float RollHoldTime = RollDuration - RollDownTime - RollUpTime;

    // Pragul minim de swipe in pixeli (citit din setari, default 35)
    private float SwipeThreshold => SettingsManager.Instance != null
        ? SettingsManager.Instance.SwipeSensitivity : 35f;

    // Starea curenta a miscarii
    private int m_CurrentLane;
    private bool m_IsChangingLane;
    private bool m_IsJumping;
    private bool m_IsRolling;
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

            // Capsula de roll: aceeasi inaltime dar centru coborat la 80%
            m_RollCapsuleHeight = m_OriginalCapsuleHeight;
            m_RollCapsuleCenter = new Vector3(
                m_OriginalCapsuleCenter.x,
                m_OriginalCapsuleCenter.y * 0.8f,
                m_OriginalCapsuleCenter.z
            );
        }

        m_PlayerLayer = LayerMask.NameToLayer("Player");
        m_GroundLayer = LayerMask.NameToLayer("Default");

        m_CurrentLane = 1; // Banda din mijloc
        GroundY = m_Rigidbody.position.y;

        Init();
    }

    // Pozitioneaza jucatorul pe banda initiala si captureaza GroundY dupa 2 frame-uri
    // (async void intentionat - fire-and-forget la initializare)
    private async void Init()
    {
        Vector3 startPos = m_PlayerView.transform.position;
        startPos.x = m_RoadView.LanePositions[m_CurrentLane];
        m_PlayerView.transform.position = startPos;

        EnhancedTouchSupport.Enable();
#if UNITY_EDITOR
        TouchSimulation.Enable(); // Simuleaza touch cu mouse-ul in Editor
#endif
        // Asteapta 2 frame-uri ca physics sa se stabilizeze
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        GroundY = m_Rigidbody.position.y;
    }

    // Apelat la fiecare frame de PlayerView; citeste input si declanseaza miscarea
    public async void Tick()
    {
        if (m_LevelGenerator != null)
            m_Animator.SetFloat(s_MoveSpeedHash, m_LevelGenerator.MoveSpeed);

        Movement movement = ReadInput();
        if (movement == Movement.None) return;

        await HandleMovementAsync(movement);
    }

    // Dezactiveaza touch support la distrugerea controllerului
    public void Dispose()
    {
#if UNITY_EDITOR
        TouchSimulation.Disable();
#endif
        EnhancedTouchSupport.Disable();
    }

    // ---------- INPUT ----------

    // Blocheaza inputul daca o miscare e deja in desfasurare
    private Movement ReadInput()
    {
        if (m_IsJumping || m_IsRolling || m_IsChangingLane)
            return Movement.None;
        return ReadInputDirect();
    }

    // Citeste input din touch sau mouse (mouse doar in Editor/Standalone)
    private Movement ReadInputDirect()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        Movement mouseMovement = ReadMouseSwipe();
        if (mouseMovement != Movement.None) return mouseMovement;
#endif
        return ReadTouchSwipe();
    }

    // Detecteaza swipe-uri pe ecranul tactil
    private Movement ReadTouchSwipe()
    {
        if (Touch.activeTouches.Count == 0) return Movement.None;

        var touch = Touch.activeTouches[0];
        var phase = touch.phase;

        if (phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            m_SwipeStartPosition = touch.screenPosition;
            m_IsSwipeTracking = true;
            return Movement.None;
        }

        if (!m_IsSwipeTracking) return Movement.None;

        if (phase == UnityEngine.InputSystem.TouchPhase.Moved ||
            phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            Movement result = EvaluateSwipe(touch.screenPosition - m_SwipeStartPosition);
            if (result != Movement.None) { m_IsSwipeTracking = false; return result; }
        }

        if (phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            m_IsSwipeTracking = false;

        return Movement.None;
    }

    // Detecteaza swipe-uri cu mouse-ul (Editor/PC)
    private Movement ReadMouseSwipe()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return Movement.None;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            m_SwipeStartPosition = mouse.position.ReadValue();
            m_IsSwipeTracking = true;
            return Movement.None;
        }

        if (!m_IsSwipeTracking) return Movement.None;

        if (mouse.leftButton.isPressed)
        {
            Movement result = EvaluateSwipe(mouse.position.ReadValue() - m_SwipeStartPosition);
            if (result != Movement.None) { m_IsSwipeTracking = false; return result; }
        }

        if (mouse.leftButton.wasReleasedThisFrame)
            m_IsSwipeTracking = false;

        return Movement.None;
    }

    // Interpreteaza directia swipe-ului in functie de axa dominanta si prag minim
    private Movement EvaluateSwipe(Vector2 delta)
    {
        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);

        // Axa Y dominanta → saritura (sus) sau rostogolire (jos)
        if (absY >= SwipeThreshold && absY > absX)
            return delta.y > 0f ? Movement.Jump : Movement.Roll;

        // Axa X dominanta → stanga/dreapta (doar daca banda e disponibila)
        if (absX >= SwipeThreshold && absX > absY)
        {
            if (delta.x < 0f && m_CurrentLane > 0) return Movement.Left;
            if (delta.x > 0f && m_CurrentLane < m_RoadView.LanePositions.Length - 1) return Movement.Right;
        }

        return Movement.None;
    }

    // ---------- MISCARE ----------

    // Executa miscarea corespunzatoare: animatie, sunet, deplasare
    private async Awaitable HandleMovementAsync(Movement movement)
    {
        m_IsChangingLane = true;

        switch (movement)
        {
            case Movement.Left:
                m_CurrentLane = Mathf.Max(0, m_CurrentLane - 1);
                m_Animator.ResetTrigger(s_SwipeRightHash);
                m_Animator.SetTrigger(s_SwipeLeftHash);
                AudioManager.Instance.PlaySwipe();
                await MovePlayerAsync();
                break;

            case Movement.Right:
                m_CurrentLane = Mathf.Min(m_RoadView.LanePositions.Length - 1, m_CurrentLane + 1);
                m_Animator.ResetTrigger(s_SwipeLeftHash);
                m_Animator.SetTrigger(s_SwipeRightHash);
                AudioManager.Instance.PlaySwipe();
                await MovePlayerAsync();
                break;

            case Movement.Jump:
                m_Animator.SetTrigger(s_JumpHash);
                AudioManager.Instance.PlayJump();
                await JumpAsync();
                break;

            case Movement.Roll:
                m_Animator.SetTrigger(s_RollHash);
                AudioManager.Instance.PlayRoll();
                await RollAsync();
                break;
        }

        m_IsChangingLane = false;
    }

    // Deplaseaza smooth jucatorul pe axa X catre banda tinta
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

    // Incearca sa initieze un schimb de banda in aer; returneaza true daca a reusit
    private bool TryStartAirLaneChange(Movement input, LaneChangeState state)
    {
        if (input != Movement.Left && input != Movement.Right) return false;

        int newLane = input == Movement.Left
            ? Mathf.Max(0, m_CurrentLane - 1)
            : Mathf.Min(m_RoadView.LanePositions.Length - 1, m_CurrentLane + 1);

        if (newLane == m_CurrentLane) return false;

        m_CurrentLane = newLane;

        if (input == Movement.Left) { m_Animator.ResetTrigger(s_SwipeRightHash); m_Animator.SetTrigger(s_SwipeLeftHash); }
        else { m_Animator.ResetTrigger(s_SwipeLeftHash); m_Animator.SetTrigger(s_SwipeRightHash); }

        state.StartX = m_Rigidbody.position.x;
        state.TargetX = m_RoadView.LanePositions[m_CurrentLane];
        state.Elapsed = 0f;
        state.Active = true;
        return true;
    }

    // Avanseaza interpolarea X a unui schimb de banda in desfasurare; returneaza pozitia X curenta
    private float TickLaneX(LaneChangeState state, float laneDuration)
    {
        if (!state.Active) return m_Rigidbody.position.x;

        state.Elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(state.Elapsed / laneDuration);
        float x = Mathf.Lerp(state.StartX, state.TargetX, t);
        if (t >= 1f) state.Active = false;
        return x;
    }

    // Executa saritura cu arc sinusoidal pe Y; permite schimb de banda in aer
    private async Awaitable JumpAsync()
    {
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();

        m_IsJumping = true;
        float time = 0f;
        float invDuration = 1f / JumpDuration;
        float laneDuration = m_RoadView.LaneChangeDuration;

        var lane = new LaneChangeState
        {
            StartX = m_Rigidbody.position.x,
            TargetX = m_RoadView.LanePositions[m_CurrentLane],
            Active = false
        };

        while (time < JumpDuration && !m_PlayerView.destroyCancellationToken.IsCancellationRequested)
        {
            if (TryStartAirLaneChange(ReadInputDirect(), lane))
                AudioManager.Instance.PlaySwipe();

            float currentX = TickLaneX(lane, laneDuration);
            // Arc sinusoidal: 0 → maxim la mijloc → 0
            float currentY = GroundY + JumpHeight * Mathf.Sin(Mathf.PI * (time * invDuration));
            m_Rigidbody.MovePosition(new Vector3(currentX, currentY, m_Rigidbody.position.z));

            await Awaitable.NextFrameAsync();
            time += Time.deltaTime;
        }

        m_Rigidbody.MovePosition(new Vector3(m_RoadView.LanePositions[m_CurrentLane], GroundY, m_Rigidbody.position.z));
        m_IsJumping = false;
    }

    // Executa rostogolirea: coborare Y → mentinere → ridicare Y
    // Permite schimb de banda si saritura queued in timpul rostogolirii
    private async Awaitable RollAsync()
    {
        m_IsRolling = true;

        // Dezactiveaza gravitatia si coliziunea cu solul pe durata rolei
        m_Rigidbody.useGravity = false;
        m_Rigidbody.linearVelocity = Vector3.zero;

        Physics.IgnoreLayerCollision(m_PlayerLayer, m_GroundLayer, true);

        // Micsoreaza capsula (jucatorul se "aplatizeaza")
        if (m_Capsule != null)
        {
            m_Capsule.height = m_RollCapsuleHeight;
            m_Capsule.center = m_RollCapsuleCenter;
        }

        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();

        float laneDuration = m_RoadView.LaneChangeDuration;
        var lane = new LaneChangeState
        {
            StartX = m_Rigidbody.position.x,
            TargetX = m_RoadView.LanePositions[m_CurrentLane],
            Active = false
        };

        // Faza 1: coboara Y la RollY
        await RollLerpYAsync(GroundY, RollY, RollDownTime, lane, laneDuration);

        // Faza 2: mentine pozitia joasa, asculta input
        bool jumpQueued = false;
        float elapsed = 0f;

        while (elapsed < RollHoldTime && !m_PlayerView.destroyCancellationToken.IsCancellationRequested)
        {
            Movement input = ReadInputDirect();

            if (input == Movement.Jump) { jumpQueued = true; break; } // Saritura queued

            if (!lane.Active && TryStartAirLaneChange(input, lane))
                AudioManager.Instance.PlaySwipe();

            float currentX = TickLaneX(lane, laneDuration);
            m_Rigidbody.MovePosition(new Vector3(currentX, m_Rigidbody.position.y, m_Rigidbody.position.z));

            await Awaitable.NextFrameAsync();
            elapsed += Time.deltaTime;
        }

        // Faza 3: ridica Y inapoi la GroundY
        await RollLerpYAsync(RollY, GroundY, RollUpTime, lane, laneDuration);

        float finalX = m_RoadView.LanePositions[m_CurrentLane];
        m_Rigidbody.MovePosition(new Vector3(finalX, GroundY, m_Rigidbody.position.z));

        // Restaureaza capsula si coliziunile
        if (m_Capsule != null)
        {
            m_Capsule.height = m_OriginalCapsuleHeight;
            m_Capsule.center = m_OriginalCapsuleCenter;
        }

        Physics.IgnoreLayerCollision(m_PlayerLayer, m_GroundLayer, false);
        m_Rigidbody.useGravity = false;
        m_Rigidbody.linearVelocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;

        m_IsRolling = false;

        // Executa saritura queued imediat dupa terminarea rolei
        if (jumpQueued)
        {
            m_Rigidbody.position = new Vector3(finalX, GroundY, m_Rigidbody.position.z);
            m_Animator.SetTrigger(s_JumpHash);
            await JumpAsync();
        }
    }

    // Interpoleaza Y de la fromY la toY in timp ce permite si schimb de banda
    private async Awaitable RollLerpYAsync(float fromY, float toY, float duration,
        LaneChangeState lane, float laneDuration)
    {
        float elapsed = 0f;
        float invDuration = 1f / duration;

        while (elapsed < duration && !m_PlayerView.destroyCancellationToken.IsCancellationRequested)
        {
            if (!lane.Active && TryStartAirLaneChange(ReadInputDirect(), lane))
                AudioManager.Instance.PlaySwipe();

            float currentX = TickLaneX(lane, laneDuration);
            float currentY = Mathf.Lerp(fromY, toY, elapsed * invDuration);
            m_Rigidbody.MovePosition(new Vector3(currentX, currentY, m_Rigidbody.position.z));

            await Awaitable.NextFrameAsync();
            elapsed += Time.deltaTime;
        }
    }

    // Utilitare: deplaseaza rigidbody doar pe o singura axa
    private void MoveX(float x) =>
        m_Rigidbody.MovePosition(new Vector3(x, m_Rigidbody.position.y, m_Rigidbody.position.z));

    private void MoveY(float y) =>
        m_Rigidbody.MovePosition(new Vector3(m_Rigidbody.position.x, y, m_Rigidbody.position.z));

    // Interpoleaza Y simplu (fara schimb de banda) - folosit in alte contexte
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