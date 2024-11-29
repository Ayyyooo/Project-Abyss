using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;  // Serialized field to ensure visibility
    [SerializeField] private float rotationSpeed = 10f;  // Serialized for visibility in Inspector

    void Update()
    {
        RotateCharacterToCamera();
    }

    void RotateCharacterToCamera()
    {
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;  // Prevent rotation on the Y axis

        if (forward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
