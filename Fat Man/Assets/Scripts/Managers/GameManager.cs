using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	private const float TIME_TO_SPAWN_PLAYER1 = 4f;
	[SerializeField] private PlayerOneManager playerOne;
	[SerializeField] private GameObject playerSpawnStartPosition;
	[SerializeField] private FallingBlockSpawner blockSpawner;
	[SerializeField] private GameObject timer;
	private const string LOTTERY = "Lottery";

	public static GameManager Shared { get; private set; }

	private Vector3 _targetPos;

	private void Awake()
	{
		if (Shared == null) Shared = this;
		else Destroy(gameObject);
		_targetPos = playerOne.transform.position;
		playerOne.transform.position = playerSpawnStartPosition.transform.position;
	}

	private void Start()
	{
		StartCoroutine(StartGame());
	}

	private IEnumerator StartGame()
	{
		yield return SmoothLerp(TIME_TO_SPAWN_PLAYER1);
		playerOne.ResetPlayer(false);
		timer.GetComponent<Timer>().StartTimer();
	}

	//https://answers.unity.com/questions/1501234/smooth-forward-movement-with-a-coroutine.html
	private IEnumerator SmoothLerp(float time)
	{
		Collider2D player1Collider = playerOne.GetComponent<Collider2D>();
		Vector3 startingPos = playerSpawnStartPosition.transform.position;
		Vector3 finalPos = _targetPos;
		float elapsedTime = 0;
		player1Collider.enabled = false;
		while (elapsedTime < time)
		{
			playerOne.transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		player1Collider.enabled = true;
		blockSpawner.ActivatePlayerKeys();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
		if (Input.GetKeyDown(KeyCode.Minus))
		{
			SceneManager.LoadScene(LOTTERY);
		}
	}

	public void ResetSentVegetables()
	{
		blockSpawner.SetSentBlocksInactive();
	}

	public void AddBlockToSentBlocks(GameObject block)
	{
		blockSpawner.AddBlockToSentBlocks(block);
	}

	public Transform GetActiveVegetableTransform()
	{
		return blockSpawner.GetActiveVegetableTransform();
	}
	
	public bool IsActiveBlockFalling()
	{
		return blockSpawner.IsActiveBlockFalling();
	}

	public bool IsPlayerDead()
	{
		return playerOne.IsDead();
	}
}