using System.Collections;
using Avrahamy;
using Avrahamy.EditorGadgets;
using Avrahamy.Math;
using BitStrap;
using UnityEngine;
using WalkingSimulator;

namespace Flocking
{
	public class PeepMainController : MonoBehaviour
	{
		[SerializeField] PeepModel peep;
		[ImplementsInterface(typeof(IMoveable))] [SerializeField]
		MonoBehaviour _moveable;
		[SerializeField] InputModel inputModel;
		[SerializeField] float senseRadius = 2f;
		[SerializeField] PassiveTimer navigationTimer;
		[SerializeField] LayerMask navigationMask;
		[TagSelector] [SerializeField] string peepTag;
		[SerializeField] bool repelFromSameGroup;
		[SerializeField] private int hitsOfPlayerModifier = 1000;
		// These 3 attributes below should sum to 1
		[SerializeField] [Range(0, 1)] private float separationWeight;
		[SerializeField] [Range(0, 1)] private float alignmentWeight;
		[SerializeField] [Range(0, 1)] private float cohesionWeight;
		[SerializeField] private float timerBeforeHittingPlayer = 2f;
		[SerializeField] private bool drawDebugFlockingArrows = false;
		[SerializeField] [ConditionalHide("drawDebugFlockingArrows")]
		private float arrowDuration = 2f;
		private PeepHitController minionHitController;
		private bool _isAttacking;
		private float _nearPlayerTimer;
		private static readonly Collider[] COLLIDER_RESULTS = new Collider[MAX_COLLISIONS];
		private IMoveable moveable;
		private bool _isNearPlayer;
		private Animator _animator;
		private Vector3 origPosition;
		private Quaternion origRotation;
		// Constants
		private static readonly int IsRunning = Animator.StringToHash("IsRunning");
		private const int PLAYER_GROUP = -1;
		private const string ATTACK_1_STATE_NAME = "Attack 1";
		private const string ATTACK_2_STATE_NAME = "Attack 2";
		private const string IDLE_STATE_NAME = "Idle";
		public const int MAX_COLLISIONS = 100;

		protected void OnEnable()
		{
			navigationTimer.Start();
			_isNearPlayer = false;
			_isAttacking = false;
			_nearPlayerTimer = timerBeforeHittingPlayer;
			moveable.Velocity = Vector3.zero;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
		}

		protected void Awake()
		{
			moveable = _moveable as IMoveable;
			_animator = GetComponent<Animator>();
			_nearPlayerTimer = timerBeforeHittingPlayer;
			minionHitController = GetComponent<PeepHitController>();
		}

		protected void Reset()
		{
			if (peep == null)
			{
				peep = GetComponent<PeepModel>();
			}
			if (_moveable == null)
			{
				_moveable = GetComponent<IMoveable>() as MonoBehaviour;
			}
			if (inputModel == null)
			{
				inputModel = GetComponent<InputModel>();
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Around Player"))
			{
				_isNearPlayer = true;
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Around Player"))
			{
				_isNearPlayer = false;
			}
		}

		protected void Update()
		{
			if (_isAttacking) return;
			if (_isNearPlayer)
			{
				_nearPlayerTimer -= Time.deltaTime;
				inputModel.MoveInput = Vector2.zero;
				moveable.Velocity = Vector3.zero;
				if (_nearPlayerTimer <= 0 && !_isAttacking) Attack();
			}
			else
			{
				_nearPlayerTimer = timerBeforeHittingPlayer;
				FlockingBehavior();
			}
			HandleWalkingAnimation();
		}

		private void Attack()
		{
			_isAttacking = true;
			StartCoroutine(PerformAttack());
		}

		private IEnumerator PerformAttack()
		{
			int getRandomAnim = Random.Range(0, 2);
			switch (getRandomAnim)
			{
				case 0:
					_animator.Play(ATTACK_1_STATE_NAME);
					break;
				case 1:
					_animator.Play(ATTACK_2_STATE_NAME);
					break;
			}
			while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(IDLE_STATE_NAME))
			{
				yield return null;
			}
			if(!minionHitController.IsHit()) GameManager.Shared.LowerPlayerHealth(); // Check if the peep isn't hit already
			_isAttacking = false;
		}

		private void HandleWalkingAnimation()
		{
			_animator.SetBool(IsRunning, !_isNearPlayer);
		}

		private void FlockingBehavior()
		{
			if (navigationTimer.IsActive) return;
			navigationTimer.Start();
			var position = moveable.Position;

			// Check for colliders in the sense radius.
			var hits = Physics.OverlapSphereNonAlloc(
				position,
				senseRadius,
				COLLIDER_RESULTS,
				navigationMask.value);

			// There will always be at least one hit on our own collider.
			if (hits <= 1) return;
			Vector3 avgDirection = CalculateSeparationVector(hits, position);
			Vector3 alignmentVector = CalculateAlignmentVector(hits);
			Vector3 cohesionVector = CalculateCohesionVector(hits);
			if (drawDebugFlockingArrows)
				DrawFlockingArrows(avgDirection, alignmentVector, cohesionVector);
			Vector2 moveVector = separationWeight * avgDirection.normalized.ToVector2XZ() +
								 alignmentWeight * alignmentVector.normalized.ToVector2XZ() +
								 cohesionWeight * cohesionVector.normalized.ToVector2XZ();
			inputModel.MoveInput = moveVector.normalized;
		}

		private Vector3 CalculateSeparationVector(int hits, Vector3 position)
		{
			var avgDirection = Vector3.zero;
			for (int i = 0; i < hits; i++)
			{
				var hit = COLLIDER_RESULTS[i];
				// Ignore self.
				if (!hit.gameObject.activeSelf) continue;
				if (hit.attachedRigidbody == null) continue;
				if (hit.attachedRigidbody.gameObject == gameObject) continue;

				// Always repel from walls.
				var repel = true;
				if (hit.CompareTag(peepTag))
				{
					// Sensed another peep.
					var otherPeed = hit.attachedRigidbody.GetComponent<PeepModel>();
					// Ignore the player.
					if (otherPeed.Group == PLAYER_GROUP) continue;
					repel = repelFromSameGroup;
				}
				var closestPoint = hit.ClosestPoint(position);
				closestPoint.y = 0f;
				var direction = closestPoint - position;
				var magnitude = direction.magnitude;
				var distancePercent = repel
					? Mathf.InverseLerp(peep.SelfRadius, senseRadius, magnitude)
					// Inverse attraction factor so peeps won't be magnetized to
					// each other without being able to separate.
					: Mathf.InverseLerp(senseRadius, peep.SelfRadius, magnitude);

				// Make sure the distance % is not 0 to avoid division by 0.
				distancePercent = Mathf.Max(distancePercent, 0.01f);

				// Force is stronger when distance percent is closer to 0 (1/x-1).
				var forceWeight = 1f / distancePercent - 1f;

				// Angle between forward to other collider.
				var angle = transform.forward.GetAngleBetweenXZ(direction);
				var absAngle = Mathf.Abs(angle);
				if (absAngle > 90f)
				{
					// Decrease weight of colliders that are behind the peep.
					// The closer to the back, the lower the weight.
					var t = Mathf.InverseLerp(180f, 90f, absAngle);
					forceWeight *= Mathf.Lerp(0.1f, 1f, t);
				}
				direction = direction.normalized * forceWeight;
				if (repel)
				{
					avgDirection -= direction;
				}
				else
				{
					avgDirection += direction;
				}
			}
			return avgDirection;
		}

		private Vector3 CalculateAlignmentVector(int hits)
		{
			Vector3 alignmentVector = Vector3.zero;
			int totalPeepsHit = 0;
			for (int i = 0; i < hits; i++)
			{
				var hit = COLLIDER_RESULTS[i];
				if (hit.attachedRigidbody == null) continue;                  // Ignore non-rigidbody colliders.
				if (hit.attachedRigidbody.gameObject == gameObject) continue; // Ignore Self.
				if (hit.CompareTag("Peep"))
				{
					alignmentVector += hit.transform.forward;
					totalPeepsHit++;
				}
			}
			if (totalPeepsHit != 0)
			{
				alignmentVector /= totalPeepsHit;
			}
			else // No other peeps found, so use own alignment.
			{
				alignmentVector = transform.forward;
			}
			return alignmentVector;
		}

		private Vector3 CalculateCohesionVector(int hits)
		{
			Vector3 otherRigidbodiesPositionSum = Vector3.zero;
			var actualHitsCohesion = 0;
			for (int i = 0; i < hits; i++)
			{
				var hit = COLLIDER_RESULTS[i];
				if (hit.attachedRigidbody == null) continue;                  // Ignore non-rigidbody hits
				if (hit.attachedRigidbody.gameObject == gameObject) continue; // Ignore self
				var otherPeepModel = hit.gameObject.GetComponentInParent<PeepModel>();
				if (otherPeepModel == null) continue; // Ignore others without peep model.
				// Ignore peeps from other groups. (Except the player.
				if (otherPeepModel.Group != peep.Group && otherPeepModel.Group != PLAYER_GROUP) continue;

				//an attempt to give more cohesion weight to MyCreature so peeps dont stick together
				if (otherPeepModel.Group == PLAYER_GROUP) // Player
				{
					actualHitsCohesion += hitsOfPlayerModifier;
					otherRigidbodiesPositionSum += (hitsOfPlayerModifier * hit.attachedRigidbody.position);
				}
				else
				{
					otherRigidbodiesPositionSum += hit.attachedRigidbody.position;
					actualHitsCohesion++;
				}
			}
			Vector3 cohesionVector = Vector3.zero;
			if (actualHitsCohesion != 0)
			{
				cohesionVector = (otherRigidbodiesPositionSum / actualHitsCohesion) - transform.position;
			}
			return cohesionVector;
		}

		private void DrawFlockingArrows(Vector3 avgDirection, Vector3 alignmentVector, Vector3 cohesionVector)
		{
			DebugDraw.DrawArrowXZ(
				moveable.Position + Vector3.up,
				(avgDirection.normalized.ToVector2XZ()),
				1f,
				30f,
				Color.red, arrowDuration);
			DebugDraw.DrawArrowXZ(
				moveable.Position + Vector3.up,
				(alignmentVector.normalized.ToVector2XZ()),
				1f,
				30f,
				Color.yellow, arrowDuration);
			DebugDraw.DrawArrowXZ(
				moveable.Position + Vector3.up,
				(cohesionVector.normalized.ToVector2XZ()),
				1f,
				30f,
				Color.cyan, arrowDuration);
		}

		protected void OnDrawGizmos()
		{
			var angle = transform.forward.GetAngleXZ();
			DebugDraw.GizmosDrawSector(
				transform.position,
				senseRadius,
				180f + angle,
				-180f + angle,
				Color.green);
		}
	}
}