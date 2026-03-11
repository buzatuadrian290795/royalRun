using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float xClamp = 3f;

    float horizontalInput;
    float fixedZPosition;
    Rigidbody rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        fixedZPosition = rigidBody.position.z;

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
        HandleMovement();
    }

    private void ReadInput()
    {
        horizontalInput = 0f;

        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                Vector2 touchPosition = touch.screenPosition;

                if (touchPosition.x < Screen.width / 2f)
                    horizontalInput = -1f;
                else
                    horizontalInput = 1f;
            }
            Debug.Log("Touch pos: " + touch.screenPosition + " Screen width: " + Screen.width);
        }
    }

    private void HandleMovement()
    {
        Vector3 currentPosition = rigidBody.position;
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, 0f);
        Vector3 newPosition = currentPosition + moveDirection * moveSpeed * Time.fixedDeltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, -xClamp, xClamp);
        newPosition.z = fixedZPosition;

        rigidBody.MovePosition(newPosition);
    }
}