using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
	public class PickupSpawner : MonoBehaviour
	{
		[SerializeField] private List<GameObject> pickupPrefabs;
		[SerializeField] private Collider groundCollider;
		[SerializeField] private Transform playerTransform;
		[Tooltip("Will spawn X seconds after last pickup is picked up.")] [SerializeField]
		private float pickupSpawnTimer = 10f;
		[Tooltip("The minimum distance the pickup will spawn from the player.")] [SerializeField]
		private float minDistanceToPlayer;
		private readonly List<GameObject> _pickupPool = new();
		private GameObject _activePickup;
		private float _pickupTimer;
		private bool _tryingToSpawnPickup = false;

		private void Awake()
		{
			for (int i = 0; i < pickupPrefabs.Count; i++)
			{
				GameObject pickup = Instantiate(pickupPrefabs[i]);
				pickup.SetActive(false);
				_pickupPool.Add(pickup);
			}
			_activePickup = _pickupPool[Random.Range(0, _pickupPool.Count)];
			_pickupTimer = pickupSpawnTimer;
		}

		private void Update()
		{
			if (_tryingToSpawnPickup) return;
			if (_pickupTimer <= 0)
			{
				_tryingToSpawnPickup = true;
				StartCoroutine(SpawnPickup());
				_pickupTimer = pickupSpawnTimer;
			}
			else
			{
				_pickupTimer -= Time.deltaTime;
			}
		}

		private IEnumerator SpawnPickup()
		{
			if (_activePickup.activeSelf) _activePickup.SetActive(false);
			_activePickup = _pickupPool[Random.Range(0, _pickupPool.Count)];
			Vector3 randomSpawnPos = GetRandomPointInArena();
			while (Vector3.Distance(randomSpawnPos, playerTransform.position) < minDistanceToPlayer)
			{
				randomSpawnPos = GetRandomPointInArena();
				yield return null;
			}
			_activePickup.transform.position = randomSpawnPos;
			_activePickup.SetActive(true);
			_tryingToSpawnPickup = false;
		}

		private Vector3 GetRandomPointInArena()
		{
			Bounds bounds = groundCollider.bounds;
			return new Vector3(Random.Range(bounds.min.x, bounds.max.x), pickupPrefabs[0].transform.position.y,
				Random.Range(bounds.min.z, bounds.max.z));
		}
	}
}