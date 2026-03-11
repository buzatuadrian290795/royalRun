using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float laneChangeDuration = 0.25f;
    [SerializeField] float[] lanePositions = { -3f, 0f, 3f };
    [SerializeField] Animator animator;

    Rigidbody rigidBody;
    Transform cachedTransform;
    float fixedZPosition;
    float halfScreenWidth;

    int currentLane = 1;

    bool isChangingLane;
    float laneChangeTimer;
    float startX;
    float targetX;

    private void Awake()
    {
        cachedTransform = transform;

        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (rigidBody == null)
        {
            Debug.LogError("PlayerController: Rigidbody not found.");
            enabled = false;
            return;
        }

        if (animator == null)
        {
            Debug.LogError("PlayerController: Animator not found.");
        }

        if (lanePositions == null || lanePositions.Length != 3)
        {
            Debug.LogError("PlayerController: lanePositions must contain exactly 3 values.");
            enabled = false;
            return;
        }

        fixedZPosition = rigidBody.position.z;
        halfScreenWidth = Screen.width * 0.5f;

        currentLane = Mathf.Clamp(currentLane, 0, lanePositions.Length - 1);
        targetX = lanePositions[currentLane];

        Vector3 startPosition = rigidBody.position;
        startPosition.x = targetX;
        startPosition.z = fixedZPosition;
        rigidBody.position = startPosition;

        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
    }

    private void OnDestroy()
    {
        TouchSimulation.Disable();
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        HandleLaneChangeMovement();
    }

    private void ReadInput()
    {
        if (isChangingLane)
            return;

        for (int i = 0; i < Touch.activeTouches.Count; i++)
        {
            Touch touch = Touch.activeTouches[i];

            if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
                continue;

            if (touch.screenPosition.x < halfScreenWidth)
                TryMoveLeft();
            else
                TryMoveRight();

            return;
        }
    }

    private void TryMoveLeft()
    {
        if (currentLane <= 0)
            return;

        StartLaneChange(currentLane - 1);
    }

    private void TryMoveRight()
    {
        if (currentLane >= lanePositions.Length - 1)
            return;

        StartLaneChange(currentLane + 1);
    }

    private void StartLaneChange(int newLane)
    {
        if (newLane == currentLane)
            return;

        isChangingLane = true;
        laneChangeTimer = 0f;
        startX = rigidBody.position.x;
        targetX = lanePositions[newLane];
        currentLane = newLane;

        if (animator != null)
            animator.SetBool("IsJumping", true);
    }

    private void HandleLaneChangeMovement()
    {
        if (!isChangingLane)
            return;

        laneChangeTimer += Time.fixedDeltaTime;

        float duration = laneChangeDuration > 0f ? laneChangeDuration : 0.0001f;
        float t = laneChangeTimer / duration;

        if (t >= 1f)
        {
            t = 1f;
            isChangingLane = false;
        }

        Vector3 position = rigidBody.position;
        position.x = Mathf.Lerp(startX, targetX, t);
        position.z = fixedZPosition;

        rigidBody.MovePosition(position);

        if (!isChangingLane && animator != null)
            animator.SetBool("IsJumping", false);
    }

    public void SetLaneInstant(int laneIndex)
    {
        laneIndex = Mathf.Clamp(laneIndex, 0, lanePositions.Length - 1);

        currentLane = laneIndex;
        targetX = lanePositions[currentLane];
        startX = targetX;
        laneChangeTimer = 0f;
        isChangingLane = false;

        Vector3 position = rigidBody.position;
        position.x = targetX;
        position.z = fixedZPosition;

        rigidBody.position = position;

        if (animator != null)
            animator.SetBool("IsJumping", false);
    }

    public int GetCurrentLane()
    {
        return currentLane;
    }
}