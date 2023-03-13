using UnityEngine;

public class RockInfo
{
	public RockInfo(RockScript script, Coroutine spawnCoroutine)
	{
		Script = script;
		SpawnCoroutine = spawnCoroutine;
	}

	public RockScript Script { get; set; }
	public Coroutine SpawnCoroutine { get; set; }
}