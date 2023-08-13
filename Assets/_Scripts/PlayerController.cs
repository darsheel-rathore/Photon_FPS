using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public GameObject viewPoint;
    [SerializeField] public float mouseSensitivity = 2.0f;
    [SerializeField] private bool enableInvertedLook = false;
    [SerializeField] private bool mouseCursorVisible = true;

    private float verticalRotStore;


    private void Start()
    {
        if (!mouseCursorVisible)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;  
        }
    }
    private void Update()
    {
        // Handle Player Rotation
        PlayerRotation();
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
}

