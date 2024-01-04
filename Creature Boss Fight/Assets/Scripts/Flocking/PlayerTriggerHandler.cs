using UnityEngine;

namespace Flocking
{
	public class PlayerTriggerHandler : MonoBehaviour
	{
		[SerializeField] private int healthPerPickup = 2;
		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Pickup"))
			{
				GameManager.Shared.IncreaseHealth(healthPerPickup);
				other.gameObject.SetActive(false);
			}
		}
	}
}
