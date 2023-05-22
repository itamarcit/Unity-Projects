using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
	[SerializeField] private Transform player;
	[SerializeField] private float speed = 3f;
	[SerializeField] private float minDistance = 1f;

	private bool _init = false;
	private bool _initMovement = false;
	private GameObject _toFollow;
	private Vector3 _initTarget;
	private float _lowerSpeedMultiplier;
	private bool _stoppedMoving = false;

	public void Init(GameObject toFollow)
	{
		_toFollow = toFollow;
		_initTarget = transform.position + (toFollow.transform.position - transform.position).normalized;
		_init = true;
		_initMovement = true;
	}

	public void ChangeObjectToFollow(GameObject toFollow)
	{
		_toFollow = toFollow;
	}

	public void StopFollowing()
	{
		_init = false;
		_initMovement = false;
		_toFollow = null;
	}

	public bool IsFollowing()
	{
		return _init;
	}

	private void FixedUpdate()
	{
		if (_initMovement)
		{ // initial movement to go to the back of the queue
			if (Vector3.Distance(transform.position, _initTarget) > 0.1f)
			{
				transform.position = Vector3.MoveTowards(transform.position, _initTarget, Time.deltaTime * speed);
			}
			else
			{
				_initMovement = false;
			}
		}
		else if (_init)
		{
			float distanceToTarget = Vector3.Distance(_toFollow.transform.position, transform.position);
			if (distanceToTarget > minDistance)
			{
				_stoppedMoving = false;
				transform.position =
					Vector3.Lerp(transform.position, _toFollow.transform.position, speed * Time.deltaTime);
			}
			else if (distanceToTarget <= minDistance)
			{
				if (!_stoppedMoving)
				{
					_stoppedMoving = true;
					_lowerSpeedMultiplier = speed;
				}
				transform.position =
					Vector3.Lerp(transform.position, _toFollow.transform.position, _lowerSpeedMultiplier * Time.deltaTime);
				_lowerSpeedMultiplier = _lowerSpeedMultiplier >= 0
					? _lowerSpeedMultiplier - speed / 100f
					: _lowerSpeedMultiplier = 0;
			}
		}
	}

	public GameObject GetFollower()
	{
		return _toFollow;
	}
}