using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    private const string GAME_OVER_MESSAGE = "GAME OVER";
    public List<GameObject> healthObjects;

    private int _length;

    private void Start()
    {
        _length = healthObjects.Count;
    }

    void Update()
    {
        if (_length > 0 && Input.GetKeyDown(KeyCode.Space))
        {
            Destroy(healthObjects[_length - 1]);
            _length--;
            if (_length == 0)
            {
                print(GAME_OVER_MESSAGE);
            }
        }
    }
}