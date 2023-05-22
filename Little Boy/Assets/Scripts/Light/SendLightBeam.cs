using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mirror
{
	public class SendLightBeam : MonoBehaviour
	{
		public int maxReflections;
		public bool Caught { get; private set; }

		public enum Direction
		{
			Up,
			Down,
			Right,
			Left
		}

		public Direction startingDirection;
		private Direction _currentDirection;
		private Direction _previousDirection;

		private LineRenderer _lineRenderer;
		private Ray2D _ray;
		private RaycastHit2D _hit;
		private int _ghostMirrorIndex = -1;

		private void Awake()
		{
			_lineRenderer = GetComponent<LineRenderer>();
			Physics2D.alwaysShowColliders = true;
			Physics2D.showColliderContacts = true;
		}

		private void Update()
		{
			CalculateLightBeam();
		}

		private void CalculateLightBeam()
		{
			bool didHitGhostMirror = false;
			MirrorData currentMirror = null;
			_ray = new Ray2D(transform.position, transform.up);
			AddLightSourcePoint();
			_currentDirection = startingDirection;
			_previousDirection = startingDirection;
			for (int i = 0; i < maxReflections; i++)
			{
				_hit = Physics2D.Raycast(_ray.origin, _ray.direction, Mathf.Infinity,
					~(LayerMask.GetMask("NotHitByLight")));
				if (currentMirror != null)
				{
					// name is a bit confusing, in this context currentMirror is the previous mirror.
					currentMirror.EnableMirrorTriggers(); // enable last mirror hit triggers
				}
				if (PlayerBlocksLight())
				{
					// this checks if the player is inside the mirror itself.
					break;
				}
				if (_hit && HitAMirror(_hit))
				{
					currentMirror = HandleMirrorHit(ref didHitGhostMirror);
				}
				else if (_hit && _hit.collider.CompareTag("Enemy"))
				{
					_hit.collider.gameObject.SendMessage("HitByRay");
				}
				else if (_hit)
				{
					// hit something that is not a mirror.
					AddHitPoint();
					Caught = _hit.collider.CompareTag("RayCatcher"); // Did the ray hit a ray catcher
					break;
				}
				else // Didn't hit anything.
				{
					break;
				}
				if (!didHitGhostMirror)
				{
					_ghostMirrorIndex = -1;
				}
			}
		}

		private MirrorData HandleMirrorHit(ref bool didHitGhostMirror)
		{
			MirrorData currentMirror = _hit.collider.transform.parent.GetComponent<MirrorData>();
			_previousDirection = _currentDirection;
			Vector2 reflectDirection =
				currentMirror.GetDirectionFromMirror(_hit.collider.gameObject, _previousDirection,
					out _currentDirection);
			DrawRelevantPointsFromHit(currentMirror, reflectDirection);
			if (_hit.collider.transform.parent.parent.CompareTag("GhostMirror"))
			{
				didHitGhostMirror = true;
				_ghostMirrorIndex = _lineRenderer.positionCount - 1;
				_hit.collider.transform.parent.parent.gameObject.SendMessage("HitByRay");
			}
			currentMirror // so it will not hit the same mirror before it hit something else.
				.DisableMirrorTriggers();
			return currentMirror;
		}

		private void DrawRelevantPointsFromHit(MirrorData currentMirror, Vector2 reflectDirection)
		{
			List<Vector3> pointsToAdd = currentMirror.GetPointsFromMirror(_previousDirection, _currentDirection);
			AddPointsFromList(pointsToAdd);
			if (pointsToAdd.Count > 0)
			{
				_ray = new Ray2D(pointsToAdd.Last(), reflectDirection);
			}
		}

		private bool PlayerBlocksLight()
		{
			if (!_hit) return false;
			RaycastHit2D hitPlayer = Physics2D.Raycast(_ray.origin, _ray.direction, _hit.distance + 0.5f,
				(LayerMask.GetMask("Player")));
			if (hitPlayer)
			{
				AddHitPoint();
				return true;
			}
			return false;
		}

		private bool HitAMirror(RaycastHit2D hit)
		{
			GameObject collidedWith = hit.collider.gameObject;
			return collidedWith.CompareTag("Mirror Trigger Bottom")
			       || collidedWith.CompareTag("Mirror Trigger Left")
			       || collidedWith.CompareTag("Mirror Trigger Right")
			       || collidedWith.CompareTag("Mirror Trigger Top")
			       || collidedWith.CompareTag("GhostMirror");
		}

		private void AddLightSourcePoint()
		{
			_lineRenderer.positionCount = 1;
			_lineRenderer.SetPosition(0, transform.position);
		}

		private void AddHitPoint()
		{
			var positionCount = _lineRenderer.positionCount;
			positionCount += 1;
			_lineRenderer.positionCount = positionCount;
			_lineRenderer.SetPosition(positionCount - 1,
				new Vector3(_hit.point.x, _hit.point.y, -1));
		}

		private void AddPointsFromList(List<Vector3> points)
		{
			foreach (Vector3 point in points)
			{
				int positionCount = _lineRenderer.positionCount;
				positionCount += 1;
				_lineRenderer.positionCount = positionCount;
				_lineRenderer.SetPosition(positionCount - 1, point);
			}
		}

		public List<Vector3> GetPathInfo()
		{
			if (_ghostMirrorIndex == -1)
			{
				return null; // this shouldn't happen
			}
			Vector3[] temp = new Vector3[_lineRenderer.positionCount];
			_lineRenderer.GetPositions(temp);
			return new List<Vector3>(new List<Vector3>(temp).Skip(_ghostMirrorIndex));
		}
	}
}