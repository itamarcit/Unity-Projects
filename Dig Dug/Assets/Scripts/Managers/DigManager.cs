using UnityEngine;

public class DigManager : MonoBehaviour
{
#region Fields
	[SerializeField] private GameObject movementHolePrefab;
	[SerializeField] private GameObject arrivedHolePrefabLeft;
	public static DigManager Shared { get; private set; }
#endregion

#region Events
	private void Awake()
	{
		if (Shared == null)
		{
			Shared = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}
#endregion

#region Methods
	public void CreateMovingHole(Vector3 position)
	{
		GameObject obj = Instantiate(movementHolePrefab);
		obj.transform.position = position;
	}

	public void CreateStationaryHole(Vector3 position, Vector3Int direction)
	{
		GameObject obj =
			Instantiate(arrivedHolePrefabLeft);
		obj.transform.position = position;
		obj.transform.localEulerAngles = new Vector3(0, 0,
			(direction == Vector3Int.up || direction == Vector3Int.down) ? 90 : 0);
	}
#endregion
}