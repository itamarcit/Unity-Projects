using UnityEngine;

public class Drop : MonoBehaviour
{
#region Fields
	[SerializeField] private float dropExpiryCooldown = 20f;
	private float _timer;
#endregion

#region Events
	private void Start()
	{
		_timer = dropExpiryCooldown;
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.CompareTag("Player"))
		{
			ScoreManager.Shared.AddScore(ScoreManager.Score.PickupDrop, col.gameObject.transform.position, true);
			PowerUpManager.Shared.AddEnergy(PowerUpManager.Shared.energyForDrop);
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		_timer -= Time.deltaTime;
		if (_timer <= 0)
		{
			Destroy(gameObject);
		}
	}
#endregion
}