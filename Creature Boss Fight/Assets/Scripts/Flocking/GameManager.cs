using System.Collections;
using ItamarRamon;
using UnityEngine;

namespace Flocking
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager Shared; // Singleton instance
		[SerializeField] private Collider playerDetectionTrigger;
		[SerializeField] private int startingPlayerHealth = 100;
		[SerializeField] private int peepsDamage = 5;
		[Tooltip("Increase the peeps damage every time the flock spawner decreases the timer.")] [SerializeField]
		private int peepsDamageAddition = 1;
		[HideInInspector] public WiggleTail monsterTail;

		private int _health;
		private int _score;
		private bool _isGameOver;

		private void Awake()
		{
			if (Shared == null)
			{
				Shared = this;
				_health = startingPlayerHealth;
			}
			else
			{
				Destroy(gameObject);
			}
		}

		private void Update()
		{
			if (_health <= 0)
			{
				_isGameOver = true;
			}
		}

		public IEnumerator ResetPlayerDetectionCollider()
		{
			playerDetectionTrigger.enabled = false;
			yield return null;
			playerDetectionTrigger.enabled = true;
		}

		public void LowerPlayerHealth()
		{
			_health -= peepsDamage;
		}

		public int GetPlayerScore()
		{
			return _score;
		}

		public void IncreaseScore()
		{
			_score++;
		}

		public int GetPlayerHealth()
		{
			return _health;
		}

		public bool IsGameOver()
		{
			return _isGameOver;
		}

		public void IncreaseHealth(int health)
		{
			_health += health;
			if (_health > startingPlayerHealth)
			{
				_health = startingPlayerHealth;
			}
			_score++;
		}

		public void IncreasePeepDamage()
		{
			peepsDamage += peepsDamageAddition;
		}
	}
}