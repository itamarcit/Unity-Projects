using System.Collections;
using UnityEngine;

public class PlayerCollisionDetection : MonoBehaviour
{
	[SerializeField] private PlayersManager playersManager;
	[SerializeField] private float resetTimer = 2f;
	[SerializeField] private Color deathColor;
	[SerializeField] private ScoreManager scoreManager;
	
	private bool _isHitByFallingBlocks = false;
	private bool _isHitByWall = false;
	private SpriteRenderer _renderer;
	private Rigidbody2D _rigidbody;
	private PlayerOneManager _playerManager;
	private Vector3 _startPosition;

	private void Awake()
	{
		_startPosition = transform.position;
	}

	private void Start()
	{
		_renderer = GetComponent<SpriteRenderer>();
		_rigidbody = GetComponent<Rigidbody2D>();
		_playerManager = GetComponent<PlayerOneManager>();
		if (_playerManager == null)
		{
			Debug.LogWarning("PlayerOneManager not found in PlayerCollisionDetection");
		}
	}

	private void OnCollisionEnter2D(Collision2D col)
	{
		if (col.otherCollider.CompareTag("Player One Bottom Collider") &&
			(col.collider.CompareTag("Bounds") || col.collider.CompareTag("BottomBound")))
		{
			_isHitByWall = true;
		}
		else if (col.otherCollider.CompareTag("Player One Top Collider") &&
				 col.collider.CompareTag("Falling Blocks"))
		{
			_isHitByFallingBlocks = true;
			playersManager.IncreaseCoachScore(2, scoreManager.ScoreForHittingPlayer);
		}
	}

	private void OnCollisionExit2D(Collision2D other)
	{
		if (other.otherCollider.CompareTag("Player One Bottom Collider") &&
			(other.collider.CompareTag("Bounds") || other.collider.CompareTag("BottomBound")))
		{
			_isHitByWall = false;
		}
		else if (other.otherCollider.CompareTag("Player One Top Collider") &&
				 other.collider.CompareTag("Falling Blocks"))
		{
			_isHitByFallingBlocks = false;
		}
	}

	private void Update()
	{
		if (_isHitByFallingBlocks && _isHitByWall && !_playerManager.IsDead())
		{
			StartCoroutine(PlayerDeath());
		}
	}

	private IEnumerator PlayerDeath()
	{
		_rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
		_renderer.color = deathColor;
		_playerManager.KillPlayer();
		playersManager.IncreaseCoachScore(2, scoreManager.ScoreForKillingPlayer); // player 2
		yield return new WaitForSeconds(resetTimer);
		ResetPlayer();
	}

	private void ResetPlayer()
	{
		_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
		transform.position = _startPosition;
		_playerManager.RevivePlayer();
		_isHitByWall = false;
		_isHitByFallingBlocks = false;
		_renderer.color = Color.white;
	}
}