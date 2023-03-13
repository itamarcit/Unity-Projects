using UnityEngine;

public class PaddleMovementScript : MonoBehaviour
{
    [SerializeField] private float speed = 750;
    private Rigidbody2D _physics;
    private bool _left;
    private bool _right;
    private Vector2 _originalPosition;

    private void Start()
    {
        _physics = GetComponent<Rigidbody2D>();
        _originalPosition = _physics.position;
    }

    void Update()
    {
        _left = Input.GetKey(KeyCode.LeftArrow);
        _right = Input.GetKey(KeyCode.RightArrow);
    }

    private void FixedUpdate()
    {
        if (GameManager.Shared.DidGameStart())
        {
            if ((!_left && !_right) || (_left && _right))
            {
                _physics.velocity = Vector2.zero;
                return;
            }

            if (_left)
            {
                _physics.AddForce(new Vector2(-speed * Time.deltaTime, 0));
            }
            else if (_right)
            {
                _physics.AddForce(new Vector2(speed * Time.deltaTime, 0));
            }
        }
    }

    public void ResetPaddle()
    {
        gameObject.SetActive(true);
        _physics.velocity = Vector2.zero;
        _physics.position = _originalPosition;
    }
}