using System.Collections;
using UnityEngine;

public class PlayerPickUp : MonoBehaviour
{
	[SerializeField] private PlayerOneManager playerOneManager;
	[SerializeField] private ApplesManager applesManager;
	[SerializeField] private float hitMoveDuration = 1f;
	private bool _isBeingSentBackToField = false;
	private FollowPlayer _followPlayerScript;

	private void Start()
	{
		_followPlayerScript = GetComponent<FollowPlayer>();
		if (_followPlayerScript == null)
		{
			Debug.LogWarning("FollowPlayer.cs script missing from a pickup.");
		}
	}

	public bool IsBeingSentBackToField()
	{
		return _isBeingSentBackToField;
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.CompareTag("Player"))
		{
			gameObject.layer = LayerMask.NameToLayer("Player Colliders");
			GameObject toFollow = playerOneManager.GetObjectToFollow(this);
			_followPlayerScript.Init(toFollow);
		}
		if (col.CompareTag("Falling Blocks") && _followPlayerScript.IsFollowing())
		{
			_isBeingSentBackToField = true;
			StopFollowing();
			playerOneManager.RemoveFromFollowingList(gameObject);
			applesManager.GetVectorForAppleAfterHit(out Vector3 randomVectorFarFromPlayer);
			StartCoroutine(MoveToLocation(randomVectorFarFromPlayer));
			applesManager.RemoveColorFromApple(gameObject);
		}
	}

	private IEnumerator MoveToLocation(Vector3 randomVectorInBoundingBox)
	{
		float timeElapsed = 0;
		Vector3 startPos = transform.position;
		while (timeElapsed <= hitMoveDuration)
		{
			float interpolationFactor = timeElapsed / hitMoveDuration;
			transform.position = Vector3.Lerp(startPos, randomVectorInBoundingBox, interpolationFactor);
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		applesManager.RemoveAppleTargetPos(randomVectorInBoundingBox);
		transform.position = randomVectorInBoundingBox;
		gameObject.layer = LayerMask.NameToLayer("Player Triggers");
		gameObject.tag = "Apple";
		_isBeingSentBackToField = false;
	}

	public void StopFollowing()
	{
		_followPlayerScript.StopFollowing();
	}

	public void ChangeObjectToFollow(GameObject toFollow)
	{
		_followPlayerScript.ChangeObjectToFollow(toFollow);
	}

	public GameObject GetFollower()
	{
		return _followPlayerScript.GetFollower();
	}

	public bool IsFollowing()
	{
		return _followPlayerScript.IsFollowing();
	}
}