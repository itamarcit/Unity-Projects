using System.Collections;
using UnityEngine;

public class RockScript : MonoBehaviour
{
#region Fields
	[SerializeField] private Vector2Int gridBottomLocation;
	[SerializeField] private float movementTotalTime;
	[SerializeField] private float delayFall = 3f;
	[SerializeField] private float delayDestroyRock = 4f;
	[SerializeField] private Sprite waitingToFallRock;
	[SerializeField] private Sprite fallingRock;
	[SerializeField] private Sprite shatteredRock;
	[SerializeField] private float powerRockSpeed = 10;
	private Sprite _fullRock;
	private SpriteRenderer _renderer;
	private BoxCollider2D _collider;
	private Rigidbody2D _rigidbody;
	private Vector3 _origPosition;
	private Vector3 _targetPosition;
	private float _movementElapsedTime;
	private bool _canFall;
	private bool _startedFallingSequence;
	private bool _isFalling;
	private int _fallIterations = 0;
	private float _tiltRockTimer;
	private bool _isSpawning;
	private bool _isPowerUpUsed;
	private bool _startedDownMovement;
	private Coroutine _fallingCoroutine;
	private const float SECONDS_BETWEEN_EACH_SPRITE = 0.5f;
#endregion

#region Events
	private void Start()
	{
		GridManager.Shared.RegisterRock(gridBottomLocation.x, gridBottomLocation.y);
	}

	private void Awake()
	{
		_renderer = GetComponent<SpriteRenderer>();
		_collider = GetComponent<BoxCollider2D>();
		_rigidbody = GetComponent<Rigidbody2D>();
		_fullRock = _renderer.sprite;
	}

	private void Update()
	{
		if (_isSpawning || _isPowerUpUsed) return;
		_canFall = CanFall(true);
		if (_canFall && !_startedFallingSequence)
		{
			_startedFallingSequence = true;
			_fallingCoroutine = StartCoroutine(DropRock());
		}
	}

	private void OnCollisionEnter2D(Collision2D col)
	{
		if (_isFalling && col.gameObject.CompareTag("Player"))
		{
			GameManager.Shared.KillPlayerByRock();
		}
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (_isFalling && col.gameObject.CompareTag("Enemy"))
		{
			EnemyController controller = col.gameObject.GetComponent<EnemyController>();
			if (controller != null && !controller.IsSpawningNow())
			{
				GameManager.Shared.KillEnemyByRock(col.gameObject);
			}
		}
	}
#endregion
#region Methods
	private bool CanFall(bool isFirstFall)
	{
		if (gridBottomLocation.y - _fallIterations < 0)
		{
			return false;
		}
		if (isFirstFall)
		{
			if (!GridManager.Shared.IsGridSquareDug(gridBottomLocation.x, gridBottomLocation.y - 1,
				    new[]
				    {
					    GridManager.Direction.East, GridManager.Direction.West, GridManager.Direction.South
				    }))
			{
				return false;
			}
		}
		else
		{
			if (!GridManager.Shared.IsGridSquareDugNorth(gridBottomLocation.x,
				    gridBottomLocation.y - 1 - _fallIterations))
			{
				return false;
			}
		}
		return true;
	}

	private IEnumerator TiltRock()
	{
		_tiltRockTimer = 0;
		while (_tiltRockTimer < delayFall)
		{
			_renderer.sprite = waitingToFallRock;
			yield return new WaitForSeconds(SECONDS_BETWEEN_EACH_SPRITE);
			_renderer.sprite = _fullRock;
			yield return new WaitForSeconds(SECONDS_BETWEEN_EACH_SPRITE);
			_tiltRockTimer -= Time.deltaTime;
			_tiltRockTimer += 1f;
		}
	}

	private IEnumerator DropRock()
	{
		if (_fallIterations == 0)
		{
			yield return StartCoroutine(TiltRock());
			_renderer.sprite = waitingToFallRock;
			_isFalling = true;
			GridManager.Shared.DeregisterRock(gridBottomLocation.x, gridBottomLocation.y);
			_fallingCoroutine = null;
		}
		_origPosition = transform.position;
		_targetPosition = _origPosition - new Vector3(0, GridManager.Shared.GetGridSpacingY(), 0);
		_movementElapsedTime = 0;
		while (_movementElapsedTime < movementTotalTime)
		{
			transform.position =
				Vector3.Lerp(_origPosition, _targetPosition, (_movementElapsedTime / movementTotalTime));
			_movementElapsedTime += Time.deltaTime;
			yield return null;
		}
		transform.position = _targetPosition;
		_fallIterations += 1;
		if (CanFall(false))
		{
			StartCoroutine(DropRock());
		}
		else
		{
			yield return ShatterRock();
		}
	}

	public void SpawnRock(Vector2Int rockLoc)
	{
		_isSpawning = true;
		gridBottomLocation = rockLoc;
	}

	public void FinishSpawn()
	{
		_isSpawning = false;
	}

	public void StartPowerUpMovement(Vector2Int direction)
	{
		if (_isFalling)
		{
			return;
		}
		if (_fallingCoroutine != null)
		{
			StopCoroutine(_fallingCoroutine);
		}
		GridManager.Shared.DeregisterRock(gridBottomLocation.x, gridBottomLocation.y);
		_isPowerUpUsed = true;
		_isFalling = true;
		Vector2Int targetGridPos = Vector2Int.zero;
		Vector2Int newGridPos = gridBottomLocation + direction;
		if (GridManager.Shared.IsGridSquareDug(newGridPos.x, newGridPos.y,
			    new[]
			    {
				    GridManager.Direction.South, GridManager.Direction.East, GridManager.Direction.North,
				    GridManager.Direction.West
			    }))
		{
			targetGridPos = newGridPos;
		}
		else
		{
			newGridPos = gridBottomLocation + Vector2Int.down;
			if (GridManager.Shared.IsGridSquareDug(newGridPos.x, newGridPos.y,
				    new[]
				    {
					    GridManager.Direction.North, GridManager.Direction.South, GridManager.Direction.East,
					    GridManager.Direction.West
				    })) // if the block below the rock is dug in any direction
			{
				targetGridPos = newGridPos;
				_startedDownMovement = true;
			}
		}
		if (targetGridPos == Vector2Int.zero)
		{
			StartCoroutine(ShatterRock());
		}
		else
		{
			StartCoroutine(PowerRockMovement(newGridPos, gridBottomLocation));
		}
	}

	private IEnumerator PowerRockDirection(Vector2Int direction)
	{
		Vector2Int targetGridPos = Vector2Int.zero;
		Vector2Int newGridPos = gridBottomLocation + direction;
		if (!_startedDownMovement && GridManager.Shared.IsGridSquareDug(newGridPos.x, newGridPos.y,
			    GridManager.Shared.GetOppositeDirection(direction)))
		{
			targetGridPos = newGridPos;
		}
		else
		{
			newGridPos = gridBottomLocation + Vector2Int.down;
			if (GridManager.Shared.IsGridSquareDug(newGridPos.x, newGridPos.y, GridManager.Direction.North))
			{
				// checks if the lower square is dug.
				_startedDownMovement = true;
				targetGridPos = newGridPos;
			}
		}
		if (targetGridPos == Vector2Int.zero)
		{
			yield return ShatterRock();
		}
		else
		{
			StartCoroutine(PowerRockMovement(targetGridPos, gridBottomLocation));
		}
	}

	private IEnumerator PowerRockMovement(Vector2Int targetGridPos, Vector2Int originalGridPos)
	{
		Vector3 targetWorldPos = GridManager.Shared.GetWorldPosition(targetGridPos.x, targetGridPos.y);
		while (Vector3.Distance(_rigidbody.position, targetWorldPos) >= 0.01f)
		{
			_rigidbody.MovePosition(Vector2.MoveTowards(new Vector2(transform.position.x, transform.position.y),
				new Vector2(targetWorldPos.x, targetWorldPos.y), Time.deltaTime * powerRockSpeed));
			yield return null;
		}
		transform.position = targetWorldPos;
		gridBottomLocation = targetGridPos;
		StartCoroutine(PowerRockDirection(targetGridPos - originalGridPos));
	}

	private IEnumerator ShatterRock()
	{
		_renderer.sprite = fallingRock;
		_collider.size = new Vector2(0, 0);
		yield return new WaitForSeconds(delayDestroyRock / 2);
		_renderer.sprite = shatteredRock;
		yield return new WaitForSeconds(delayDestroyRock / 2);
		Destroy(gameObject);
	}

	public SpriteRenderer GetRenderer()
	{
		return _renderer;
	}

	public Vector2Int GetGridLocation()
	{
		return gridBottomLocation;
	}
#endregion
}