using UnityEngine;

public class PlayerEating : MonoBehaviour
{
	[SerializeField] private GameObject applesManager;
	private PlayerOneManager _manager;

	private void Start()
	{
		_manager = GetComponent<PlayerOneManager>();
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.CompareTag("Apple"))
		{
			applesManager.GetComponent<ApplesManager>().CheckItemCollected(col);
			col.tag = "Moving Apple";
		}
		if (col.CompareTag("Eating Station"))
		{
			StartCoroutine(_manager.EatFollowers());
		}
	}
}
