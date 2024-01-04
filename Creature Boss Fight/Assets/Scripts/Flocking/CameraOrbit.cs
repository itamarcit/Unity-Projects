using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;        // The object to orbit around
    public float rotationSpeed = 10f;
    public float returnSpeed = 20f;  // Speed at which the camera returns to the original position

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isRotating = false;
    private bool cameraReturnedToOrigPosition = true;

    public bool IsCameraInGamePosition()
    {
        return cameraReturnedToOrigPosition;
    }

    private void Start()
    {
        originalRotation = transform.rotation;
        originalPosition = transform.position;
        targetRotation = originalRotation;
        targetPosition = originalPosition;
    }

    private void Update()
    {
        if (isRotating)
        {
            // Rotate the camera around the target
            transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);
            transform.LookAt(target);
        }
        else if (!cameraReturnedToOrigPosition)
        {
            MoveToOriginalPositionSmoothly();
        }
    }

    private void MoveToOriginalPositionSmoothly()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, returnSpeed * Time.deltaTime);
        transform.position = Vector3.Slerp(transform.position, targetPosition, returnSpeed * Time.deltaTime);
        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            cameraReturnedToOrigPosition = true;
        }
    }

    public void StartCameraRotation()
    {
        targetRotation = transform.rotation; // Set the target rotation to the current rotation
        isRotating = true;
        cameraReturnedToOrigPosition = false;
    }

    public void StopCameraRotation()
    {
        isRotating = false;
        targetRotation = originalRotation; // Set the target rotation to the original rotation
    }
}