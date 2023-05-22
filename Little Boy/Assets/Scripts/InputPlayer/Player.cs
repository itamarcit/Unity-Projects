using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
	private Rigidbody2D myRigidbody2D = null;
	private SpriteRenderer _renderer;
	private Vector2 _rawInput;
	private Vector2 _prevVelocity;
	[SerializeField] private float playerSpeed = 1;
	private Animator _animator;
	private static readonly int MoveX = Animator.StringToHash("move X");
	private static readonly int MoveY = Animator.StringToHash("move Y");

	private bool StopMovmentOnStageComplete = false;
	private bool _dying = false;
	private bool _changeDirectionInLastTime;
	private DateTime _lastChangeDirection;
	
	private void Awake()
	{
		myRigidbody2D = GetComponent<Rigidbody2D>();
		_animator = GetComponent<Animator>();
		_renderer = GetComponent<SpriteRenderer>();
	}
	private void OnMovement(InputValue value)
	{
		Vector2 inputMovement = value.Get<Vector2>();
		_rawInput = inputMovement;
	}

	private void Update()
	{
		StopMovementOnCollision();
	}

	private void CheckIfChangeDirection(Vector2 roundedInput)
	{
		if (myRigidbody2D.velocity != roundedInput)
		{
			_lastChangeDirection = DateTime.Now;
		}
	}

	public bool DidChangeDirectionRecently()
	{
		return (DateTime.Now - _lastChangeDirection).Seconds < 0.3f;
	}

	private void StopMovementOnCollision()
	{
		List<float> allowedVelocities = new()
		{
			0,
			1,
			-1
		};
		Vector2 currentVelocity = myRigidbody2D.velocity;
		if (!allowedVelocities.Contains(currentVelocity.x) || !allowedVelocities.Contains(currentVelocity.y))
		{
			myRigidbody2D.velocity = Vector2.zero;
		}
	}

	private void FixedUpdate()
	{
		if (StopMovmentOnStageComplete)
		{
			ActivatePlayerIdleAnimation();
			return;
		}
		Vector2 roundedInput = RoundMovementInput();
		if (roundedInput != Vector2.zero)
		{
			CheckIfChangeDirection(roundedInput);
			ActivatePlayerWalkingAnimation(roundedInput);
		}
		else
		{
			ActivatePlayerIdleAnimation();
		}
		myRigidbody2D.velocity = roundedInput * playerSpeed;
		if (myRigidbody2D.velocity != Vector2.zero)
		{
			_prevVelocity = myRigidbody2D.velocity;
		}
	}

	private void ActivatePlayerIdleAnimation()
	{
		_animator.Play("Idle Anim");
	}

	public Vector2 GetInput()
	{
		return _prevVelocity;
	}

	private void ActivatePlayerWalkingAnimation(Vector2 roundedInput)
	{
		_animator.Play("Walking Anim");
		_animator.SetFloat(MoveX, _prevVelocity.x);
		_animator.SetFloat(MoveY, _prevVelocity.y);
	}

	// Rounds the raw input and returns a new vector2 which is in the form of one of the following:
	// (-1, 0), (1, 0), (0, -1), (0, 1), (0, 0)
	private Vector2 RoundMovementInput()
	{
		Vector2 result;
		if (_rawInput.x == 0 || _rawInput.y == 0) return _rawInput;
		if (_rawInput.x != 0)
		{
			result = new Vector2(_rawInput.x, 0);
			return result.normalized;
		}
		// If this line is reached, _rawInput.y != 0
		result = new Vector2(_rawInput.x, 0);
		return result.normalized;
	}

	// Kill by monster

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (other.transform.CompareTag("Enemy") && !_dying)
		{
			_dying = true;
			StopMovement();
			StartCoroutine(DissolveAndReloadScene());
		}
	}

	private IEnumerator DissolveAndReloadScene()
	{
		StartCoroutine(CharacterUtils.Dissolve(_renderer));
		yield return new WaitForSeconds(1f);
		SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
	}

	public void StopMovement()
	{
		StopMovmentOnStageComplete = true;
	}
}