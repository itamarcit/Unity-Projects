using UnityEngine;

public class OldLadyBehavior : MonoBehaviour
{
	private Animator _animator;
	private Rigidbody _rigidbody;
	private static readonly int IsWalking = Animator.StringToHash("IsWalking");
	private bool _isWalking;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_rigidbody = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		bool oldState = _isWalking;
		_isWalking = _rigidbody.velocity.magnitude > 0;
		if (oldState != _isWalking)
		{
			_animator.SetBool(IsWalking, _isWalking);
		}
	}

    
}