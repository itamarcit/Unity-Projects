using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ResetScript : MonoBehaviour
{
    private Rigidbody2D _physics;
    public float gravity = 1;
    private bool _isFalling;
    private Vector2 _originalPos;
    
    private void Awake()
    {
        _physics = GetComponent<Rigidbody2D>();
        _originalPos = _physics.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _isFalling = !_isFalling;
        }
    }

    private void FixedUpdate()
    {
        if (!_isFalling)
        {
            ResetMovement();
        }
        else
        {
            _physics.gravityScale = gravity;
        }
    }

    private void ResetMovement()
    {
        _physics.gravityScale = 0;
        _physics.velocity = Vector2.zero;
        _physics.position = _originalPos;
    }
}
