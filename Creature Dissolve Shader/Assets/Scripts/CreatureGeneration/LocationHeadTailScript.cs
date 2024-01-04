using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class LocationHeadTailScript : MonoBehaviour
{
    private const float X_HEAD = -0.5f;
    private const float Y_HEAD = 3f;
    private const float Z_HEAD = -6.6f;
    
    private const float X_TAIL = -0.5f;
    private const float Y_TAIL = 7.2f;
    private const float Z_TAIL = 8.5f;
    
    public GameObject head;
    public GameObject tail;

    
    // Just for beauty of the creature.
    void Start()
    {
        head.transform.position = new Vector3(X_HEAD, Y_HEAD, Z_HEAD);
        tail.transform.position = new Vector3(X_TAIL, Y_TAIL, Z_TAIL);
    }

}
