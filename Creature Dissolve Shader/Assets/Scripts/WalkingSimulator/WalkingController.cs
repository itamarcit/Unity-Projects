using System.Collections;
using Avrahamy;
using UnityEngine;
using UnityEngine.Serialization;

namespace ItamarRamon
{
	public class WalkingController : MonoBehaviour
	{
		private const int FRONT_LEFT_LEG = 0;
		private const int FRONT_RIGHT_LEG = 1;
		private const int BACK_LEFT_LEG = 2;
		private const int BACK_RIGHT_LEG = 3;
		private const float BODY_HEIGHT_OFFSET = 0.31f;
		private const float smoothTime = 0.5f;
		private const float BODY_ROTATION_SPEED = 50f;
		private const int TOTAL_LEGS = 4;
		private const float RETURN_HEAD_TO_PLACE_SPEED = 30f;
		private const float TILT_HEAD_SPEED = 10f;
		private const float HEAD_MOVEMENT_COOLDOWN = 3f;
		private const int HEAD = 4;
		private const int TAIL = 5;
		private const int FRONT_LEG = 1;
		private const int TILT_HEAD_MAX_DEGREES = 20;
		private const int HEAD_MOVEMENT_SPEED = 10;
		private const float DIRECTION_MAGNITUDE = 4f;
		private const float TAIL_PELVIS_OFFSET = 0.49f;

		[Header("Speed setting")] [SerializeField] [Range(0.01f, 5)]
		private float stepLength;
		[SerializeField] [Range(0.01f, 1)] private float stepTimer;
		[FormerlySerializedAs("animationCurve")] [SerializeField]
		private AnimationCurve upDownAnimationCurve;
		[SerializeField] private AnimationCurve leftRightAnimationCurve;
		[SerializeField] [Range(1, 5)] private float bodyPartsAlignSpeed;

		[Header("Body parts")] [SerializeField]
		private Transform[] legsConstraints;
		[SerializeField] private Transform headConstraint;
		[SerializeField] private Transform tailConstraint;
		[SerializeField] private Transform[] legGoalTransform;
		[SerializeField] private Transform frontPelvis;
		[SerializeField] private Transform backPelvis;
		[SerializeField] private Transform tailPelvis;
		[SerializeField] private Transform creatureBody;

		[Header("Is grounded Setting")] [SerializeField]
		private Transform[] legsUpperpart;
		[SerializeField] private LayerMask groundLayer;

		private readonly PassiveTimer[] timer = new PassiveTimer[4];
		private PassiveTimer tailTimer;

		private readonly Vector3[] originPositions = new Vector3[6];
		private readonly Vector3[] prevPositions = new Vector3[4];
		private readonly Vector3[] nextPositions = new Vector3[4];
		private readonly Coroutine[] steps = new Coroutine[4];
		private readonly Ray[] legsRays = new Ray[4];
		private readonly bool[] canMoveLeg = new bool[4];
		private Coroutine moveHead;
		private bool isRotatingLeft = true;
		private Quaternion lookingLeft;
		private Quaternion lookingRight;
		private Quaternion lookingForward;
		private bool isIdle = true;
		private bool tiltingHead;
		private float headMovementCooldown = 1f;
		private int randomTailMove;
		private PassiveTimer timerForTail;

		private void Awake()
		{
			for (int i = 0; i < TOTAL_LEGS; i++)
			{
				originPositions[i] = legsConstraints[i].transform.position;
				prevPositions[i] = originPositions[i];
				canMoveLeg[i] = false;
				timer[i] = new PassiveTimer(stepTimer);
			}
			canMoveLeg[BACK_LEFT_LEG] = true;
			canMoveLeg[FRONT_LEFT_LEG] = true;
			canMoveLeg[FRONT_RIGHT_LEG] = false;
			canMoveLeg[BACK_RIGHT_LEG] = false;
			originPositions[HEAD] = headConstraint.transform.position;
			originPositions[TAIL] = tailConstraint.transform.position;
			lookingLeft = headConstraint.rotation * Quaternion.Euler(0, TILT_HEAD_MAX_DEGREES, 0);
			lookingRight = headConstraint.rotation * Quaternion.Euler(0, -TILT_HEAD_MAX_DEGREES, 0);
			lookingForward = headConstraint.rotation;
		}

		private void Update()
		{
			VerifyMovement();
			CheckIfIdle();
			for (int i = 0; i < TOTAL_LEGS; i++)
			{
				if (Vector3.Distance(prevPositions[i], legGoalTransform[i].position) >=
					stepLength)
				{
					if (steps[i] == null && canMoveLeg[i])
					{
						nextPositions[i] = legGoalTransform[i].position;
						steps[i] = StartCoroutine(TakeAStep(i));
						canMoveLeg[i] = false;
					}
					else if (steps[i] == null) // the leg isn't actively moving
					{
						legsConstraints[i].transform.position = prevPositions[i];
					}
					legsRays[i] = new Ray(legsUpperpart[i].position, Vector3.down);
					if (Physics.Raycast(legsRays[i], out RaycastHit hitInfo, Mathf.Infinity, groundLayer))
						legGoalTransform[i].transform.position = hitInfo.point;
				}
			}
			AlignBody();
			AlignTailPelvis();
			AlignHead();
			RotateHead();
		}

		private void CheckIfIdle()
		{
			bool prevIsIdle = isIdle;
			bool isLegMoving = false;
			for (int i = 0; i < 4; i++)
			{
				if (steps[i] != null)
				{
					isLegMoving = true;
					break;
				}
			}
			isIdle = !isLegMoving;
			if (isIdle != prevIsIdle) headMovementCooldown = HEAD_MOVEMENT_COOLDOWN;
		}

		private void RotateHead()
		{
			if (isIdle && headMovementCooldown <= 0)
			{
				TiltHead();
			}
			else if (!isIdle && headMovementCooldown <= 0)
			{
				tiltingHead = false;
				headConstraint.localRotation = Quaternion.RotateTowards(headConstraint.localRotation,
					isRotatingLeft ? lookingLeft : lookingRight, Time.deltaTime * TILT_HEAD_SPEED);
				if (Quaternion.Angle(headConstraint.localRotation, lookingRight) <= 0.5f ||
					Quaternion.Angle(headConstraint.localRotation, lookingLeft) <= 0.5f)
				{
					isRotatingLeft = !isRotatingLeft;
				}
			}
			else
			{
				if (tiltingHead)
				{
					headConstraint.localRotation = Quaternion.RotateTowards(headConstraint.localRotation,
						lookingForward,
						Time.deltaTime * RETURN_HEAD_TO_PLACE_SPEED);
				}
				headMovementCooldown -= Time.deltaTime;
			}
		}

		private void TiltHead()
		{
			if (tiltingHead || Quaternion.Angle(headConstraint.localRotation, lookingForward) <= 0.5f)
			{
				tiltingHead = true;
				Quaternion targetRotation = headConstraint.localRotation * Quaternion.Euler(0, 0, 30);
				if (Mathf.Abs(headConstraint.rotation.z) <= 0.3f)
				{
					headConstraint.localRotation = Quaternion.RotateTowards(headConstraint.localRotation,
						targetRotation, Time.deltaTime * 20f);
				}
			}
			else // rotating to normal before starting to tilt
			{
				headConstraint.localRotation = Quaternion.RotateTowards(headConstraint.localRotation, lookingForward,
					Time.deltaTime * 30f);
			}
		}

		private void VerifyMovement()
		{
			bool fixMovement = true;
			for (int i = 0; i < TOTAL_LEGS; i++)
			{
				if (canMoveLeg[i] || steps[i] != null)
				{
					fixMovement = false;
					break;
				}
			}
			if (fixMovement)
			{
				canMoveLeg[FRONT_LEFT_LEG] = true;
				canMoveLeg[BACK_LEFT_LEG] = true;
			}
		}

		private Vector3 _velocity;
		private void AlignHead()
		{
			Vector3 direction = (frontPelvis.position - backPelvis.position).normalized;
			Vector3 targetPosWorld = frontPelvis.position + direction * DIRECTION_MAGNITUDE;
			// headConstraint.position = Vector3.MoveTowards(headConstraint.position,
			// 	targetPosWorld, Time.deltaTime * bodyPartsAlignSpeed * HEAD_MOVEMENT_SPEED);
			headConstraint.position = Vector3.SmoothDamp(headConstraint.position,
				targetPosWorld,ref _velocity, smoothTime);
		}

		private void AlignTailPelvis()
		{
			Vector3 targetPosLocal = backPelvis.localPosition +
									 (backPelvis.localPosition - frontPelvis.localPosition).normalized * TAIL_PELVIS_OFFSET;
			tailPelvis.localPosition = Vector3.MoveTowards(tailPelvis.localPosition, targetPosLocal,
				Time.deltaTime * bodyPartsAlignSpeed);
		}

		private IEnumerator TakeAStep(int legNum)
		{
			timer[legNum].Start();
			canMoveLeg[GetLegMatch(legNum)] = false;
			while (timer[legNum].Progress <= 1)
			{
				legsConstraints[legNum].transform.position = Vector3.LerpUnclamped(prevPositions[legNum],
					nextPositions[legNum] + Vector3.up * (2 * upDownAnimationCurve.Evaluate(timer[legNum].Progress))
										  + (backPelvis.position - frontPelvis.position).normalized
										  * (-2 * leftRightAnimationCurve.Evaluate(timer[legNum].Progress)),
					timer[legNum].Progress);
				yield return null;
			}
			canMoveLeg[GetLegMatch(legNum)] = true;
			legsConstraints[legNum].transform.position = nextPositions[legNum];
			legsConstraints[legNum].GetComponent<AudioSource>().Play();
			yield return null;
			prevPositions[legNum] = nextPositions[legNum];
			timer[legNum].Clear();
			steps[legNum] = null;
			AlignPelvis(legNum);
		}

		private void AlignBody()
		{
			Vector3 lastPos = creatureBody.position;
			Vector3 worldPosTarget = new Vector3(lastPos.x,
				((backPelvis.position.y + frontPelvis.position.y) / 2) + BODY_HEIGHT_OFFSET, lastPos.z);
			Vector3 localPosTarget = creatureBody.InverseTransformPoint(worldPosTarget);
			creatureBody.localPosition = Vector3.MoveTowards(creatureBody.localPosition,
				creatureBody.localPosition + localPosTarget, Time.deltaTime * bodyPartsAlignSpeed);
			Quaternion targetRotation = Quaternion.LookRotation(-frontPelvis.position + backPelvis.position);
			creatureBody.rotation = Quaternion.RotateTowards(creatureBody.rotation, targetRotation,
				Time.deltaTime * BODY_ROTATION_SPEED);
		}

		private void AlignPelvis(int legNum)
		{
			if (Mathf.Abs(legsConstraints[legNum].position.y - legsConstraints[GetLegMatch(legNum)].position.y) < 2f)
			{
				if (legNum <= FRONT_LEG)
				{
					// front leg
					Vector3 currentPos = frontPelvis.position;
					frontPelvis.transform.position = new Vector3(currentPos.x, legsConstraints[legNum].position.y + 2,
						currentPos.z);
				}
				else // back leg
				{
					Vector3 currentPos = backPelvis.position;
					backPelvis.transform.position = new Vector3(currentPos.x, legsConstraints[legNum].position.y + 2,
						currentPos.z);
				}
			}
		}

		/// <summary>
		/// Gets the leg that is close to the given legNum based on the values in the inspector
		/// (if it's a front leg, will get the other front leg, etcetera)
		/// </summary>
		private int GetLegMatch(int legNum)
		{
			switch (legNum)
			{
				case FRONT_LEFT_LEG:
					return FRONT_RIGHT_LEG;
				case FRONT_RIGHT_LEG:
					return FRONT_LEFT_LEG;
				case BACK_LEFT_LEG:
					return BACK_RIGHT_LEG;
				case BACK_RIGHT_LEG:
					return BACK_LEFT_LEG;
				default:
					return -1;
			}
		}
	}
}