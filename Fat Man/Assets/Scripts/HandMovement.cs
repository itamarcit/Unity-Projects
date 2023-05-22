using UnityEngine;

public class HandMovement : MonoBehaviour
{
	private Transform _activeVegetableTransform;

	private void Update()
	{
		if (!GameManager.Shared.IsActiveBlockFalling())
		{
			_activeVegetableTransform = GameManager.Shared.GetActiveVegetableTransform();
			transform.position = new Vector3(_activeVegetableTransform.position.x, transform.position.y, 0);
		}
	}
}