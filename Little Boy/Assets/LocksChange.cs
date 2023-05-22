using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

public class LocksChange : MonoBehaviour
{
    public ManegeLevelsLocks locks;
    private int numOfLevelsOpen;

    [SerializeField] private GameObject[] locksSprites;
    [SerializeField] private GameObject[] openLevelSprites;
    [SerializeField] private GameObject[] buttons;


    void Start()
    {
        //DontDestroyOnLoad(this);
        numOfLevelsOpen = 0;
    }

    public void Reset()
    {
        numOfLevelsOpen = 0;
    }

    private void Update()
    {
        numOfLevelsOpen = locks.GetLevel();
        OpenLocks();

        if (Input.GetKeyDown(KeyCode.K))
        {
            locks.AddLevel();
        }
    }

    void OpenLocks()
    {
        for (int i = 0; i < numOfLevelsOpen; i++)
        {
            if (i < 5)
            {
                locksSprites[i].SetActive(false);
                openLevelSprites[i].SetActive(true);
                buttons[i].SetActive(true);
            }
        }
    }
}