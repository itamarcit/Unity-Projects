using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MirrorData : MonoBehaviour
{
	private const string MIRROR_LEFT_DOWN = "Mirror Left Down";
	private const string MIRROR_LEFT_UP = "Mirror Left Up";
	private const string MIRROR_RIGHT_UP = "Mirror Right Up";
	private const string TRANSFORM_LISTS_TOOLTIP_MSG = "The transforms will be added as points for the ray. " +
													   "Always look at them as coming " +
													   "from top/bottom and going towards left/right";
	private const string MIRROR_TRIGGER_LEFT = "Mirror Trigger Left";
	private const string MIRROR_TRIGGER_RIGHT = "Mirror Trigger Right";

	[Tooltip(TRANSFORM_LISTS_TOOLTIP_MSG)]
	[SerializeField] private List<Transform> goThroughLeftToUp = new();
	[Tooltip(TRANSFORM_LISTS_TOOLTIP_MSG)]
	[SerializeField] private List<Transform> goThroughRightToUp = new();
	[Tooltip(TRANSFORM_LISTS_TOOLTIP_MSG)]
	[SerializeField] private List<Transform> goThroughLeftToDown = new();
	[Tooltip(TRANSFORM_LISTS_TOOLTIP_MSG)]
	[SerializeField] private List<Transform> goThroughRightToDown = new();
	[Tooltip("Includes all of the mirror triggers so the light won't hit the same mirror twice.")]
	[SerializeField] private List<GameObject> triggers;
	private DateTime _sinceDisabled;

	private void Awake()
	{
		_sinceDisabled = DateTime.Now;
	}

	private void Update()
	{
		CheckForInactiveTriggers();
	}
	
	private void CheckForInactiveTriggers()
	{
		if (triggers[0].activeSelf)
		{
			_sinceDisabled = DateTime.Now;
		}
		if ((DateTime.Now - _sinceDisabled).Seconds > 0.5f)
		{
			EnableMirrorTriggers();
		}
	}

	public List<Vector3> GetPointsFromMirror(SendLightBeam.Direction previousDirection, SendLightBeam.Direction currentDirection)
	{
		List<Vector3> result = new();
		List<Transform> correctList = GetCorrectList(previousDirection, currentDirection);
		foreach (Transform pointTransform in correctList)
		{
			result.Add(new Vector3(pointTransform.position.x, pointTransform.position.y, -1));
		}
		return result;
	}

	private List<Transform> GetCorrectList(SendLightBeam.Direction previousDirection, SendLightBeam.Direction currentDirection)
	{
		List<Transform> correctList;
		bool shouldReverse = false;
		if (previousDirection == SendLightBeam.Direction.Down && currentDirection == SendLightBeam.Direction.Left)
		{
			correctList = goThroughLeftToUp;
		}
		else if (previousDirection == SendLightBeam.Direction.Left && currentDirection == SendLightBeam.Direction.Down)
		{
			correctList = goThroughRightToDown;
			shouldReverse = true;
		}
		else if (previousDirection == SendLightBeam.Direction.Down && currentDirection == SendLightBeam.Direction.Right)
		{
			correctList = goThroughRightToUp;
		}
		else if (previousDirection == SendLightBeam.Direction.Right && currentDirection == SendLightBeam.Direction.Down)
		{
			correctList = goThroughLeftToDown;
			shouldReverse = true;
		}
		else if (previousDirection == SendLightBeam.Direction.Up && currentDirection == SendLightBeam.Direction.Right)
		{
			correctList = goThroughRightToDown;
		}
		else if (previousDirection == SendLightBeam.Direction.Right && currentDirection == SendLightBeam.Direction.Up)
		{
			correctList = goThroughLeftToUp;
			shouldReverse = true;
		}
		else if (previousDirection == SendLightBeam.Direction.Up && currentDirection == SendLightBeam.Direction.Left)
		{
			correctList = goThroughLeftToDown;
		}
		else if (previousDirection == SendLightBeam.Direction.Left && currentDirection == SendLightBeam.Direction.Up)
		{
			correctList = goThroughRightToUp;
			shouldReverse = true;
		}
		else
		{
			var t =  new List<Transform>();
			t.Add(transform);
			return t;
		}
		if (shouldReverse)
		{
			correctList = new List<Transform>(correctList);
			correctList.Reverse();
		}
		return correctList;
	}

	public Vector2 GetDirectionFromMirror(GameObject collidedObject)
	{
		if (gameObject.CompareTag(MIRROR_LEFT_DOWN))
		{
			return collidedObject.CompareTag(MIRROR_TRIGGER_LEFT) ? Vector2.down :
				// trigger must be down
				Vector2.left;
		}
		if (gameObject.CompareTag(MIRROR_LEFT_UP))
		{
			return collidedObject.CompareTag(MIRROR_TRIGGER_LEFT) ? Vector2.up :
				// trigger must be up
				Vector2.left;
		}
		if (gameObject.CompareTag(MIRROR_RIGHT_UP))
		{
			return collidedObject.CompareTag(MIRROR_TRIGGER_RIGHT) ? Vector2.up :
				// trigger must be up
				Vector2.right;
		}
		// mirror is RIGHT DOWN
		return collidedObject.CompareTag(MIRROR_TRIGGER_RIGHT) ? Vector2.down :
			// trigger must be down
			Vector2.right;
	}

	public void DisableMirrorTriggers()
	{
		foreach (GameObject trigger in triggers)
		{
			trigger.SetActive(false);
		}
	}

	public void EnableMirrorTriggers()
	{
		foreach (GameObject trigger in triggers)
		{
			trigger.SetActive(true);
		}
	}


	public Vector2 GetDirectionFromMirror(GameObject collidedObject, SendLightBeam.Direction startingDirection,
										  out SendLightBeam.Direction currentDirection)
	{
		if (gameObject.CompareTag(MIRROR_LEFT_DOWN))
		{
			if (startingDirection == SendLightBeam.Direction.Up)
			{
				currentDirection = SendLightBeam.Direction.Left;
				return Vector2.left;
			}
			else // incoming direction is right
			{
				currentDirection = SendLightBeam.Direction.Down;
				return Vector2.down;
			}
		}
		if (gameObject.CompareTag(MIRROR_LEFT_UP))
		{
			if (startingDirection == SendLightBeam.Direction.Down)
			{
				currentDirection = SendLightBeam.Direction.Left;
				return Vector2.left;
			}
			else // incoming direction is right
			{
				currentDirection = SendLightBeam.Direction.Up;
				return Vector2.up;
			}
		}
		if (gameObject.CompareTag(MIRROR_RIGHT_UP))
		{
			if (startingDirection == SendLightBeam.Direction.Down)
			{
				currentDirection = SendLightBeam.Direction.Right;
				return Vector2.right;
			}
			else // incoming direction is left
			{
				currentDirection = SendLightBeam.Direction.Up;
				return Vector2.up;
			}
		}
		// mirror is RIGHT DOWN
		if (startingDirection == SendLightBeam.Direction.Up)
		{
			currentDirection = SendLightBeam.Direction.Right;
			return Vector2.right;
		}
		else // incoming direction is left
		{
			currentDirection = SendLightBeam.Direction.Down;
			return Vector2.down;
		}
	}
}