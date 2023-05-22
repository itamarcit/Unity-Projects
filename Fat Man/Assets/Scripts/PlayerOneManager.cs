using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOneManager : MonoBehaviour
{

	[SerializeField] private PlayersManager playersManager;
	
	[SerializeField] private GameObject mouth;
	[SerializeField] private ScoreManager scoreManager;
	[SerializeField] private float animSpeedNoFollowers = 1f;
	[SerializeField] private float animSpeedOneFollower = 0.9f;
	[SerializeField] private float animSpeedTwoFollowers = 0.8f;
	[SerializeField] private float animSpeedThreeFollowers = 0.7f;
	[SerializeField] private float animSpeedFourFollowers = 0.6f;
	private Animator _movementAnimator;
	private bool _isDead = false;
	private readonly List<PlayerPickUp> _followsPlayer = new();
	private PlayerUnrestrictedMovement _playerMovement;
	private Rigidbody2D _rigidbody2D;
	private bool _isEating;
	private bool _isFrozen;
	private Vector3 _initialPosition;
	private static readonly int Direction = Animator.StringToHash("Direction");
	private const float COOLDOWN_BEFORE_SENDING_PLAYER_AFTER_RETURNED_ITEMS = 0.75f;
	public const int LEFT_DIRECTION = 0;
	public const int RIGHT_DIRECTION = 1;
	public const int DOWN_DIRECTION = 2;
	public const int UP_DIRECTION = 3;
	private const int ZERO_FOLLOWERS = 0;
	private const int ONE_FOLLOWER = 1;
	private const int TWO_FOLLOWERS = 2;
	private const int THREE_FOLLOWERS = 3;
	private const int FOUR_FOLLOWERS = 4;
	
	

	private void Update()
	{
		switch (_followsPlayer.Count)
		{
			case ZERO_FOLLOWERS:
				_movementAnimator.speed = animSpeedNoFollowers;
				break;
			case ONE_FOLLOWER:
				_movementAnimator.speed = animSpeedOneFollower;
				break;
			case TWO_FOLLOWERS:
				_movementAnimator.speed = animSpeedTwoFollowers;
				break;
			case THREE_FOLLOWERS:
				_movementAnimator.speed = animSpeedThreeFollowers;
				break;
			case FOUR_FOLLOWERS:
				_movementAnimator.speed = animSpeedFourFollowers;
				break;
		}
	}

	private void Awake()
	{
		_movementAnimator = GetComponent<Animator>();
		_playerMovement = GetComponent<PlayerUnrestrictedMovement>();
		_rigidbody2D = GetComponent<Rigidbody2D>();
		_initialPosition = gameObject.transform.position;
	}

	public void KillPlayer()
	{
		_isDead = true;
		ResetPlayer(false);
	}

	public void RevivePlayer()
	{
		_isDead = false;
	}

	public bool IsDead()
	{
		return _isDead;
	}

	public bool IsFrozen()
	{
		return _isFrozen;
	}

	/**
	 * Gets the next follower number and increments the counter for the next call.
	 * Should only be called once per game object.
	 */
	public GameObject GetObjectToFollow(PlayerPickUp pickupToAdd)
	{
		_followsPlayer.Add(pickupToAdd);
		return _followsPlayer.Count == 1
			? gameObject                     // The player itself if count is 1.
			: _followsPlayer[^2].gameObject; // Get the last apple that follows the player.
	}

	/**
	 * Eats the following snacks
	 */
	public IEnumerator EatFollowers()
	{
		if (_followsPlayer.Count == 0 || _isEating) yield break;
		_isEating = true;
		foreach (PlayerPickUp objectToMove in _followsPlayer)
		{
			StartCoroutine(MoveToMouthAndDisappear(objectToMove));
		}
		playersManager.IncreaseFatmanScore(1, GetCorrectScoreFromAmountFollowers(_followsPlayer.Count));
		_followsPlayer.Clear();
		_isEating = false;
		yield return new WaitForSeconds(COOLDOWN_BEFORE_SENDING_PLAYER_AFTER_RETURNED_ITEMS);
		GameManager.Shared.ResetSentVegetables();
		ResetPlayer(true);
	}

	private IEnumerator MoveToMouthAndDisappear(PlayerPickUp pickup)
	{
		float timer = 0, lerpAlpha;
		Vector3 startPos = pickup.transform.position;
		pickup.StopFollowing();
		while (timer < 1f)
		{
			timer += Time.deltaTime * 2f;
			lerpAlpha = Mathf.Min(timer, 1f);
			pickup.transform.position = Vector3.Lerp(startPos, mouth.transform.position, lerpAlpha);
			yield return null;
		}
		timer = 0;
		while (timer < 1f)
		{
			timer += Time.deltaTime * 4f;
			yield return null;
		}
		pickup.gameObject.SetActive(false);
	}

	private int GetCorrectScoreFromAmountFollowers(int followers)
	{
		return followers switch
		{
			1 => scoreManager.ScoreForOneJunkFood,
			2 => scoreManager.ScoreForTwoJunkFood,
			3 => scoreManager.ScoreForThreeJunkFood,
			4 => scoreManager.ScoreForFourJunkFood,
			_ => 0
		};
	}

	public void ResetPlayer(bool moveToStart)
	{
		_playerMovement.ResetMovement();
		if (moveToStart)
		{
			StartCoroutine(MovePlayerToStart());
		}
	}

	private IEnumerator MovePlayerToStart()
	{
		_isFrozen = true;
		_rigidbody2D.simulated = false;
		float lerpAlpha = 0;
		Vector3 startPos = transform.position;
		while (lerpAlpha < 1)
		{
			lerpAlpha += Mathf.Min(Time.deltaTime, 1);
			transform.position = Vector3.Lerp(startPos, _initialPosition, lerpAlpha);
			yield return null;
		}
		_rigidbody2D.simulated = true;
		_isFrozen = false;
	}

	public void RemoveFromFollowingList(GameObject objToMove)
	{
		int index = GetListIndexOfFollowingObject(objToMove);
		if (index == -1)
		{
			return;
		}
		if (index + 1 < _followsPlayer.Count) // change the following object of the one behind the removed object
		{
			_followsPlayer[index + 1]
				.ChangeObjectToFollow(index - 1 >= 0
					? _followsPlayer[index - 1].gameObject
					: gameObject);
		}
		_followsPlayer.RemoveAt(index);
	}

	/// <summary>
	/// Gets the index of gameObject in the list _followsPlayer.
	/// </summary>
	/// <param name="followingObject">object to find in _followsPlayer</param>
	/// <returns>-1 if doesn't exist, index otherwise</returns>
	private int GetListIndexOfFollowingObject(GameObject followingObject)
	{
		if (followingObject == null) return -1;
		int index = 0;
		foreach (PlayerPickUp pickUp in _followsPlayer)
		{
			if (pickUp == null) return -1;
			else if (pickUp.gameObject == followingObject)
			{
				return index;
			}
			index++;
		}
		return -1; // didn't find.
	}

	public int NumberOfFollowers()
	{
		return _followsPlayer.Count;
	}

	public void SetAnimationDirection(int direction)
	{
		switch (direction)
		{
			case LEFT_DIRECTION:
				_movementAnimator.SetInteger(Direction, LEFT_DIRECTION);
				break;
			case RIGHT_DIRECTION:
				_movementAnimator.SetInteger(Direction, RIGHT_DIRECTION);
				break;
			case DOWN_DIRECTION:
				_movementAnimator.SetInteger(Direction, DOWN_DIRECTION);
				break;
			case UP_DIRECTION:
				_movementAnimator.SetInteger(Direction, UP_DIRECTION);
				break;
		} 
	}
}