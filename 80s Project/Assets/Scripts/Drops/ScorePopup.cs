using UnityEngine;

public class ScorePopup : MonoBehaviour
{
#region Fields
	[SerializeField] private float popupSpeed = 1;
	[SerializeField] private float popupLength = 2;
	private float _timer;
#endregion

#region Events
	private void Start()
	{
		_timer = popupLength;
	}

	private void Update()
	{
		transform.position = transform.position + Vector3.up * (Time.deltaTime * popupSpeed);
		_timer -= Time.deltaTime;
		if (_timer <= 0)
		{
			Destroy(gameObject);
		}
	}
#endregion
}
