using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FallingBlockSpawner : MonoBehaviour
{
	// private const float MOVE_RIGHT = 22.304f;
	// private const float MOVE_LEFT = -7.913f;

	[SerializeField] private GameObject bottomAppleRightSpawnLoc;
	[SerializeField] private GameObject bottomAppleLeftSpawnLoc;

		
	[SerializeField] private GameObject fallingBlockPrefab;
	[SerializeField] private Vector3 bottomBlockPosition;
	[SerializeField] private float queueDiffY;
	[SerializeField] private PlayerOneManager playerManager;
	[SerializeField] private float decreaseSizeVegetablesForQueue = 2.5f;
	[SerializeField] private Transform coachHand;
	private readonly Queue<FallingObject> _blockQueue = new();
	private readonly List<FallingObject> _blocks = new();
	private readonly List<FallingObject> _sentBlocks = new();
	private FallingObject _activeFallingBlock;
	private const int QUEUE_SIZE = 3;
	private int _count = 0;
	private bool _pause;
	private const string DORMANT_BLOCK = "Dormant Block";
	private const string ACTIVE_BLOCK = "Falling Blocks";
	private const float VEGETABLE_SPAWN_LOCATION_OFFSET = 1.5f;

	private void Start()
	{
		InitializeRound();
	}

	/**
	 * Activates the player's (which controls the vegetables) controls.
	 */
	public void ActivatePlayerKeys()
	{
		_activeFallingBlock.tag = ACTIVE_BLOCK;
	}

	private void InitializeRound()
	{
		InitializeBlockQueue();
		_activeFallingBlock = GenerateRandomBlock(true, false);
	}

	private FallingObject GenerateRandomBlock(bool isDormant, bool lowerScale)
	{
		GameObject result = Instantiate(fallingBlockPrefab, transform.position, Quaternion.identity);
		FallingObject fallingObject = result.GetComponent<FallingObject>();
		_blocks.Add(fallingObject);
		result.tag = isDormant ? DORMANT_BLOCK : ACTIVE_BLOCK;
		result.name = "Newly created block " + _count;
		if(lowerScale) result.transform.localScale /= decreaseSizeVegetablesForQueue;
		++_count;
		return fallingObject;
	}

	public void MoveToRightSide()
	{
		bottomBlockPosition = bottomAppleRightSpawnLoc.transform.position;
	}
	
	public void MoveToLeftSide()
	{
		bottomBlockPosition = bottomAppleLeftSpawnLoc.transform.position;
	}
	

	private void Update()
	{
		if (playerManager.IsDead() && !_pause) // runs the first time the player died
		{
			_pause = true;
		}
		else if (_pause && !playerManager.IsDead()) // runs after the player revived 
		{
			foreach (FallingObject sentBlock in _sentBlocks)
			{
				sentBlock.gameObject.SetActive(false);
			}
			_pause = false;
		}
		if (!_pause && _activeFallingBlock.CompareTag("BottomBound"))
		{  // Generates a new block after the last vegetable hit the floor/another vegetable.
			GenerateNewBlock();
		}
		if (_activeFallingBlock.enabled == false)
		{ // This should not happen
			GenerateNewBlock();
		}
	}

	/**
	 * Advances the queue
	 */
	private void GenerateNewBlock()
	{
		// Get the first in queue to be ready to fall
		FallingObject topObject = _blockQueue.Dequeue();
		_activeFallingBlock = topObject;
		_activeFallingBlock.transform.position = coachHand.position + Vector3.down * VEGETABLE_SPAWN_LOCATION_OFFSET;
		_activeFallingBlock.tag = ACTIVE_BLOCK;
		_activeFallingBlock.transform.localScale *= decreaseSizeVegetablesForQueue;
		// Get a new block in the queue and move it up as required
		_blockQueue.Enqueue(GenerateRandomBlock(true, true));
		ReorganizeQueue();
	}

	private void ReorganizeQueue()
	{
		int i = 0;
		foreach (FallingObject block in _blockQueue.Reverse())
		{
			ChooseType chooseType = block.gameObject.GetComponent<ChooseType>();
			chooseType.SetForm(i > 1);
			block.transform.position = new Vector3(bottomBlockPosition.x, bottomBlockPosition.y + (queueDiffY * i),
				bottomBlockPosition.z);
			i++;
		}
	}

	private void InitializeBlockQueue()
	{
		for (int i = 0; i < QUEUE_SIZE; i++)
		{
			Vector3 newBlockLocation = new Vector3(bottomBlockPosition.x, bottomBlockPosition.y + (queueDiffY * i),
				bottomBlockPosition.z);
			GameObject newFallingBlock = Instantiate(fallingBlockPrefab, newBlockLocation, Quaternion.identity);
			FallingObject newFallingObject = newFallingBlock.GetComponent<FallingObject>();
			newFallingBlock.tag = DORMANT_BLOCK;
			newFallingBlock.name = "Queue Block " + i;
			newFallingBlock.transform.localScale /= decreaseSizeVegetablesForQueue;
			_blocks.Add(newFallingObject);
			_blockQueue.Enqueue(newFallingObject);
			ReorganizeQueue();
		}
	}

	public void AddBlockToSentBlocks(GameObject block)
	{
		_sentBlocks.Add(block.GetComponent<FallingObject>());
	}

	public void SetSentBlocksInactive()
	{
		foreach (FallingObject sentBlock in _sentBlocks)
		{
			sentBlock.gameObject.SetActive(false);
		}
		if (_activeFallingBlock.IsFalling() && _activeFallingBlock.CompareTag("Falling Blocks"))
		{
			GenerateNewBlock();
		}
	}

	public Transform GetActiveVegetableTransform()
	{
		return _activeFallingBlock.transform;
	}

	public bool IsActiveBlockFalling()
	{
		return _activeFallingBlock.IsFalling();
	}
}