using System;
using UnityEngine;

public class CheckLeafWinCondition : MonoBehaviour
{
	private void FixedUpdate()
	{
		int maxLeaves = GPUInstancing._startNumOfLeaves;
		int numOfLeavesFell = (GPUInstancing._startNumOfLeaves - GPUInstancing._leafCountOnMainGround);
		int percentComplete = (int)(((float)numOfLeavesFell / maxLeaves) * 100);
		float percentageToNext = GameManager.Shared.GetPercentageNeeded();
		if (percentComplete >= percentageToNext && !GameManager.Shared.IsAdvancingStage() &&
			GPUInstancing._startNumOfLeaves > 0 && numOfLeavesFell > 0)
		{
			GameManager.Shared.LevelComplete();
		}
	}

	private void Update()
	{
		if ((Input.GetKey(KeyCode.X) && Input.GetKeyDown(KeyCode.O)) ||
			(Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.O))) GameManager.Shared.LevelComplete();
	}
}