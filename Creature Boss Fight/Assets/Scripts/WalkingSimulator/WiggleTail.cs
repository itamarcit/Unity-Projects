using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WalkingSimulator;

namespace ItamarRamon
{
	public class WiggleTail : MonoBehaviour
	{
		[SerializeField] private float tailMovementSpeedIdle = 1f;
		[SerializeField] private float tailMovementSpeedPreparingAttack = 2f;
		[SerializeField] private Transform tailTarget;
		[SerializeField] private AnimationCurve zOffsetCurve;
		[SerializeField] private float rotationSpeed = 10f;
		[SerializeField] private float moveForwardFactor = 1f;
		[SerializeField] private TailMovementData[] attackingSequence;
		private bool isGoingLeft = true;
		private const float RIGHT_TAIL_POS_X = 2.662f; // These constant floats are of the tail in idle
		private const float LEFT_TAIL_POS_X = -2.662f; // state, when he's moving from side to side.
		private const float TAIL_HEIGHT = 4f; // They are all in local position.
		private const float TAIL_MIDDLE_Z = 5.67625f; 
		private const int LEFT = 0;
		private const int RIGHT = 1;
		private const float EPSILON = .1f;

		private readonly List<Vector3> tailPositionsInLastMovement = new();
		private Vector3 preparationTargetLocalPos;
		public static TailMode tailMode;
		private Keyboard keyboard;

		private int attackingSequenceIndex; // Remembers the index of the attacking sequence currently in.
		private int reloadModeTargetIndex;  // Remembers the index of the reload target pos (list) currently in.

		private int attackDirection; // Remembers if the player attacked left or right

		private bool isPaused = true;

		public enum TailMode
		{
			Idle,
			ReloadingAttack,
			PreparingAttack,
			Attacking
		}
		
		private void Awake()
		{
			keyboard = Keyboard.current;
			tailMode = TailMode.Idle;
			float prepTargetYValue = 0;
			foreach (TailMovementData data in attackingSequence)
			{
				if (data.movementDir == TailMovementData.MovementDir.Y)
				{
					prepTargetYValue = data.valueLeft;
				}
			}
			preparationTargetLocalPos = new Vector3(0, prepTargetYValue, TAIL_MIDDLE_Z);
		}

		public void PauseTailAttack(bool toPause)
		{
			isPaused = toPause;
		}
		
		
		private void Update()
		{
			CheckForInput();
			switch (tailMode)
			{
				case TailMode.Idle:
					MoveFromSideToSide();
					break;
				case TailMode.ReloadingAttack:
					PerformReload();
					RotateTail(Vector3.down);
					break;
				case TailMode.PreparingAttack:
					MoveTailTowardsPos(preparationTargetLocalPos);
					RotateTail(Vector3.up);
					break;
				case TailMode.Attacking:
					PerformTailAttack();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void PerformReload()
		{
			if (IsReloadingFinished()) return;
			MoveTailTowardsPos(tailPositionsInLastMovement[reloadModeTargetIndex]);
		}

		private void PerformTailAttack()
		{
			if (IsAttackingFinished()) return;
			TailMovementData currentSequence = attackingSequence[attackingSequenceIndex];
			if (currentSequence.isSkippable && keyboard.spaceKey.wasPressedThisFrame)
			{
				tailPositionsInLastMovement.Add(tailTarget.localPosition);
				attackingSequenceIndex++;
				return;
			}
			Vector3 targetPos = currentSequence.GetTargetVector(tailTarget.localPosition, attackDirection == LEFT);
			MoveTailTowardsPos(targetPos);
		}

		private bool IsAttackingFinished()
		{
			if (attackingSequenceIndex != attackingSequence.Length) return false;
			attackingSequenceIndex = 0;
			tailPositionsInLastMovement.Reverse();
			tailMode = TailMode.ReloadingAttack;
			return true;
		}

		private bool IsReloadingFinished()
		{
			if (reloadModeTargetIndex != tailPositionsInLastMovement.Count) return false;
			reloadModeTargetIndex = 0;
			tailPositionsInLastMovement.Clear();
			tailMode = TailMode.Idle;
			return true;
		}

		/// <summary>
		/// Moves the tail to its original position. When done, switches to attack mode or to idle mode.
		/// </summary>
		private void MoveTailTowardsPos(Vector3 targetPoint)
		{
			Vector3 startPoint = tailTarget.localPosition;
			float distanceRemaining = Vector3.Distance(startPoint, targetPoint);
			float distanceThisFrame = GetCurrentTailSpeed() * Time.deltaTime;
			if(distanceRemaining < distanceThisFrame)
			{
				tailTarget.localPosition = targetPoint;
				SwitchTailMode(); // Finished Movement. Move to next mode (or update the mode we're in).
				return;
			}
			tailTarget.localPosition = Vector3.MoveTowards(startPoint, targetPoint, distanceThisFrame);
		}

		private float GetCurrentTailSpeed() // Will return 0 if tail mode is idle
		{
			switch (tailMode)
			{
				case TailMode.ReloadingAttack:
				case TailMode.PreparingAttack:
					return tailMovementSpeedPreparingAttack;
				case TailMode.Attacking:
					return attackingSequence[attackingSequenceIndex].baseSpeed;
				default:
					return 0;
			}
		}

		private void SwitchTailMode()
		{
			switch (tailMode)
			{
				case TailMode.PreparingAttack:
					tailMode = TailMode.Attacking;
					break;
				case TailMode.ReloadingAttack:
					reloadModeTargetIndex++;
					break;
				case TailMode.Attacking:
					attackingSequenceIndex++;
					tailPositionsInLastMovement.Add(tailTarget.localPosition);
					break;
			}
		}

		private void CheckForInput()
		{
			if (isPaused) return;
			if (keyboard.xKey.wasPressedThisFrame && tailMode == TailMode.Idle)
			{
				tailPositionsInLastMovement.Add(tailTarget.localPosition);
				tailPositionsInLastMovement.Add(preparationTargetLocalPos);
				tailMode = TailMode.PreparingAttack;
				attackDirection = RIGHT;
			}
			else if (keyboard.zKey.wasPressedThisFrame && tailMode == TailMode.Idle)
			{
				tailPositionsInLastMovement.Add(tailTarget.localPosition);
				tailPositionsInLastMovement.Add(preparationTargetLocalPos);
				tailMode = TailMode.PreparingAttack;
				attackDirection = LEFT;
			}
		}

		private void MoveFromSideToSide()
		{
			if (isGoingLeft)
			{
				Vector3 currentLocalPos = tailTarget.localPosition;
				Vector3 localPosTarget = new Vector3(currentLocalPos.x - (Time.deltaTime * tailMovementSpeedIdle),
					TAIL_HEIGHT, TAIL_MIDDLE_Z - moveForwardFactor * zOffsetCurve.Evaluate(Mathf.Abs(tailTarget.localPosition.x) /
						RIGHT_TAIL_POS_X));
				tailTarget.localPosition = localPosTarget;
				RotateTail(Vector3.left);
				if (Mathf.Abs(tailTarget.localPosition.x - LEFT_TAIL_POS_X) <= EPSILON)
				{
					isGoingLeft = false;
				}
			}
			else
			{
				Vector3 currentLocalPos = tailTarget.localPosition;
				Vector3 localPosTarget = new Vector3(currentLocalPos.x + (Time.deltaTime * tailMovementSpeedIdle),
					TAIL_HEIGHT, TAIL_MIDDLE_Z - moveForwardFactor * zOffsetCurve.Evaluate(Mathf.Abs(tailTarget.localPosition.x) /
						RIGHT_TAIL_POS_X));
				tailTarget.localPosition = localPosTarget;
				RotateTail(Vector3.forward);
				if (Mathf.Abs(tailTarget.localPosition.x - RIGHT_TAIL_POS_X) <= EPSILON)
				{
					isGoingLeft = true;
				}
			}
		}

		private void RotateTail(Vector3 direction) // Should be called from update, uses Time.deltaTime
		{
			tailTarget.Rotate(direction * (rotationSpeed * Time.deltaTime));
		}

		public bool IsTailInAttackMode()
		{
			return tailMode == TailMode.Attacking || tailMode == TailMode.PreparingAttack;
		}
	}
}