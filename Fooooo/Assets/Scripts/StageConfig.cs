using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageConfig
{
	public float maxTimeForThreeStars;
	public float maxTimeForTwoStars;
	public float percentageToComplete;
	public List<GameObject> objectsToToggle;
	public Collider enterToStageCollider;
	public Collider islandBoundsCollider;

	public void ToggleObjects(bool setActiveBool)
	{
		foreach (GameObject objectToToggle in objectsToToggle)
		{
			// Debug.Log("hi");
			objectToToggle.SetActive(setActiveBool);
		}
	}
}
