using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	private const float DELAY_AFTER_MOVING_IN_NEW_DIRECTION_SECONDS = 0.05f;
#region Fields
	[SerializeField] private float movementTotalTime = 0.3f;
	private Rigidbody2D _rigidbody;
	private Animator _animator;
	private bool _isDead;
	private bool _up;
	private bool _down;
	private bool _right;
	private bool _left;
	private bool _isMoving;
	private bool _isFiring;
	private bool _isDeadWhileMoving;
	private Vector3 _origPosition;
	private Vector3 _targetPosition;
	private Vector3 _startScale;
	private Vector3 _startPosition;
	private float _movementElapsedTime;
	private float _animatorSpeed;
	private float noMovementTimer;
	private Vector2Int _gridTarget;
	private Vector2Int _gridPosition;
	private Vector2Int _facingDirection;
	private const float NO_MOVEMENT_TIMER = 0.75f;
	private static readonly int IsMoving = Animator.StringToHash("IsMoving");
	private static readonly int IsDigging = Animator.StringToHash("IsDigging");
	private static readonly int IsDeadByRock = Animator.StringToHash("IsDeadByRock");
	private static readonly int IsDeadByEnemy = Animator.StringToHash("IsDeadByEnemy");
	private Coroutine _moveCoroutine;
	private static readonly int IsShooting = Animator.StringToHash("IsShooting");
#endregion

#region Events
	private void Start()
	{
		_animator = GetComponent<Animator>();
		_rigidbody = GetComponent<Rigidbody2D>();
		_animatorSpeed = _animator.speed;
		_gridPosition = GridManager.Shared.GetStartLocationGrid();
		_startPosition = transform.position;
		_startScale = transform.localScale;
		_facingDirection = Vector2Int.right;
		noMovementTimer = NO_MOVEMENT_TIMER;
	}

	private void Update()
	{
		_up = Input.GetKey(KeyCode.UpArrow);
		_down = Input.GetKey(KeyCode.DownArrow);
		_left = Input.GetKey(KeyCode.LeftArrow);
		_right = Input.GetKey(KeyCode.RightArrow);
		if (noMovementTimer <= 0)
		{
			GameManager.Shared.SetMusic(false);
			noMovementTimer = NO_MOVEMENT_TIMER;
		}
		else
		{
			noMovementTimer -= Time.deltaTime;
		}
	}

	private void FixedUpdate()
	{
		HandleMovement();
	}
#endregion

#region Methods
	private void HandleMovement()
	{
		if (GameManager.Shared.GetGamePausedTimer() > 0 || _isFiring)
		{
			return;
		}
		if (_up && _down)
		{
		}
		else if (_up && !_isMoving)
		{
			_isMoving = true;
			_moveCoroutine = StartCoroutine(MovePlayer(Vector3Int.up, true));
		}
		else if (_down && !_isMoving)
		{
			_isMoving = true;
			_moveCoroutine = StartCoroutine(MovePlayer(Vector3Int.down, true));
		}
		else if (_left && _right)
		{
		}
		else if (_left && !_isMoving)
		{
			_isMoving = true;
			_moveCoroutine = StartCoroutine(MovePlayer(Vector3Int.left, false));
		}
		else if (_right && !_isMoving)
		{
			_isMoving = true;
			_moveCoroutine = StartCoroutine(MovePlayer(Vector3Int.right, false));
		}
		else if (!_isMoving)
		{
			_animator.SetBool(IsMoving, false);
		}
	}

	private IEnumerator MovePlayer(Vector3Int direction, bool isUpDownMovement)
	{
		if (_isDead)
		{
			_isMoving = false;
			yield break;
		}
		bool shouldReceiveDigPoints = false;
		_origPosition = transform.position;
		if (isUpDownMovement)
		{
			_targetPosition = _origPosition + (Vector3)direction * GridManager.Shared.GetGridSpacingY();
		}
		else
		{
			_targetPosition = _origPosition + (Vector3)direction * GridManager.Shared.GetGridSpacingX();
		}
		Vector2Int targetGridPosition = _gridPosition + new Vector2Int(direction.x, direction.y);
		_gridTarget = targetGridPosition;
		Vector2Int oldFacingDirection = _facingDirection;
		_facingDirection = new Vector2Int(direction.x, direction.y);
		if (!GridManager.Shared.IsInRangeAfterMovement(_gridPosition,
			    new Vector2Int(direction.x, direction.y)))
		{
			// checks if is in the grid range after movement, if not changes facing direction and breaks.
			ChangeSprite(direction, false, targetGridPosition);
			_isMoving = false;
			yield break;
		}
		if (!(oldFacingDirection.x == direction.x && oldFacingDirection.y == direction.y))
		{
			// adds to the feel of moving, if you press short you just change direction and don't move 
			ChangeSprite(direction, false, targetGridPosition);
			yield return new WaitForSeconds(DELAY_AFTER_MOVING_IN_NEW_DIRECTION_SECONDS);
			_isMoving = false;
			yield break;
		}
		GameManager.Shared.SetMusic(true);
		noMovementTimer = NO_MOVEMENT_TIMER;
		if (!GridManager.Shared.IsGridSquareDug(targetGridPosition.x, targetGridPosition.y, new[]
		    {
			    GridManager.Direction.East, GridManager.Direction.West, GridManager.Direction.South
		    })) // checks if the square was ever dug at, true if it wasn't
		{
			shouldReceiveDigPoints = true;
		}
		ChangeSprite(direction, true, targetGridPosition);
		yield return StartCoroutine(CommenceMovement(direction)); // responsible for movement
		GridManager.Shared.DigSquare(_gridPosition.x, _gridPosition.y, targetGridPosition.x,
			targetGridPosition.y, GridManager.GetDirectionFromVector(new Vector3Int(direction.x, direction.y, 0)));
		if (!_isDeadWhileMoving)
		{
			transform.position = _targetPosition;
		}
		if (shouldReceiveDigPoints) ScoreManager.Shared.AddScore(ScoreManager.Score.Dig, Vector3.zero, false);
		DigManager.Shared.CreateStationaryHole(_targetPosition, direction);
		_gridPosition = targetGridPosition;
		GameManager.Shared.WasEnemyFreed(_gridPosition);
		_isDeadWhileMoving = false;
		_isMoving = false;
	}

	private IEnumerator CommenceMovement(Vector3Int direction)
	{
		_movementElapsedTime = 0;
		bool middleDrawn = false;
		while (_movementElapsedTime < movementTotalTime)
		{
			if (_isDeadWhileMoving)
			{
				if (!middleDrawn)
				{
					DigManager.Shared.CreateStationaryHole(Vector3.Lerp(_origPosition, _targetPosition, 0.21f),
						direction);
				}
				yield break;
			}
			transform.position =
				Vector3.Lerp(_origPosition, _targetPosition, (_movementElapsedTime / movementTotalTime));
			_movementElapsedTime += Time.deltaTime;
			DigManager.Shared.CreateMovingHole(transform.position + (Vector3)direction * 0.3f);
			if (_movementElapsedTime / movementTotalTime > 0.21f && !middleDrawn)
			{
				middleDrawn = true;
				DigManager.Shared.CreateStationaryHole(transform.position, direction);
			}
			yield return null;
		}
	}

	/**
	 * Changes the sprite of the player to match its direction.
	 */
	private void ChangeSprite(Vector3 direction, bool startedMovement, Vector2Int gridTarget)
	{
		Vector3 currentScale = transform.localScale;
		_animator.SetBool(IsDigging, startedMovement && !GridManager.Shared.IsGridSquareDug(gridTarget.x, gridTarget.y,
			GridManager.GetDirectionFromVector(new Vector3Int((int)-direction.x, (int)-direction.y, 0))));
		_animator.SetBool(IsMoving, startedMovement);
		if (direction == Vector3.up || direction == Vector3.down)
		{
			transform.localScale = new Vector3(Math.Abs(currentScale.x), currentScale.y, currentScale.z);
			transform.localEulerAngles = new Vector3(0, 0, ((direction == Vector3.up) ? 90 : 270));
		}
		else if (direction == Vector3.right)
		{
			transform.localScale = new Vector3(Math.Abs(currentScale.x), currentScale.y, currentScale.z);
			transform.localEulerAngles = new Vector3(0, 0, 0);
		}
		else if (direction == Vector3.left)
		{
			transform.localEulerAngles = new Vector3(0, 0, 0);
			transform.localScale = new Vector3(-Math.Abs(currentScale.x), currentScale.y, currentScale.z);
		}
	}

	public void KillPlayerByRock()
	{
		KillPlayer();
		_animator.SetBool(IsDeadByRock, true);
	}

	public void KillPlayerByEnemy()
	{
		KillPlayer();
	}

	public void StartPlayerDeathAnim()
	{
		_animator.SetBool(IsDeadByEnemy, true);
	}

	private void KillPlayer()
	{
		if (_moveCoroutine != null)
		{
			_isDeadWhileMoving = true;
		}
		_isMoving = false;
		_isDead = true;
		_animator.SetBool(IsMoving, false);
		_animator.SetBool(IsDigging, false);
		_rigidbody.simulated = false;
	}

	public List<Vector2Int> GetGridPositions()
	{
		List<Vector2Int> result = new()
		{
			_gridPosition,
			_gridTarget
		};
		return result;
	}

	public void ResetPlayer()
	{
		if (_moveCoroutine != null)
		{
			_isDeadWhileMoving = false;
		}
		_isDead = false;
		_isMoving = false;
		_animator.speed = _animatorSpeed;
		_animator.SetBool(IsDeadByEnemy, false);
		_animator.SetBool(IsDeadByRock, false);
		_rigidbody.simulated = true;
		_gridPosition = GridManager.Shared.GetStartLocationGrid();
		transform.position = _startPosition;
		transform.localEulerAngles = Vector3.zero;
		transform.localScale = _startScale;
		_facingDirection = Vector2Int.right;
		_animator.SetBool(IsMoving, false);
	}

	public Animator GetAnimator()
	{
		return _animator;
	}

	public bool CanPlayerShoot()
	{
		return !_isMoving;
	}

	public Vector2Int GetFacingDirection()
	{
		return _facingDirection;
	}

	public void SetFiring(bool isFiring)
	{
		_isFiring = isFiring;
		_animator.SetBool(IsShooting, isFiring);
	}
#endregion
}