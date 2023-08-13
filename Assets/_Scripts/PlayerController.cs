using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Rotation")]
    [SerializeField] public GameObject viewPoint;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private bool enableInvertedLook = false;
    private float verticalRotStore;

    [Header("Mouse Visibility")] 
    [SerializeField] private bool mouseCursorVisible = true;

    [Header("Player Movement")]
    [SerializeField] [Range(1, 20)] private float moveSpeed = 5f;
    private CharacterController characterController;
    private float gravity = -9.8f;

    private Vector3 movement;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (!mouseCursorVisible)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        // Handles Player Movement
        PlayerMovement();

        // Handle Player Rotation
        PlayerRotation();

        // Handles Gravity
        CheckPlayerOnGround();

        characterController.Move(movement);
    }

    private void PlayerRotation()
    {
        var mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        PlayerYRotation(mouseInput);
        PlayerChildViewPointXRotation(mouseInput);

        // Handles the rotation of the Y axis in parent game object
        void PlayerYRotation(Vector2 mouseInput)
        {
            // Get the change in mouse position since the last frame
            float mouseX = mouseInput.x;

            // Adjust the player's yaw based on the mouse movement
            float playerYaw = transform.eulerAngles.y + mouseX * mouseSensitivity;

            // Create a new rotation for the player based on the updated yaw
            Quaternion newRotation = Quaternion.Euler(transform.eulerAngles.x, playerYaw, transform.eulerAngles.z);

            // Apply the new rotation to the player's Transform component
            transform.rotation = newRotation;

        }

        // Handles the rotation of the X axis in child game object
        void PlayerChildViewPointXRotation(Vector2 mouseInput)
        {
            // Invert the Y-axis input if specified
            float mouseY = (enableInvertedLook == true) ? mouseInput.y : mouseInput.y * -1;

            // Update the vertical rotation store based on the mouse input and sensitivity
            verticalRotStore += mouseY * mouseSensitivity;

            // Clamp the vertical rotation store to a specified range
            verticalRotStore = Mathf.Clamp(verticalRotStore, -60, 60);

            // Create a new rotation for the viewPoint based on the clamped vertical rotation
            Quaternion newRotation = Quaternion.Euler(verticalRotStore, viewPoint.transform.eulerAngles.y, viewPoint.transform.eulerAngles.z);

            // Apply the new rotation to the viewPoint's rotation
            viewPoint.transform.rotation = newRotation;
        }
    }

    private void PlayerMovement()
    {
        float horiztonalDelta = Input.GetAxisRaw("Horizontal");
        float verticalDelta = Input.GetAxisRaw("Vertical");

        // Create a movement vector based on the input values
        Vector3 inputDirection = new Vector3(horiztonalDelta, 0f, verticalDelta);
        inputDirection.Normalize();

        #region Another way to create local direction using player's rotation
        //// Get the player's current rotation
        //Quaternion playerRotation = transform.rotation;

        //// Transform the input vector into the player's local space
        //Vector3 localDirection = playerRotation * inputDirection;
        #endregion

        // Transform the input vector into the player's local space
        Vector3 localDirection = (transform.forward * inputDirection.z) + (transform.right * inputDirection.x);

        movement = localDirection * moveSpeed * Time.deltaTime;
    }

    private void CheckPlayerOnGround()
    {
        if(!characterController.isGrounded)
        {
            movement += transform.up * gravity * Time.deltaTime;
        }
    }
}

