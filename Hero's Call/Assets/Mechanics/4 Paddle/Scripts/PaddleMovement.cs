using System;
using UnityEngine;

public class PaddleMovement : MonoBehaviour
{
    private const float LOWER_VELOCITY_MULTIPLIER = 0.95f;
    public float speed = 1;
    private Rigidbody2D _physics;
    private bool _left;
    private bool _right;

    private void Start()
    {
        _physics = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _left = Input.GetKey(KeyCode.LeftArrow);
        _right = Input.GetKey(KeyCode.RightArrow);
    }

    private void FixedUpdate()
    {
        if ((!_left && !_right) || (_left && _right))
        {
            _physics.velocity *= LOWER_VELOCITY_MULTIPLIER;
            return;
        }
        
        if (_left)
        {
            _physics.AddForce(new Vector2(-speed, 0));
        }

        if (_right)
        {
            _physics.AddForce(new Vector2(speed, 0));
        }
    }
}
