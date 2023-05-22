using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackMirrorSparkles : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) _animator.Play("BackMirror");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _animator.Play("Empty");
    }
}
