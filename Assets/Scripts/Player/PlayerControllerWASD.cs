using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerWASD : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float xClamp = 3f;
    //[SerializeField] float zClamp = 1f;

    Vector2 movement;
    Rigidbody rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    public void Move(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
        Debug.Log(movement);
    }

    private void HandleMovement()
    {
        Vector3 currentPosition = GetComponent<Rigidbody>().position;
        //Vector3 moveDirection = new Vector3(movement.x, 0f, movement.z);
        Vector3 moveDirection = new Vector3(movement.x, 0f, 0f);
        Vector3 newPosition = currentPosition + moveDirection * (moveSpeed * Time.fixedDeltaTime);

        newPosition.x = Mathf.Clamp(newPosition.x, -xClamp, xClamp);
        //newPosition.z = Mathf.Clamp(newPosition.z, -zClamp, zClamp);

        GetComponent<Rigidbody>().MovePosition(newPosition);
    }
}
