using UnityEngine;

public class EnemyInfo
{
	public EnemyInfo(EnemyController controller, Coroutine spawnCoroutine)
	{
		Controller = controller;
		SpawnCoroutine = spawnCoroutine;
	}

	public EnemyController Controller { get; set; }
	public Coroutine SpawnCoroutine { get; set; }
}