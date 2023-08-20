using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Player Rotation")]
    [SerializeField] public GameObject viewPoint;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private bool enableInvertedLook = false;
    private float verticalRotStore;

    [Header("Mouse Visibility")]
    [SerializeField] private bool mouseCursorVisible = true;

    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] public float runSpeed = 8f;
    private CharacterController characterController;
    private float jumpDistance = 3f;
    private float activeMoveSpeed;

    [Header("Ground Check")]
    [SerializeField] public Transform groundCheck;
    private float groundRadius = 0.2f;
    public bool isGrounded;
    public LayerMask groundMask;
    private Vector3 velocity;
    private const float _GRAVITY = -9.81f;
    [SerializeField] private float gravityModifier = 1.5f;

    private Vector3 movement;
    private Camera cam;

    public Animator animator;

    public GameObject playerModel;

    private void Awake()
    {
        // Initialize references and set mouse cursor behavior
        characterController = GetComponent<CharacterController>();
        cam = Camera.main;

        if (!mouseCursorVisible)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            playerModel.SetActive(false);
        }
    }
    private void Update()
    {
        // Check is the player is controlling locally
        if (!photonView.IsMine)
            return;

        // Handles Gravity
        CheckPlayerOnGround();

        // Handles Player Movement
        Movement();

        // Handle Player Rotation
        PlayerRotation();

        // Move the game object
        characterController.Move(movement);

        // Handles Jump
        PlayerJump();

        // Apply gravity
        ApplyGravity();

        // Move the player with calculated velocity
        characterController.Move(velocity * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        //CheckPlayerOnGround();
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine)
            return;

        // Update camera position and rotation to match viewPoint
        FollowCamera();

        // Call the method to disable cursor lock
        DisableCursorLock();
    }

    private void FollowCamera()
    {
        // Move the camera to the position and rotation of the viewPoint
        cam.transform.position = viewPoint.transform.position;
        cam.transform.rotation = viewPoint.transform.rotation;
    }

    private void PlayerRotation()
    {
        // Get mouse input for rotation
        var mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Handle Y-axis rotation of the player
        PlayerYRotation(mouseInput);

        // Handle X-axis rotation of the child viewPoint
        PlayerChildViewPointXRotation(mouseInput);

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

    private void Movement()
    {
        // Get player input for movement
        float horiztonalDelta = Input.GetAxisRaw("Horizontal");
        float verticalDelta = Input.GetAxisRaw("Vertical");

        // Calculate input direction and normalize it
        Vector3 inputDirection = new Vector3(horiztonalDelta, 0f, verticalDelta);
        inputDirection.Normalize();

        // Transform local direction based on player's orientation
        Vector3 localDirection = (transform.forward * inputDirection.z) + (transform.right * inputDirection.x);

        // Set active move speed based on whether the player is running
        if (Input.GetKey(KeyCode.LeftShift))
            activeMoveSpeed = runSpeed;
        else
            activeMoveSpeed = moveSpeed;

        // Calculate movement based on local direction and speed
        movement = localDirection * activeMoveSpeed * Time.deltaTime;

        // Animation
        animator.SetFloat("Speed", movement.magnitude);
    }

    private void PlayerJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // https://discussions.unity.com/t/jumping-a-specific-height-using-velocity-gravity/125103
            // Apply vertical impulse for jumping
            velocity.y = Mathf.Sqrt(jumpDistance * -2 * _GRAVITY);

            // animation
            animator.SetTrigger("Jump");
            //animator.SetBool("Jump", (velocity.y > 0.2));
        }
    }

    private void CheckPlayerOnGround()
    {
        // Check if the player is grounded using a sphere check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);

        // Apply a slight negative vertical velocity when grounded to ensure the player sticks to the ground
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    private void ApplyGravity()
    {
        // Apply constant gravity to the vertical velocity
        velocity.y += _GRAVITY * gravityModifier * Time.deltaTime;
    }

    private void DisableCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
