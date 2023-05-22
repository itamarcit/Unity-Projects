using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class miniMapScript : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    private Vector3 playerPos;
    private Vector3 camPos;

    [SerializeField] private float  xMax;
    [SerializeField] private float xMin;
    [SerializeField] private float yMax;
    [SerializeField] private float yMin;


    void Update()
    {
        playerPos = Player.transform.position;
        if (xMin < playerPos.x && playerPos.x < xMax)
        {
            transform.position = new Vector3(playerPos.x, transform.position.y, transform.position.z);
        }
        if (yMin < playerPos.y && playerPos.y < yMax)
        {
            transform.position = new Vector3(transform.position.x,playerPos.y, transform.position.z);
        }
        
    }
}
