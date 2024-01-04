using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicOpening : MonoBehaviour
{
    [SerializeField] private AudioSource BGMusic;


    private void OnEnable()
    {
        BGMusic.Play();
    }

    private void OnDisable()
    {
        BGMusic.Stop();
    }
}
