using System.Collections.Generic;
using UnityEngine;

public class DropGenerator : MonoBehaviour
{
#region Fields
	[SerializeField] private List<GameObject> dropsPrefabs;
	private GameObject _currentDrop;
#endregion

#region Methods
	public void GenerateDrop()
	{
		if (_currentDrop != null) return;
		Vector2Int spawnPos = GridManager.Shared.GetRandomEmptyGridSquare();
		if (spawnPos.x == -1) // no place found.
		{
			return;
		}
		int randomIndex = Random.Range(0, dropsPrefabs.Count);
		_currentDrop = Instantiate(dropsPrefabs[randomIndex]);
		_currentDrop.transform.position = GridManager.Shared.GetWorldPosition(spawnPos.x, spawnPos.y);
	}
#endregion
}