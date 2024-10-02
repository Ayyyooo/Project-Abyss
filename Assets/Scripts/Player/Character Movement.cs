using UnityEngine;
using FishNet.Object;

//This is made by Bobsi Unity - Youtube
public class PlayerController : NetworkBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    

    [Header("Settings")]
    public GamePlayerSettings playerSettings;


    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField]
    public Camera PlayerCamera;


    public override void OnStartClient()
    {
        base.OnStartClient();

        // Check if the current client owns this player object
        if (base.IsOwner)
        {
            // Find the Camera attached to the player prefab
            PlayerCamera = GetComponentInChildren<Camera>();

            if (PlayerCamera != null)
            {
                // Enable the camera for the local player
                PlayerCamera.enabled = true;
                PlayerCamera.GetComponent<AudioListener>().enabled = true;
            }
        }
        else
        {
            // Disable the player controller for non-owned players
            gameObject.GetComponent<PlayerController>().enabled = false;

            // Disable the camera to avoid multiple active cameras
            Camera PlayerCamera = GetComponentInChildren<Camera>();

            if (PlayerCamera != null)
            {
                PlayerCamera.enabled = false;
                PlayerCamera.GetComponent<AudioListener>().enabled = false;
            }
        }
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        bool isRunning = false;

        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
        
        // Player and Camera rotation
        if (canMove && PlayerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * (playerSettings.YInverted ? -playerSettings.YSensitivity : playerSettings.YSensitivity);
            rotationX = Mathf.Clamp(rotationX, -playerSettings.lookXLimit, playerSettings.lookXLimit);
            PlayerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * (playerSettings.XInverted ? -playerSettings.XSensitivity : playerSettings.XSensitivity), 0);
        }
    }
}
