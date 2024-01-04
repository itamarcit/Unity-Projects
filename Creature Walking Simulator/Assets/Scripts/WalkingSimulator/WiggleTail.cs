using UnityEngine;

namespace ItamarRamon
{
	public class WiggleTail : MonoBehaviour
	{
		[SerializeField] private float tailMovementSpeed = 2f;
		[SerializeField] private Transform tailTarget;
		[SerializeField] private AnimationCurve zOffsetCurve;
		[SerializeField] private float rotationSpeed = 10f;
		[SerializeField] private float moveForwardFactor = 1f;
		private bool isGoingLeft = true;
		private const float RIGHT_TAIL_POS_X = 2.662f;
		private const float LEFT_TAIL_POS_X = -2.662f;
		private const float TAIL_HEIGHT = 4f;
		private float originalZ;

		private void Awake()
		{
			originalZ = tailTarget.localPosition.z;
		}

		private void Update()
		{
			MoveFromSideToSide();
		}

		private void MoveFromSideToSide()
		{
			if (isGoingLeft)
			{
				Vector3 currentLocalPos = tailTarget.localPosition;
				Vector3 localPosTarget = new Vector3(currentLocalPos.x - (Time.deltaTime * tailMovementSpeed),
					TAIL_HEIGHT, originalZ - moveForwardFactor * zOffsetCurve.Evaluate(Mathf.Abs(tailTarget.localPosition.x) /
						RIGHT_TAIL_POS_X));
				tailTarget.localPosition = localPosTarget;
				tailTarget.Rotate(Vector3.left * (rotationSpeed * Time.deltaTime));
				if (Mathf.Abs(tailTarget.localPosition.x - LEFT_TAIL_POS_X) <= .1f)
				{
					isGoingLeft = false;
				}
			}
			else
			{
				Vector3 currentLocalPos = tailTarget.localPosition;
				Vector3 localPosTarget = new Vector3(currentLocalPos.x + (Time.deltaTime * tailMovementSpeed),
					TAIL_HEIGHT, originalZ - moveForwardFactor * zOffsetCurve.Evaluate(Mathf.Abs(tailTarget.localPosition.x) /
						RIGHT_TAIL_POS_X));
				tailTarget.localPosition = localPosTarget;
				tailTarget.Rotate(Vector3.forward * (rotationSpeed * Time.deltaTime));
				if (Mathf.Abs(tailTarget.localPosition.x - RIGHT_TAIL_POS_X) <= .1f)
				{
					isGoingLeft = true;
				}
			}
		}
	}
}