using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Door : MonoBehaviour
{
    [SerializeField] private Vector2 openDirection;
    [SerializeField] private AudioSource _openDoorSound;
    [SerializeField] private AudioSource _closeDoorSound;
    private Vector2 _closedPosition;
    private Vector2 _openedPosition;
    private bool _isOpen;
    private bool _door;
    float _openTime = 1f;

    private void Start()
    {
        _closedPosition = transform.position;
        _openedPosition = (Vector2)transform.position + openDirection;
    }
    
    private void Update()
    {
        if (_door && Input.GetKeyDown(KeyCode.Space))
        {
            if (_isOpen)
            {
                _openDoorSound.Play();
                StartCoroutine(Close());
            }
            else if (!_isOpen)
            {
                _closeDoorSound.Play();
                StartCoroutine(Open());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) _door = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _door = false;
    }

    IEnumerator Open()
    {
        float elapsedTime = 0;
        while (elapsedTime < _openTime)
        {
            transform.position = Vector2.Lerp(_closedPosition, _openedPosition, (elapsedTime / _openTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _isOpen = true;
        yield return null;
    }

    IEnumerator Close()
    {
        float elapsedTime = 0;
        while (elapsedTime < _openTime)
        {
            transform.position = Vector2.Lerp(_openedPosition, _closedPosition, (elapsedTime / _openTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _isOpen = false;
        yield return null;
    }
}
