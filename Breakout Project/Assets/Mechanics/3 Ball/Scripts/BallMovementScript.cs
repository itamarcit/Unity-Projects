using System;
using UnityEngine;

public class BallMovementScript : MonoBehaviour
{
    private Rigidbody2D _physics;
    private Vector2 _prevVelocity;

    private void Start()
    {
        _physics = GetComponent<Rigidbody2D>();
        _physics.velocity = Vector2.one.normalized;
        _prevVelocity = _physics.velocity;
    }

        private void OnCollisionExit2D(Collision2D other)
        {
            _prevVelocity = _physics.velocity;
        }
        

        private void OnCollisionEnter2D(Collision2D col)
        {
            ContactPoint2D contact = col.contacts[0];
            Vector2 contactNormal = contact.normal;
            FixCollisionVelocity(contactNormal);
            Vector2 newVelocity = Vector2.Reflect(_prevVelocity, contactNormal);
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
    }
