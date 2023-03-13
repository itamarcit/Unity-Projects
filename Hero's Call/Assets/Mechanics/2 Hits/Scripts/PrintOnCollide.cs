using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintOnCollide : MonoBehaviour
{
    private const string SECOND_HIT_MESSAGE = "Second Hit";

    private void OnCollisionEnter2D(Collision2D col)
    {
        print(SECOND_HIT_MESSAGE);
    }
}
