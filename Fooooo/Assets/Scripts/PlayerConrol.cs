using System;
using UnityEngine;

public class PlayerConrol : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    private Rigidbody playerRigidbody;

    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical);

        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            targetRotation *= Quaternion.Euler(0f, 270f, 0f); // Rotate by 90 degrees around y-axis
            //playerRigidbody.MoveRotation(targetRotation);
            playerRigidbody.velocity = movement.normalized * moveSpeed;
            //playerRigidbody.AddForce(movement.normalized * moveSpeed);
            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            playerRigidbody.velocity = Vector3.zero;
        }
        
    }
}

