using System.Collections;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
#region Fields
	[SerializeField] private float fallSpeed = 2f;
	[SerializeField] private float leftRightSpeed = 2f;
	[SerializeField] private int rotationDegrees = 90;
	[SerializeField] private float destroyVegetablesTimer = 5f;
	private bool _moveLeft;
	private bool _moveRight;
	private bool _isFalling = false;
	private bool _finishedMovement = false;
	private bool _activateGravity = false;
	private Rigidbody2D _rigidbody;
	private Vector2 _direction;
	private Vector2 _target;

	private const int UP = 0;
	private const int DOWN = 1;
	private const int RIGHT = 2;
	private const int LEFT = 3;
	
	private static readonly KeyCode[] Controls = new KeyCode[4];

#endregion

	private void Start()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_direction = Vector2.zero;
		_target = _rigidbody.position;
	}

	private void OnCollisionEnter2D(Collision2D col)
	{
		if (col.collider.CompareTag("BottomBound") && _isFalling)
		{
			_finishedMovement = true;
			// Once it stops moving, it is no longer a falling block and it becomes part of the bounding box
			StartCoroutine(FallingBlockToBoundingBlock(destroyVegetablesTimer));
		}
	}

	// Changes the falling object to be part of the bounding box after given delay.
	private IEnumerator FallingBlockToBoundingBlock(float delay)
	{
		gameObject.tag = "BottomBound";
		yield return new WaitForSeconds(delay);
		gameObject.SetActive(false);
	}

	
	public void SetControls(KeyCode[] controlsParam)
	{
		for (int i = 0; i < 4; ++i)
		{
			Controls[i] = controlsParam[i];
		}
	}
	
	private void Update()
	{
		if (!_finishedMovement && gameObject.CompareTag("Falling Blocks"))
		{
			HandleRotation();
			DownMovement();
			LeftRightMovementInput();
		}
	}

	private void FixedUpdate()
	{
		if (gameObject.CompareTag("Dormant Block")) return;
		if (!_finishedMovement) // Until the block reaches the bottom
		{
			_direction = DownMovement();
			_direction += LeftRightMovement();
			_target = _direction;
			_rigidbody.velocity = _target;
		}
		else if(!_activateGravity) // Executes once after hit the bottom
		{
			_rigidbody.velocity = Vector2.zero;
			_rigidbody.mass = 5f;
			_rigidbody.gravityScale = 10f;
			_activateGravity = true;
		}
	}

	private void HandleRotation()
	{
		if (Input.GetKeyDown(Controls[UP]))
		{
			transform.Rotate(0, 0, rotationDegrees);
		}
	}

	private void LeftRightMovementInput()
	{
		if (Input.GetKey(Controls[LEFT]))
		{
			_moveLeft = true;
		}
		if (Input.GetKey(Controls[RIGHT]))
		{
			_moveRight = true;
		}
	}

	private Vector2 LeftRightMovement()
	{
		Vector2 result = Vector2.zero;
		if (_moveLeft)
		{
			result = Vector3.left * leftRightSpeed;
			_moveLeft = false;
		}
		if (_moveRight)
		{
			result = Vector3.right * leftRightSpeed;
			_moveRight = false;
		}
		return result;
	}

	private Vector2 DownMovement()
	{
		if (Input.GetKeyDown(Controls[DOWN]) && !_isFalling && !_finishedMovement && !GameManager.Shared.IsPlayerDead())
		{ // code enters this if only on the first time the down button is pressed for each object.
			_rigidbody.constraints = RigidbodyConstraints2D.None;
			GameManager.Shared.AddBlockToSentBlocks (gameObject);
			_isFalling = true;
		}
		if (_isFalling)
		{
			return Vector2.down * fallSpeed;
		}
		return Vector2.zero;
	}

	public bool IsFalling()
	{
		return _isFalling;
	}
}