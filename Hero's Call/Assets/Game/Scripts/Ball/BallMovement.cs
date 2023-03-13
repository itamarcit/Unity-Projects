using UnityEngine;

public class BallMovement : MonoBehaviour
{
    private Rigidbody2D _physics;
    private Vector2 _prevVelocity;
    private Vector2 _originalPosition;
    [SerializeField] private float startSpeed = 4;
    [SerializeField] private float lowSpeed = 5;
    [SerializeField] private float mediumSpeed = 6;
    [SerializeField] private float highSpeed = 7;
    [SerializeField] private GameObject arrow;

    private float _lastY;
    
    public enum Speed
    {
        Zero,
        Start,
        Low,
        Medium,
        Max
    }

    private Speed _activeSpeed;

    public void StartMovement()
    {
        _physics.velocity = arrow.transform.up;
        _activeSpeed = Speed.Start;
        arrow.SetActive(false);
        _prevVelocity = _physics.velocity;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        _physics = GetComponent<Rigidbody2D>();
        var position = _physics.position;
        _originalPosition = position;
        _lastY = position.y - 1;
    }

    private void Update()
    {
        _prevVelocity = _physics.velocity;
    }


    private void OnCollisionEnter2D(Collision2D col)
    {
        ContactPoint2D contact = col.contacts[0];
        Vector2 contactNormal = contact.normal;
        FixCollisionVelocity(contactNormal);
        Vector2 newVelocity = Vector2.Reflect(_prevVelocity * (1+Mathf.Epsilon), contactNormal);
        _physics.velocity = newVelocity;
    }

    private void FixCollisionVelocity(Vector2 contactNormal)
    {
        float angleDeg = Vector2.SignedAngle(contactNormal, -_prevVelocity);
        float absDeg = Mathf.Abs(angleDeg);
        float clampedDeg = Mathf.Clamp(absDeg, 15, 70);
        if (Mathf.Approximately(absDeg, clampedDeg))
        {
            return;
        }

        float signedRad = clampedDeg * Mathf.Sign(angleDeg) * Mathf.Deg2Rad;
        float normalRad = Mathf.Atan2(contactNormal.y, contactNormal.x);
        float newRad = normalRad + signedRad;
        Vector2 newDir = new Vector2(Mathf.Cos(newRad), Mathf.Sin(newRad));
        float magnitude = _prevVelocity.magnitude;
        _prevVelocity = newDir * -magnitude;
    }

    private void FixedUpdate()
    {
        if (GameManager.Shared.DidGameStart())
        {
            _physics.velocity = GetSpeed(_activeSpeed) * _physics.velocity.normalized;
            FixYStallIssue();
        }
    }

    /**
     * This code is intended to fix the problem we have when the ball has
     * a vector which is directly on the x-axis. In that case the ball is stuck,
     * and this function should fix it by changing the direction of the vector in such cases.
     */
    private void FixYStallIssue()
    {
        float diffY = Mathf.Abs(_lastY - _physics.position.y);
        _lastY = _physics.position.y;
        if (diffY < Mathf.Epsilon)
        {
            _physics.velocity += Vector2.up * 0.01f;
        }
    }

    public void ResetBall()
    {
        gameObject.SetActive(true);
        arrow.SetActive(true);
        _physics.velocity = Vector2.zero;
        SetSpeed(Speed.Zero);
        _prevVelocity = Vector2.zero;
        _physics.position = _originalPosition;
    }

    private float GetSpeed(Speed speed)
    {
        switch (speed)
        {
            case Speed.Zero:
                return 0;
            case Speed.Start:
                return startSpeed;
            case Speed.Low:
                return lowSpeed;
            case Speed.Medium:
                return mediumSpeed;
            default:
                return highSpeed;
        }
    }

    public void SetSpeed(Speed speed)
    {
        _activeSpeed = speed;
    }
}