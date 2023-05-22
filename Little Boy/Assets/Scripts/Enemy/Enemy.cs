using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class Enemy : MonoBehaviour
{
    [FormerlySerializedAs("rigidbody")] [SerializeField] private Rigidbody2D enemyRigidBody;
    [SerializeField] private float speed;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [FormerlySerializedAs("_animator")] [SerializeField] private Animator animator;
    [SerializeField] private Collider2D _enemyCollider;
    private Transform _player;
    private bool _playerDetected;
    private bool _moving;
    private bool _canMove;

    private void Awake()
    {
        _enemyCollider = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (_playerDetected && _canMove)
        {
            Move();
        }
        else if (_moving)
        {
            Slow();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            _playerDetected = true;
            _player = col.transform;
            StartCoroutine(PlayerDetection());
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            _playerDetected = false;
            _player = null;
            StopCoroutine(PlayerDetection());
        }
    }
    

    private void HitByRay()
    {
        StartCoroutine(Death());
    }


    private void Move()
    {
        Vector3 targetDirection = (_player.position - transform.position).normalized;
        // Quaternion targetRotation = Quaternion.LookRotation(transform.forward, targetDirection);
        // Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        // rigidbody.SetRotation(rotation);
        if (targetDirection.x < 0) spriteRenderer.flipX = true;
        else if (targetDirection.x > 0) spriteRenderer.flipX = false;
        enemyRigidBody.velocity = targetDirection * speed;
    }

    private IEnumerator PlayerDetection()
    {
        animator.Play("PlayerDetected");
        yield return new WaitForSeconds(1f);
        animator.Play("Empty");
        _canMove = true;
    }

    private IEnumerator Death()
    {
        
        _canMove = false;
        _enemyCollider.enabled = false;
        Stop();
        yield return CharacterUtils.Dissolve(spriteRenderer);
        gameObject.SetActive(false);
    }

    private void Slow()
    {
        enemyRigidBody.velocity = Vector2.MoveTowards(enemyRigidBody.velocity, Vector2.zero, Time.deltaTime);
    }

    public void Stop()
    {
        if (gameObject.activeSelf)
        {
            enemyRigidBody.velocity = Vector2.zero;
        }
    }
    
}
