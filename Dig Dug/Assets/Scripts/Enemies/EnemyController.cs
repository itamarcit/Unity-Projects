using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
#region Fields
	[SerializeField] private int startGridPositionX;
	[SerializeField] private int startGridPositionY;
	[SerializeField] private float movementTotalTime = 0.3f;
	[SerializeField] private int[] startingHoleX;
	[SerializeField] private int[] startingHoleY;
	[SerializeField] private int maxFlyCooldown;
	[SerializeField] private int minFlyCooldown;
	[SerializeField] private int minTotalSquaresWhenFlying = 3;
	[SerializeField] private bool canBreathFire;
	[SerializeField] private float inflationTimerPerStage = 1f;
	[SerializeField] private FygarFire fygarFire;
	[SerializeField] private float hitByPlayerWeaponTimer = 0.5f;
	private EnemyController _thisController;
	private float _inflationTimer;
	private float _hitByWeaponTimer;
	private Vector2Int _currentGridPosition;
	private Vector2Int _moveDirection;
	private Vector2Int _originalMoveDirection;
	private bool _isChasingPlayer;
	private bool _isDead;
	private bool _isMoving;
	private bool _isFlying;
	private bool _isInflated;
	private float _movementElapsedTime;
	private float _flyingTimer;
	private int _totalSquaresFlew;
	private Animator _animator;
	private Vector3 _originalPosition;
	private Vector3 _originalScale;
	private Coroutine _moveCoroutine;
	private bool _isSpawning;
	private bool _isWaitingToFire;
	private const int TOTAL_INFLATE_ANIMATIONS = 4;
	private static readonly int IsSpawning = Animator.StringToHash("IsSpawning");
	private static readonly int IsFlying = Animator.StringToHash("IsFlying");
	private static readonly int IsHitByRock = Animator.StringToHash("IsHitByRock");
	private static readonly int HitAmount = Animator.StringToHash("HitAmount");
#endregion

#region Events
	private void Awake()
	{
		_currentGridPosition = new Vector2Int(startGridPositionX, startGridPositionY);
		_flyingTimer = Random.Range(minFlyCooldown, maxFlyCooldown + 1);
		_animator = GetComponent<Animator>();
	}

	private void Start()
	{
		GetRandomDirection();
		_originalMoveDirection = _moveDirection;
		_originalPosition = transform.position;
		_originalScale = transform.localScale;
		_thisController = GetComponent<EnemyController>();
	}

	private void Update()
	{
		UpdateInflation();
		UpdateHitByWeapon();
		if (GameManager.Shared.GetGamePausedTimer() > 0 || _isDead || _isSpawning)
		{
			if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
			return;
		}
		if (_isFlying && !_isMoving)
		{
			List<MyTileData> pathToPlayer = GameManager.Shared.FindRouteToPlayer(_currentGridPosition, true);
			if (pathToPlayer.Count > 1)
			{
				_moveDirection = CheckDirection(new Vector2Int(pathToPlayer[1].GetX(), pathToPlayer[1].GetY()));
			}
			else
			{
				// reached player when flying
				ResetFlying();
				CheckIfCanChasePlayer();
				return;
			}
			_moveCoroutine = StartCoroutine(MoveTo());
		}
		else if (!_isChasingPlayer && !_isMoving && !_isDead)
		{
			if (GameManager.Shared.FindRouteToPlayer(_currentGridPosition, false) != null)
			{
				ChasePlayer();
				return;
			}
			if (GridManager.Shared.IsGridSquareDug(_currentGridPosition.x, _currentGridPosition.y,
				    GridManager.GetDirectionFromVector(new Vector3Int(_moveDirection.x,
					    _moveDirection.y, 0))))
			{
				_moveCoroutine = StartCoroutine(MoveTo());
			}
			else if (!_isWaitingToFire)
			{
				_moveDirection = GridManager.Shared.GetOppositeDirection(_moveDirection);
			}
		}
		else if (_isChasingPlayer && !_isMoving && !_isDead)
		{
			List<MyTileData> pathToPlayer = GameManager.Shared.FindRouteToPlayer(_currentGridPosition, false);
			if (pathToPlayer == null) // no path to player
			{
				GetRandomDirection();
				_isChasingPlayer = false;
			}
			else if (pathToPlayer.Count == 0)
			{
			}
			else if (pathToPlayer.Count == 1) // 
			{
				_moveDirection = Vector2Int.zero; // don't move if you're on the player
			}
			else
			{
				_moveDirection = CheckDirection(new Vector2Int(pathToPlayer[1].GetX(), pathToPlayer[1].GetY()));
				_moveCoroutine = StartCoroutine(MoveTo());
			}
		}
		if (_flyingTimer > 0)
		{
			_flyingTimer -= Time.deltaTime;
		}
		else if (!_isFlying && !_isChasingPlayer && !_isMoving && !_isInflated && !_isWaitingToFire)
		{
			_isFlying = true;
		}
		else if (!_isMoving)
		{
			_flyingTimer = _flyingTimer = Random.Range(minFlyCooldown, maxFlyCooldown);
		}
	}

	private void UpdateHitByWeapon()
	{
		if (_hitByWeaponTimer > 0)
		{
			_hitByWeaponTimer -= Time.deltaTime;
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (!_isFlying && !_isInflated && other.gameObject.CompareTag("Player") && !_isSpawning && !_isDead)
		{
			GameManager.Shared.KillPlayerByEnemy();
		}
	}
#endregion

#region Methods
	public void Inflate()
	{
		if (!_isDead && !_isSpawning && _hitByWeaponTimer <= 0)
		{
			_hitByWeaponTimer = hitByPlayerWeaponTimer;
			_isInflated = true;
			ResetFlying();
			_animator.SetInteger(HitAmount, _animator.GetInteger(HitAmount) + 1);
			if (_animator.GetInteger(HitAmount) >= TOTAL_INFLATE_ANIMATIONS)
			{
				_isDead = true;
				gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
				ScoreManager.Shared.AddScore(ScoreManager.Score.KillByInflate, transform.position, true);
				PowerUpManager.Shared.AddEnergy(PowerUpManager.Shared.energyForInflateKill);
				StartCoroutine(DeactivateAfterSeconds(inflationTimerPerStage));
			}
			_inflationTimer = inflationTimerPerStage;
		}
	}

	private IEnumerator DeactivateAfterSeconds(float i)
	{
		yield return new WaitForSeconds(i);
		gameObject.SetActive(false);
	}

	public void DieByRock()
	{
		_animator.SetBool(IsHitByRock, true);
		_isDead = true;
		if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
	}

	private void GetRandomDirection()
	{
		if (GridManager.Shared.IsGridSquareDug(_currentGridPosition.x, _currentGridPosition.y,
			    GridManager.Direction.North) ||
		    GridManager.Shared.IsGridSquareDug(_currentGridPosition.x, _currentGridPosition.y,
			    GridManager.Direction.South))
		{
			_moveDirection = Vector2Int.up;
		}
		else _moveDirection = Vector2Int.left;
	}

	private bool IsUpDownMovement(Vector2Int direction)
	{
		return direction == Vector2Int.up || direction == Vector2Int.down;
	}

	private IEnumerator MoveTo()
	{
		_isMoving = true;
		if (_isFlying) _animator.SetBool(IsFlying, true);
		Vector3 origPosition = transform.position;
		Vector3 targetPosition;
		if (IsUpDownMovement(_moveDirection))
		{
			targetPosition = origPosition +
							 new Vector3(_moveDirection.x, _moveDirection.y, 0) * GridManager.Shared.GetGridSpacingY();
		}
		else
		{
			targetPosition = origPosition +
							 new Vector3(_moveDirection.x, _moveDirection.y, 0) * GridManager.Shared.GetGridSpacingX();
		}
		Vector2Int targetGridPosition = _currentGridPosition + _moveDirection;
		SetFacingDirection();
		_movementElapsedTime = 0;
		while (_movementElapsedTime < movementTotalTime)
		{
			transform.position =
				Vector3.Lerp(origPosition, targetPosition, (_movementElapsedTime / movementTotalTime));
			_movementElapsedTime += Time.deltaTime;
			while (_isInflated)
			{
				yield return null;
			}
			yield return null;
		}
		transform.position = targetPosition;
		_currentGridPosition = targetGridPosition;
		if (_isFlying && _totalSquaresFlew >= minTotalSquaresWhenFlying && GridManager.Shared.IsGridSquareDug(
			    _currentGridPosition.x, _currentGridPosition.y,
			    new[]
			    {
				    GridManager.Direction.East, GridManager.Direction.North, GridManager.Direction.South,
				    GridManager.Direction.West
			    }))
		{
			ResetFlying();
			CheckIfCanChasePlayer();
			if (!_isChasingPlayer)
			{
				GetRandomDirection();
			}
		}
		else if (_isFlying) _totalSquaresFlew++;
		else if (canBreathFire) fygarFire.AttemptFire(_thisController, _currentGridPosition);
		_isMoving = false;
	}

	private void ResetFlying()
	{
		_isFlying = false;
		_animator.SetBool(IsFlying, false);
		_flyingTimer = Random.Range(minFlyCooldown, maxFlyCooldown);
		_totalSquaresFlew = 0;
	}

	private void CheckIfCanChasePlayer()
	{
		if (GameManager.Shared.FindRouteToPlayer(_currentGridPosition, false) != null) ChasePlayer();
	}

	public Animator GetAnimator()
	{
		return _animator;
	}

	public List<Vector2Int> GetOriginalHolePositions()
	{
		List<Vector2Int> result = new();
		int index = 0;
		foreach (int xPos in startingHoleX)
		{
			result.Add(new Vector2Int(xPos, startingHoleY[index]));
			index++;
		}
		return result;
	}

	public void ChasePlayer()
	{
		_isChasingPlayer = true;
	}

	private Vector2Int CheckDirection(Vector2Int target)
	{
		if (target - _currentGridPosition == Vector2Int.zero) return _moveDirection;
		return target - _currentGridPosition;
	}

	private void SetFacingDirection() // sets the sprite to fit the facing direction.
	{
		if (IsUpDownMovement(_moveDirection)) return;
		Vector3 oldScale = transform.localScale;
		transform.localScale = _moveDirection == Vector2Int.left
			? new Vector3(-Mathf.Abs(oldScale.x), oldScale.y, oldScale.z)
			: new Vector3(Mathf.Abs(oldScale.x), oldScale.y, oldScale.z);
	}

	// accepts only east and west directions.
	public void SetFygarFacingDirection(GridManager.Direction direction)
	{
		Vector3 oldScale = transform.localScale;
		if (direction == GridManager.Direction.East)
		{
			transform.localScale = new Vector3(Mathf.Abs(oldScale.x), oldScale.y, oldScale.z);
		}
		else if (direction == GridManager.Direction.West)
		{
			transform.localScale = new Vector3(-Mathf.Abs(oldScale.x), oldScale.y, oldScale.z);
		}
	}

	public void RestartEnemy()
	{
		if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
		_currentGridPosition = new Vector2Int(startGridPositionX, startGridPositionY);
		transform.position = _originalPosition;
		transform.localScale = _originalScale;
		_isMoving = false;
		_isDead = false;
		_isInflated = false;
		_inflationTimer = 0;
		_hitByWeaponTimer = 0;
		_isChasingPlayer = false;
		_moveDirection = _originalMoveDirection;
		gameObject.layer = LayerMask.NameToLayer("Enemies");
		ResetFlying();
		CheckIfCanChasePlayer();
	}

	private void UpdateInflation()
	{
		if (_inflationTimer > 0)
		{
			_inflationTimer -= Time.deltaTime;
			if (_inflationTimer <= 0)
			{
				int hitAmount = _animator.GetInteger(HitAmount);
				if (hitAmount > 0)
				{
					_animator.SetInteger(HitAmount, _animator.GetInteger(HitAmount) - 1);
					hitAmount -= 1;
					if (hitAmount == 0)
					{
						_isInflated = false;
						CheckIfCanChasePlayer();
					}
				}
			}
		}
		else
		{
			_inflationTimer = inflationTimerPerStage;
		}
	}

	public void InitializeSpawnedEnemy(int gridStartX, int gridStartY)
	{
		_currentGridPosition = new Vector2Int(gridStartX, gridStartY);
		_flyingTimer = Random.Range(minFlyCooldown, maxFlyCooldown + 1);
		GetRandomDirection();
		_originalMoveDirection = _moveDirection;
		_originalScale = transform.localScale;
		_animator.SetBool(IsSpawning, true);
		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		_isSpawning = true;
	}

	public void FinishSpawn()
	{
		gameObject.layer = LayerMask.NameToLayer("Enemies");
		_isSpawning = false;
		_animator.SetBool(IsSpawning, false);
		CheckIfCanChasePlayer();
	}

	public void SetIsWaitingForFire(bool value)
	{
		_isWaitingToFire = value;
	}

	public bool CanFireNow()
	{
		return !_isInflated && !_isDead && !_isFlying;
	}

	public bool IsSpawningNow()
	{
		return _isSpawning;
	}
#endregion
}