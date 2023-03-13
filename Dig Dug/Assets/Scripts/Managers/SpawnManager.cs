using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
#region Fields
	[SerializeField] private GameObject pookaPrefab;
	[SerializeField] private GameObject fygarPrefab;
	[SerializeField] private GameObject rockPrefab;
	[SerializeField] private DropGenerator dropGenerator;
	[SerializeField] private float enemySpawnRateInSeconds = 15;
	[SerializeField] private float rockSpawnRateInSeconds = 23;
	[SerializeField] private float dropRateInSeconds = 30;
	[SerializeField] private Sprite spawningRock;
	[SerializeField] private float spawnSpeed = 7f;
	private float _rockTimer;
	private float _enemyTimer;
	private float _dropTimer;
	private readonly List<EnemyInfo> spawnedEnemies = new();
	private readonly List<RockInfo> spawnedRocks = new();
	private static readonly int IsSpawning = Animator.StringToHash("IsSpawning");
	private const int TOTAL_DIFFERENT_ENEMIES = 2;
	public static SpawnManager Shared { get; private set; }
#endregion
#region Events
	private void Awake()
	{
		if (Shared == null)
		{
			Shared = this;
			ResetTimers();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		UpdateTimers();
	}
#endregion
#region Methods
	private void UpdateTimers()
	{
		if (_enemyTimer > 0)
		{
			_enemyTimer -= Time.deltaTime;
		}
		else
		{
			SpawnRandomEnemy();
			_enemyTimer = enemySpawnRateInSeconds;
		}
		if (_rockTimer > 0)
		{
			_rockTimer -= Time.deltaTime;
		}
		else
		{
			SpawnRandomRock();
			_rockTimer = rockSpawnRateInSeconds;
		}
		if (_dropTimer > 0)
		{
			_dropTimer -= Time.deltaTime;
		}
		else
		{
			dropGenerator.GenerateDrop();
			_dropTimer = dropRateInSeconds;
		}
	}

	private void SpawnRandomEnemy()
	{
		int i = spawnedEnemies.Count;
		spawnedEnemies.Add(new EnemyInfo(null, null));
		spawnedEnemies[i].SpawnCoroutine = StartCoroutine(SpawnEnemy(i));
	}

	private void SpawnRandomRock()
	{
		spawnedRocks.Add(new RockInfo(null, null));
		spawnedRocks[^1].SpawnCoroutine = StartCoroutine(SpawnRock(spawnedRocks.Count - 1));
	}

	private IEnumerator SpawnEnemy(int listIndex)
	{
		int isPooka = Random.Range(0, TOTAL_DIFFERENT_ENEMIES);
		GameObject toSpawn = isPooka == 1 ? pookaPrefab : fygarPrefab;
		Vector2Int gridPos = GridManager.Shared.GetRandomEmptyGridSquare();
		if (gridPos.x == -1)
		{
			// no place found.
			spawnedEnemies.RemoveAt(spawnedEnemies.Count - 1);
			yield break;
		}
		Vector3 worldPos = GridManager.Shared.GetWorldPosition(gridPos.x, gridPos.y);
		GameObject enemy = Instantiate(toSpawn);
		EnemyController controller = enemy.GetComponent<EnemyController>();
		spawnedEnemies[listIndex].Controller = controller;
		controller.InitializeSpawnedEnemy(gridPos.x, gridPos.y);
		yield return new WaitForSeconds(0.5f);
		enemy.transform.position = worldPos;
		yield return new WaitForSeconds(spawnSpeed);
		controller.FinishSpawn();
	}

	private IEnumerator SpawnRock(int listIndex)
	{
		Vector2Int rockLoc = GridManager.Shared.GetRandomRockLocation();
		if (rockLoc.x == -1)
		{
			spawnedRocks.RemoveAt(spawnedRocks.Count - 1);
			yield break;
		}
		GameObject rockObject = Instantiate(rockPrefab);
		rockObject.transform.position = GridManager.Shared.GetWorldPosition(rockLoc.x, rockLoc.y);
		RockScript rockScript = rockObject.GetComponent<RockScript>();
		rockScript.SpawnRock(rockLoc);
		spawnedRocks[listIndex].Script = rockScript;
		yield return StartRockAppearance(rockScript.GetRenderer());
		GridManager.Shared.RegisterRock(rockLoc.x, rockLoc.y);
		rockScript.FinishSpawn();
	}

	private IEnumerator StartRockAppearance(SpriteRenderer rockRenderer)
	{
		float elapsedTime = 0;
		Sprite oldSprite = rockRenderer.sprite;
		while (elapsedTime < spawnSpeed)
		{
			rockRenderer.sprite = spawningRock;
			yield return new WaitForSeconds(0.5f);
			rockRenderer.sprite = null;
			yield return new WaitForSeconds(0.5f);
			elapsedTime += 1f;
		}
		rockRenderer.sprite = oldSprite;
	}

	public void RestartSpawns()
	{
		foreach (EnemyInfo enemy in spawnedEnemies)
		{
			if (enemy != null)
			{
				if (enemy.SpawnCoroutine != null) StopCoroutine(enemy.SpawnCoroutine);
				if (enemy.Controller != null) Destroy(enemy.Controller.gameObject);
			}
		}
		foreach (RockInfo rock in spawnedRocks)
		{
			if (rock != null)
			{
				if (rock.SpawnCoroutine != null) StopCoroutine(rock.SpawnCoroutine);
				if (rock.Script != null)
				{
					Destroy(rock.Script.gameObject);
					Vector2Int rockPos = rock.Script.GetGridLocation();
					GridManager.Shared.DeregisterRock(rockPos.x, rockPos.y);
				}
			}
		}
		spawnedEnemies.Clear();
		spawnedRocks.Clear();
		ResetTimers();
	}

	private void ResetTimers()
	{
		_rockTimer = rockSpawnRateInSeconds;
		_enemyTimer = enemySpawnRateInSeconds;
		_dropTimer = dropRateInSeconds;
	}
#endregion
}