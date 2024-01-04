using System;
using UnityEngine;

namespace WalkingSimulator
{
	[Serializable]
	public class TailMovementData
	{
		public MovementDir movementDir;
		public float valueRight;
		public float valueLeft;
		public float baseSpeed;
		public bool isSkippable;
	
		public enum MovementDir
		{
			X,Y,Z
		}

		
		// Returns a vector with the same values in relatedTarget, except the X,Y or Z based on MovementDir.
		public Vector3 GetTargetVector(Vector3 relatedTarget, bool isLeft)
		{
			float value = isLeft ? valueLeft : valueRight;
			switch (movementDir)
			{
				case MovementDir.X:
					return new Vector3(value, relatedTarget.y, relatedTarget.z);
				case MovementDir.Y:
					return new Vector3(relatedTarget.x, value, relatedTarget.z);
				case MovementDir.Z:
					return new Vector3(relatedTarget.x, relatedTarget.y, value);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
