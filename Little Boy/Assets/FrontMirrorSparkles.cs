using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontMirrorSparkles : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) _animator.Play("FrontMirror");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _animator.Play("Empty");
    }
}
