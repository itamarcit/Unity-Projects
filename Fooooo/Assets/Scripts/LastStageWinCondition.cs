using System;
using UnityEngine;

public class LastStageWinCondition : MonoBehaviour
{
    private static int _counter;
    private Quaternion _origRotation;
    private Vector3 _origPosition;
    private const int LAST_STAGE_LETTERS = 13;
    private bool _didUpdateCounter;

    private void Awake()
    {
        _origPosition = transform.localPosition;
        _origRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        _counter = 0;
        transform.localPosition = _origPosition;
        transform.localRotation = _origRotation;
        _didUpdateCounter = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.CompareTag("downBlock") || other.gameObject.CompareTag("Cat Island")) && !_didUpdateCounter)
        {
            _didUpdateCounter = true;
            _counter++;
        }
        if (_counter >= LAST_STAGE_LETTERS) GameManager.Shared.LevelComplete();
    }
}
