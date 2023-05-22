using System.Collections;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    private const float CLOSE_DOOR_DELAY = 1f;
    private Animator _doorAnimator;
    [SerializeField] private PlayerOneManager playerOneManager;
    [SerializeField] private GameObject doorCollider;
    private bool _isOpen = false;
    private static readonly int IsOpen = Animator.StringToHash("Is Open");

    private void Awake()
    {
        _doorAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (playerOneManager.NumberOfFollowers() == 0 && _isOpen)
        {
            // _doorAnimator.SetBool(IsOpen, false);
            _isOpen = false;
            StartCoroutine(DelayBeforeSettingActive(CLOSE_DOOR_DELAY, true));
        }
        else if (playerOneManager.NumberOfFollowers() > 0 && !_isOpen)
        {
            _doorAnimator.SetBool(IsOpen, true);
            _isOpen = true;
            StartCoroutine(DelayBeforeSettingActive(CLOSE_DOOR_DELAY, false));
        }
    }

    private IEnumerator DelayBeforeSettingActive(float delay, bool isOpen)
    {
        yield return new WaitForSeconds(delay);
        _doorAnimator.SetBool(IsOpen, !isOpen);
        doorCollider.SetActive(isOpen);
    }
}
