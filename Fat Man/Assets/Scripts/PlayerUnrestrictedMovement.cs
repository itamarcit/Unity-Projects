using UnityEngine;
using UnityEngine.Serialization;

public class PlayerUnrestrictedMovement : MonoBehaviour
{
	[SerializeField] private float speed = 2;
	[FormerlySerializedAs("_foodSpeedDecreasePerItem")] [SerializeField] private float foodSpeedDecreasePerItem = 0.25f;
	private float _actualSpeed;
	private bool _up;
	private bool _down;
	private bool _right;
	private bool _left;
	private Vector2 _target;
	private Rigidbody2D _rigidbody;
	private PlayerOneManager _playerManager;
	private int _lastMovement = NOTHING;
	private const int UP = 0;
	private const int DOWN = 1;
	private const int RIGHT = 2;
	private const int LEFT = 3;
	private const int NOTHING = -1;
	
	private readonly KeyCode[] _controls = new KeyCode[4];

	public void SetControls(KeyCode[] controlsParam)
	{
		for (int i = 0; i < 4; ++i)
		{
			_controls[i] = controlsParam[i];
		}
	}
	
	private void Start()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_playerManager = GetComponent<PlayerOneManager>();
		if (_playerManager == null)
		{
			Debug.LogWarning("Player one manager couldn't be found from PlayerUnrestrictedMovement.");
		}
	}

	private void Update()
	{
		if (Input.GetKey(_controls[UP])) _lastMovement = UP;
		else if (Input.GetKey(_controls[DOWN])) _lastMovement = DOWN;
		else if (Input.GetKey(_controls[LEFT])) _lastMovement = LEFT;
		else if (Input.GetKey(_controls[RIGHT])) _lastMovement = RIGHT;
		switch (_lastMovement)
		{
			case UP:
				_playerManager.SetAnimationDirection(PlayerOneManager.UP_DIRECTION);
				break;
			case DOWN:
				_playerManager.SetAnimationDirection(PlayerOneManager.DOWN_DIRECTION);
				break;
			case LEFT:
				_playerManager.SetAnimationDirection(PlayerOneManager.LEFT_DIRECTION);
				break;
			case RIGHT:
				_playerManager.SetAnimationDirection(PlayerOneManager.RIGHT_DIRECTION);
				break;
		}
		
	}

	private void FixedUpdate()
	{
		_rigidbody.velocity = Vector2.zero;
		_actualSpeed = speed - foodSpeedDecreasePerItem * _playerManager.NumberOfFollowers();
		HandleMovement();
	}


	
	private void HandleMovement()
	{
		if (_playerManager.IsDead() || _playerManager.IsFrozen()) return;
		_target = Vector2.zero;
		switch (_lastMovement)
		{
			case UP:
				_target += Vector2.up * _actualSpeed;
				break;
			case DOWN:
				_target += Vector2.down * _actualSpeed;
				break;
			case LEFT:
				_target += Vector2.left * _actualSpeed;
				break;
			case RIGHT:
				_target += Vector2.right * _actualSpeed;
				break;
		}
		_rigidbody.velocity = _target;
	}

	public void ResetMovement()
	{
		_lastMovement = NOTHING;
	}
}