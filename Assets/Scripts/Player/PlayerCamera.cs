using UnityEngine;
using DG.Tweening;
public class PlayerCamera : MonoBehaviour
{
    public float sensX = 100f;   // Sensitivity for X-axis
    public float sensY = 100f;   // Sensitivity for Y-axis
    public Transform orientation; // Reference to the orientation Transform
    private float xRotation;      // Vertical rotation
    private float yRotation;      // Horizontal rotation

    private void Start()
    {
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation
        transform.rotation = Quaternion.Euler(0, 0, 0); // Optional, set initial rotation
        orientation.rotation = Quaternion.Euler(0, 0, 0); // Optional, set initial rotation
    }

    private void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        // Update rotation values
        yRotation += mouseX;  // Horizontal rotation
        xRotation -= mouseY;  // Vertical rotation
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp vertical rotation

        // Apply rotation to camera and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

}
