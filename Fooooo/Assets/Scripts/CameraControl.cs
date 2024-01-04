using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform playerTransform;
    public Vector3 offset = new Vector3(0f, 3f, -5f);
    public float rotationSpeed = 5f;

    void LateUpdate()
    {
        Vector3 targetPosition = playerTransform.position + offset;
        transform.position = targetPosition;

        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        if (moveVertical != 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(playerTransform.forward);
            targetRotation *= Quaternion.Euler(0f, 180f, 0f); // Rotate by 180 degrees around y-axis
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else if (moveHorizontal != 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(playerTransform.right);
            targetRotation *= Quaternion.Euler(0f, 90f * Mathf.Sign(moveHorizontal), 0f); // Rotate by 90 degrees around y-axis
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
