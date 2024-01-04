using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingBehavior : MonoBehaviour
{
    public float alignmentRadius = 5f;
    public float maxSpeed = 5f;
    public float grannyRadius = 5f;
    [SerializeField] private List<GameObject> pigeons;
    private bool isGranny;
    private Vector3 keepGrannyGrounded;
    private bool hitDownBlock;
    private Rigidbody rb;
    private Collider[] nearbyColliders;
    [SerializeField] private float force;
    private Vector3 startPos;
    private Quaternion startRotation;

    private void Awake()
    {
        startPos = transform.position;
        startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        transform.position = startPos;
        transform.rotation = startRotation;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (gameObject.CompareTag("Granny"))
        {
            isGranny = true;
        }

        keepGrannyGrounded = new Vector3(1f, 0f, 1f);
    }

    void FixedUpdate()
    {
        Vector3 positionAvg = Vector3.zero;
        if (isGranny) positionAvg = grannyChecks();
        else
        {
            //Calculate the sum of all positions
            for (int i = 0; i < pigeons.Count; i++)
            {
                positionAvg += pigeons[i].gameObject.transform.position;
            }
            positionAvg /= pigeons.Count;
        }
        //Calculate the sum of all positions
        // for (int i = 0; i < pigeons.Count; i++)
        // {
        //     positionAvg += pigeons[i].gameObject.transform.position;
        // }
        
        // Calculate the average position
        //positionAvg /= pigeons.Count;

        // Calculate the direction towards the average position
        Vector3 direction = positionAvg - transform.position;
        float radius;
        if (isGranny) radius = grannyRadius;
        else radius = alignmentRadius;
        // Check if the distance is greater than the alignment radius
        if (direction.magnitude > radius)
        {
            // Normalize the direction and apply a force towards the average position
            Vector3 alignmentForce = direction.normalized * maxSpeed;
            if ( isGranny)
            {
                alignmentForce = Vector3.Scale(alignmentForce, keepGrannyGrounded);
                transform.LookAt(new Vector3(positionAvg.x, transform.position.y, positionAvg.z));
            }
            if((isGranny && !hitDownBlock) || (!isGranny && transform.position.y > 40)) 
                rb.AddForce(alignmentForce);
            else if (isGranny && hitDownBlock) rb.AddForce(Vector3.down*force);
            //if(!hitDownBlock) rb.AddForce(alignmentForce);
        }
    }

    private float checkFallenPigeonsPercentage()
    {
        int sum = 0;
        for (int i = 0; i < pigeons.Count; i++)
        {
            if (pigeons[i].gameObject.transform.position.y < 15)
            {
                sum++;
            }
        }
        return (float)sum / pigeons.Count;
    }


    private Vector3 grannyChecks()
    {
        int toDivide = 0;
        Vector3 positionAvg = Vector3.zero;
        if (checkFallenPigeonsPercentage() < 0.5f)
        {
            // Calculate the sum of all positions
            for (int i = 0; i < pigeons.Count; i++)
            {
                if (pigeons[i].gameObject.transform.position.y > 40)
                {
                    toDivide++;
                    positionAvg += pigeons[i].gameObject.transform.position;
                }
            }
        }
        else
        {
            for (int i = 0; i < pigeons.Count; i++)
            {
                if (pigeons[i].gameObject.transform.position.y < 15)
                {
                    toDivide++;
                    positionAvg += pigeons[i].gameObject.transform.position;
                }
            }
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            if (hit.collider && hit.collider.gameObject.CompareTag("downBlock"))
            {
                // If the raycast hits the "downBlock" layer, set the flag to true
                hitDownBlock = true;
                StartCoroutine(gameOverGrannyFell());
            }
            else
            {
                hitDownBlock = false;
            }
        }

        return positionAvg/toDivide;
    }

    IEnumerator gameOverGrannyFell()
    {
        yield return new WaitForSeconds(1f);
        GameManager.Shared.GameOver();
    }
    
    
}