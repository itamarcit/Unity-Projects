using System;
using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;
using Random = UnityEngine.Random;

public class CubeSpawner : MonoBehaviour
{
	[SerializeField] private List<GameObject> carsPrefabs;
	[SerializeField] private int startingCarsPerPath = 6;
	[SerializeField] private List<PathCreator> paths;
	[SerializeField] private Transform blowerHead;
	[SerializeField] private float carRespawnTimer = 1f;
	private readonly Dictionary<int, Stack<CarWrapper>> _inactivePrefabs = new();

	private readonly Queue<CarWrapper> _carsListOne = new();
	private readonly Queue<CarWrapper> _carsListTwo = new();

	private float _carSpawnTimer;
	private WaitForSeconds _delayBetweenSpawn;
	private Coroutine _carSpawnerOne;
	private Coroutine _carSpawnerTwo;

	private void Awake()
	{
		InitializeCarDictionary();
	}

	private void OnEnable()
	{
		DeactivateAllCars();
		if (_carSpawnerOne != null) StopCoroutine(_carSpawnerOne);
		if (_carSpawnerTwo != null) StopCoroutine(_carSpawnerTwo);
		_delayBetweenSpawn = new WaitForSeconds(carRespawnTimer);
		_carSpawnerOne = StartCoroutine(SpawnNewCarIfNeeded(0));
		_carSpawnerTwo = StartCoroutine(SpawnNewCarIfNeeded(1));
	}

	private void Update()
	{
		UpdateCarsInPath(0);
		UpdateCarsInPath(1);
	}

	private void UpdateCarsInPath(int path)
	{
		DeActivateLastCarIfNeeded(path);
	}

	private IEnumerator SpawnNewCarIfNeeded(int path)
	{
		while (!GameManager.Shared.DidWinCarStage())
		{
			yield return _delayBetweenSpawn;
			AddCarToStart(path);
		}
		switch (path)
		{
			case 0:
				_carSpawnerOne = null;
				break;
			case 1:
				_carSpawnerTwo = null;
				break;
		}
	}

	private void DeActivateLastCarIfNeeded(int path)
	{
		Queue<CarWrapper> relevantQueue = path == 0 ? _carsListOne : _carsListTwo;
		if (relevantQueue.Count > 0)
		{
			CarMovement lastCar = relevantQueue.Peek().car;
			if (lastCar.FinishedMovement())
			{
				DeactivateLastCar(path);
			}
		}
	}

	private void InitializeCarDictionary()
	{
		for (int i = 0; i < carsPrefabs.Count; i++)
		{
			Stack<CarWrapper> tempStack = new();
			GameObject inactiveInstantiated = Instantiate(carsPrefabs[i]);
			inactiveInstantiated.transform.parent = transform;
			inactiveInstantiated.SetActive(false);
			tempStack.Push(new CarWrapper(i, inactiveInstantiated.GetComponent<CarMovement>()));
			_inactivePrefabs.Add(i, tempStack);
		}
	}

	private CarWrapper GetRandomCar()
	{
		int random = Random.Range(0, carsPrefabs.Count);
		CarWrapper result = _inactivePrefabs[random].Pop();
		FillDictionaryStackIfNeeded(random);
		return result;
	}

	private void FillDictionaryStackIfNeeded(int dictionaryKey)
	{
		if (_inactivePrefabs[dictionaryKey].Count == 0)
		{
			GameObject inactiveInstantiated = Instantiate(carsPrefabs[dictionaryKey]);
			inactiveInstantiated.transform.parent = transform;
			inactiveInstantiated.SetActive(false);
			_inactivePrefabs[dictionaryKey].Push(new CarWrapper(dictionaryKey,
				inactiveInstantiated.GetComponent<CarMovement>()));
		}
	}

	private void InitializeStartingCars()
	{
		InitializeStartingCarsPath(0);
		InitializeStartingCarsPath(1);
	}

	private void InitializeStartingCarsPath(int path)
	{
		for (int i = 0; i < startingCarsPerPath; i++)
		{
			CarWrapper newCar = GetRandomCar();
			newCar.car.gameObject.SetActive(true);
			newCar.car.SetPath(paths[path]);
			newCar.car.InitBlowerHead(blowerHead);
			newCar.car.SetStartingPos((float)(startingCarsPerPath - i - 1) / startingCarsPerPath);
			switch (path)
			{
				case 0:
					_carsListOne.Enqueue(newCar);
					break;
				case 1:
					_carsListTwo.Enqueue(newCar);
					break;
			}
		}
	}

	private void DeactivateAllCars()
	{
		while (_carsListOne.Count > 0)
		{
			DeactivateLastCar(0);
		}
		while (_carsListTwo.Count > 0)
		{
			DeactivateLastCar(1);
		}
	}

	private void DeactivateLastCar(int path)
	{
		CarWrapper lastCar;
		switch (path)
		{
			case 0:
				lastCar = _carsListOne.Dequeue();
				break;
			case 1:
				lastCar = _carsListTwo.Dequeue();
				break;
			default:
				return;
		}
		lastCar.car.gameObject.SetActive(false);
		_inactivePrefabs[lastCar.index].Push(lastCar);
	}

	private void AddCarToStart(int path)
	{
		CarWrapper newCar = GetRandomCar();
		switch (path)
		{
			case 0:
				_carsListOne.Enqueue(newCar);
				break;
			case 1:
				_carsListTwo.Enqueue(newCar);
				break;
		}
		newCar.car.SetPath(paths[path]);
		newCar.car.InitBlowerHead(blowerHead);
		newCar.car.SetStartingPos(0f);
		newCar.car.gameObject.SetActive(true);
	}
}