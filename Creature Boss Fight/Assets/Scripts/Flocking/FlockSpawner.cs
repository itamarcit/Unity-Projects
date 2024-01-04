using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avrahamy.EditorGadgets;
using Flocking;
using UnityEngine;

public class FlockSpawner : MonoBehaviour
{
	/// <summary>
	/// A spawner for the flocks, implements a pooling for the flock prefabs.
	/// </summary>
	[SerializeField] [Range(3, 10)] private float flockSpawnRatePerSecond = 10f;
	[SerializeField] private List<GameObject> flockPrefabs;
	[SerializeField] private List<Transform> spawnLocations;
	[SerializeField] [Range(1, 4)]
	private int minFlocksPerWave = 2; // The range is capped at 4 because there are 4 spawn locations. Change if needed.
	[SerializeField] [Range(1, 4)]
	private int maxFlocksPerWave = 4; // The range is capped at 4 because there are 4 spawn locations. Change if needed.
	[SerializeField] private bool shouldDecreaseSpawnTimer;
	[SerializeField]
	[Tooltip("Decreases the spawn timer by value after amount of groups defeated by player")]
	[ConditionalHide("shouldDecreaseSpawnTimer")]
	private float decreaseSpawnTimer = 0.5f;
	[SerializeField] [Tooltip("Groups to defeat before timer decrease")] [ConditionalHide("shouldDecreaseSpawnTimer")]
	private int groupsDefeatedToIncrease = 4;
	private readonly Dictionary<int, Stack<FlockWrapper>> _inactivePrefabs = new();
	private readonly List<FlockWrapper> _activeFlocks = new();
	private int _groupsDefeated = 0;
	private float _spawnTimer;
	private bool _spawnMorePeeps;
	private bool _groupsDefeatedUpdated;
	private const float MIN_SPAWN_TIMER = 3f;
	private const int NUM_OF_PEEPS_IN_FLOCK = 4;

	private void Awake()
	{
		InitializeFlockDictionary();
	}

	private void Start()
	{
		StartCoroutine(SpawnFlockIfNeeded());
		StartCoroutine(GameManager.Shared.ResetPlayerDetectionCollider());
	}

	private void Update()
	{
		ReturnInactiveFlocksToPool();
		_spawnMorePeeps = _activeFlocks.All(wrapper => wrapper.flockManager.AreAllPeepsHit());
		if (_groupsDefeatedUpdated && _groupsDefeated % groupsDefeatedToIncrease == 0)
		{
			IncreaseDifficulty();
		}
	}

	private void IncreaseDifficulty()
	{
		_groupsDefeatedUpdated = false;
		if (flockSpawnRatePerSecond > MIN_SPAWN_TIMER)
		{
			flockSpawnRatePerSecond -= decreaseSpawnTimer;
		}
		else
		{
			flockSpawnRatePerSecond = MIN_SPAWN_TIMER;
		}
		GameManager.Shared.IncreasePeepDamage();
	}

	private void ReturnInactiveFlocksToPool()
	{
		for (int i = 0; i < _activeFlocks.Count; i++)
		{
			List<PeepModel> peepModels = _activeFlocks[i].flockManager.GetPeepModels();
			bool areAllPeepsInactive = peepModels.All(peepModel => !peepModel.gameObject.activeSelf);
			if (areAllPeepsInactive)
			{
				ReturnToPool(i);
			}
		}
	}

	private IEnumerator SpawnFlockIfNeeded()
	{
		while (true)
		{
			// Do not spawn if the peeps are capped by the sense collision detect array size.
			while (_activeFlocks.Count * NUM_OF_PEEPS_IN_FLOCK >= PeepMainController.MAX_COLLISIONS) yield return null;
			// If the game is not over, spawn more peeps.
			if (!GameManager.Shared.IsGameOver())
			{
				SpawnFlocks();
			}
			yield return WaitOrSpawnIfNeeded();
		}
	}

	private IEnumerator WaitOrSpawnIfNeeded()
	{
		float timer = 0;
		while (timer < flockSpawnRatePerSecond)
		{
			if (_spawnMorePeeps && !GameManager.Shared.IsGameOver())
			{
				timer = SpawnFlockAndRestartTimer();
			}
			timer += Time.deltaTime;
			yield return null;
		}
	}

	private float SpawnFlockAndRestartTimer()
	{
		float timer;
		_spawnMorePeeps = false;
		SpawnFlocks();
		timer = 0;
		return timer;
	}

	private void SpawnFlocks()
	{
		int randomlySelectedFlocksAmount = Random.Range(minFlocksPerWave, maxFlocksPerWave + 1);
		Transform[] currentSpawnLocations = GetUniqueRandomIndicesFromList(
			spawnLocations, randomlySelectedFlocksAmount);
		int[] prefabIndicesToSpawn = GetUniqueRandomIndicesFromList(
			Enumerable.Range(0, flockPrefabs.Count).ToList(), randomlySelectedFlocksAmount);
		for (int i = 0; i < randomlySelectedFlocksAmount; i++)
		{
			FlockWrapper flockToSpawn = GetNewFlock(prefabIndicesToSpawn[i]);
			_activeFlocks.Add(flockToSpawn);
			flockToSpawn.flockManager.gameObject.SetActive(true);
			flockToSpawn.flockManager.gameObject.transform.position = currentSpawnLocations[i].position;
		}
	}

	/// <summary>
	/// Shuffles the given list.
	/// </summary>
	/// <param name="list">Given list</param>
	private static void ShuffleList(IList<int> list)
	{
		int n = list.Count;
		for (int i = list.Count - 1; i > 1; i--)
		{
			int rnd = Random.Range(0, i + 1); //random.Next(i + 1);  
			(list[rnd], list[i]) = (list[i], list[rnd]);
		}
	}

	/// <summary>
	/// Returns a random array containing random elements from the provided list.
	/// </summary>
	/// <param name="listToTakeFrom">The list to take elements from</param>
	/// <param name="amountToTake">The returned array length</param>
	/// <typeparam name="T">Any list/array element</typeparam>
	/// <returns>A random array containing random elements from the provided list.</returns>
	private static T[] GetUniqueRandomIndicesFromList<T>(List<T> listToTakeFrom, int amountToTake)
	{
		T[] result = new T[amountToTake];
		List<int> randomList = Enumerable.Range(0, listToTakeFrom.Count).ToList();
		ShuffleList(randomList);
		for (int i = 0; i < amountToTake; i++)
		{
			result[i] = listToTakeFrom[randomList[i]];
		}
		return result;
	}

	private FlockWrapper GetNewFlock(int prefabIndex)
	{
		FlockWrapper result = _inactivePrefabs[prefabIndex].Pop();
		FillDictionaryStackIfNeeded(prefabIndex);
		return result;
	}

	private void FillDictionaryStackIfNeeded(int dictionaryKey)
	{
		if (_inactivePrefabs[dictionaryKey].Count == 0)
		{
			GameObject inactiveInstantiated = Instantiate(flockPrefabs[dictionaryKey]);
			inactiveInstantiated.SetActive(false);
			_inactivePrefabs[dictionaryKey].Push(new FlockWrapper(dictionaryKey,
				inactiveInstantiated.GetComponent<FlockManager>()));
		}
	}

	private void InitializeFlockDictionary()
	{
		for (int i = 0; i < flockPrefabs.Count; i++)
		{
			Stack<FlockWrapper> tempStack = new();
			GameObject inactiveInstantiated = Instantiate(flockPrefabs[i]);
			inactiveInstantiated.SetActive(false);
			tempStack.Push(new FlockWrapper(i, inactiveInstantiated.GetComponent<FlockManager>()));
			_inactivePrefabs.Add(i, tempStack);
		}
	}

	private void ReturnToPool(int activeListIndex)
	{
		FlockWrapper toRemove = _activeFlocks[activeListIndex];
		toRemove.flockManager.gameObject.SetActive(false);
		_inactivePrefabs[toRemove.index].Push(toRemove);
		_activeFlocks.RemoveAt(activeListIndex);
		_groupsDefeated++;
		_groupsDefeatedUpdated = true;
	}
}