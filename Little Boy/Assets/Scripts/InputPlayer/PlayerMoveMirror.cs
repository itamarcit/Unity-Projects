using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerMoveMirror : MonoBehaviour
{
	[SerializeField] private AudioSource[] _pickUpSounds;
	[SerializeField] private AudioSource[] _putDownSounds;

	private GameObject someObject = null;
	private bool holdSomthing = false;

	List<Collider2D> relevantColliders = new();

	private Rigidbody2D myRigidbody;
	private SpriteRenderer mySpriteRenderer;
	private PolygonCollider2D myCollider;
	private SpriteRenderer objectRenderer;
	private float positionX;
	private float positionY;

	private float objectGap = 8f;
	private Vector2 vectorCheck;
	private float distanceDiffX;
	private float distanceDiffY;
	private const float diff = 0.3f;
	private bool downFlag = false;
	private Player _player;
	// private bool inMoveableTriggerFlag = false;

	//Player Sprites
	[SerializeField] private Sprite[] playerSprites;

	private void Start()
	{
		myRigidbody = GetComponent<Rigidbody2D>();
		mySpriteRenderer = GetComponent<SpriteRenderer>();
		myCollider = GetComponent<PolygonCollider2D>();
		_player = GetComponent<Player>();
	}

	void Update()
	{
		LiftUnliftObjects();
		if (someObject != null && holdSomthing)
		{
			MoveMirrorRightDirection();
		}
		CheckForObjectsNearPlayer();
	}

	private void CheckForObjectsNearPlayer()
	{
		relevantColliders.Clear();
		Physics2D.queriesHitTriggers = true;
		Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, 0.1f);
		Physics2D.queriesHitTriggers = false;
		foreach (Collider2D result in results)
		{
			if (result.gameObject.CompareTag("Movable") || result.gameObject.CompareTag("MirrorFrame"))
			{
				relevantColliders.Add(result);
			}
		}
	}

	private void LiftUnliftObjects()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
		    float minDistance = Single.PositiveInfinity;
		    Collider2D closestCollider = null;
		    foreach (Collider2D relevantCollider in relevantColliders)
		    {
		        float currDistance = Vector3.Distance(transform.position, relevantCollider.transform.position);
		        if (currDistance < minDistance)
		        {
		            minDistance = currDistance;
		            closestCollider = relevantCollider;
		        }
		    }
		    if (closestCollider == null) return;
		    if (holdSomthing == false)
		    {
		        _pickUpSounds[Random.Range(0, 2)].Play();
		        holdSomthing = true;
		        closestCollider.gameObject.transform.parent = gameObject.transform;
		    }
		    else
		    {
			    _putDownSounds[Random.Range(0, 2)].Play();
			    holdSomthing = false;
			    closestCollider.gameObject.transform.parent = gameObject.transform.parent;
		    }
		    someObject = closestCollider.gameObject;
		    objectRenderer = closestCollider.GetComponent<SpriteRenderer>();
		}
		// if (Input.GetKeyDown(KeyCode.Space) && someObject != null && inMoveableTriggerFlag)
		// {
		// 	if (holdSomthing == false)
		// 	{
		// 		_pickUpSounds[(int)Random.Range(0, 2)].Play();
		// 		holdSomthing = true;
		// 		someObject.transform.parent = gameObject.transform;
		// 	}
		// 	else
		// 	{
				// _putDownSounds[(int)Random.Range(0, 2)].Play();
				// holdSomthing = false;
				// someObject.transform.parent = gameObject.transform.parent;
		// 	}
		// }
	}

	private void MoveMirrorRightDirection()
	{
		Vector2 input = _player.GetInput();
		if (input.x < 0 && IfDistanceBigEnough("Left")) // Go Left
		{
			someObject.transform.localPosition = new Vector3(-objectGap, 0, 0);
			downFlag = false;
		}
		else if (input.x > 0 && IfDistanceBigEnough("Right")) // Go Right
		{
			someObject.transform.localPosition = new Vector3(objectGap, 0, 0);
			downFlag = false;
		}
		else if (input.y > 0 && IfDistanceBigEnough("Up")) // Go Up
		{
			someObject.transform.localPosition = new Vector3(0, objectGap, 0);
			downFlag = false;
		}
		if (input.y < 0 && IfDistanceBigEnough("Down")) // Go Down
		{
			someObject.transform.localPosition = new Vector3(0, -objectGap + 0.1f, 0);
			objectRenderer.sortingOrder = 2;
			downFlag = true;
		}
		if (!downFlag)
		{
			objectRenderer.sortingOrder = 0;
		}
	}

	private void MovePlayerSprite()
	{
		Vector2 input = _player.GetInput();
		if (input.x < 0) // Go Left
		{
			mySpriteRenderer.sprite = playerSprites[1];
		}
		if (input.x > 0) // Go Right
		{
			mySpriteRenderer.sprite = playerSprites[3];
		}
		if (input.y < 0) // Go Down
		{
			mySpriteRenderer.sprite = playerSprites[0];
		}
		if (input.y > 0) // Go Up
		{
			mySpriteRenderer.sprite = playerSprites[2];
		}
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		// if ((col.transform.CompareTag("MirrorFrame") || col.transform.CompareTag("Movable")) && !holdSomthing)
		// {
		// 	someObject = col.gameObject;
		// 	objectRenderer = col.transform.GetComponent<SpriteRenderer>();
		// }
	}

	// private void OnTriggerStay2D(Collider2D other)
	// {
	// 	inMoveableTriggerFlag = true;
	// }

	private void OnTriggerExit2D(Collider2D other)
	{
		// if (holdSomthing == false)
		// {
		// 	someObject = null;
		// }
	}

	private bool IfDistanceBigEnough(string direction)
	{
		switch (direction)
		{
			case "Right":
				vectorCheck = Vector2.right;
				distanceDiffX = diff;
				distanceDiffY = 0;
				break;
			case "Left":
				vectorCheck = Vector2.left;
				distanceDiffX = -diff;
				distanceDiffY = 0;
				break;
			case "Up":
				vectorCheck = Vector2.up;
				distanceDiffX = 0;
				distanceDiffY = diff;
				break;
			case "Down":
				vectorCheck = Vector2.down;
				distanceDiffX = 0;
				distanceDiffY = -diff;
				break;
		}
		RaycastHit2D hitInfo =
			Physics2D.Raycast(
				new Vector2(myRigidbody.transform.position.x + distanceDiffX,
					myRigidbody.transform.position.y + distanceDiffY),
				vectorCheck);
		if (!hitInfo || Vector2.Distance(hitInfo.transform.position, myRigidbody.transform.position) > 1.5f)
		{
			return true;
		}
		return false;
	}

	public bool IsMovingAnItem()
	{
		return holdSomthing;
	}
}