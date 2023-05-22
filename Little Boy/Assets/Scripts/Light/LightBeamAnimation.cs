using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBeamAnimation : MonoBehaviour
{
    [SerializeField] private Texture[] textures;
    private int animationStep;
    [SerializeField] private float fps = 4;
    private float fpsCounter;
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        fpsCounter += Time.deltaTime;
        if (fpsCounter >= 1f / fps)
        {
            animationStep++;
            if (animationStep == textures.Length)
            {
                animationStep = 0; 
            }
            _lineRenderer.material.SetTexture("_MainTex", textures[animationStep]);
            fpsCounter = 0;
        }
        
    }
}
