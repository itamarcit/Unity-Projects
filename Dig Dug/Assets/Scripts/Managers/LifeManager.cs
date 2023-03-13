using System.Collections.Generic;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
#region Fields
	[SerializeField] private List<GameObject> lifeObjects; // This list should always be of size 3
	private const int STARTING_LIVES = 3;
	private const float REDUCE_LIFE_COOLDOWN = 3f;
	private int _lives = STARTING_LIVES;
	private float _reduceLifeTimer;
	public static LifeManager Shared { get; private set; }
#endregion
#region Events
	private void Awake()
	{
		if (Shared == null)
		{
			Shared = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		if (_reduceLifeTimer > 0) _reduceLifeTimer -= Time.deltaTime;
	}
#endregion
#region Methods
	public void LowerLife()
	{
		if (_lives <= 0 || _reduceLifeTimer > 0) return;
		if (_lives == 1)
		{
			GameManager.Shared.GameOver();
		}
		_reduceLifeTimer = REDUCE_LIFE_COOLDOWN;
		_lives--;
		lifeObjects[_lives].SetActive(false);
	}
#endregion

	public bool IsGameOver()
	{
		return _lives == 0;
	}
}