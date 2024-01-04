using System.Collections;
using UnityEngine;

public class PegonMovement : MonoBehaviour
{
    [SerializeField] private Transform blowerHead;
    [SerializeField] private float minDistToHead = 5f;
    [SerializeField] private float blowForce = 5f;
    [SerializeField] private float upForce = 1f;
    [SerializeField] private float _newGravity;
    private Rigidbody _rigidbody;
    private Animator _animator;

    private bool blowerActivated;

    // Start is called before the first frame update
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        StartCoroutine(RandomAnimActivator());
        
        float randomRotationY = Random.Range(-360f, 360f);
        
        // Apply the random rotation to the object's y-axis rotation
        Vector3 rotation = transform.eulerAngles;
        rotation.y = randomRotationY;
        transform.eulerAngles = rotation;
    }


    private IEnumerator RandomAnimActivator()
    {
        while (true)
        {
            float waitBeforeActivation = Random.Range(0, 10f);
            yield return new WaitForSeconds(waitBeforeActivation);
            _animator.Play("LookAround", -1, 0);
        }
    }
    

    // Update is called once per frame
    private void Update()
    {
        blowerActivated = Input.GetKey(KeyCode.Space);
        var dirToPos = (blowerHead.position - transform.position).normalized;
        var headForward = blowerHead.forward;
        var angleToPos = Vector3.Angle(new Vector3(headForward.x, 0, headForward.z),
            new Vector3(dirToPos.x, 0, dirToPos.z));
        var distToHead = Vector3.Distance(new Vector3(blowerHead.position.x, 0, blowerHead.position.z),
            new Vector3(transform.position.x, 0, transform.position.z));
        if (distToHead <= minDistToHead && angleToPos <= 45 && blowerActivated)
        {
            var dir = (transform.position - blowerHead.position).normalized;
            var distForce = (1 / (dir.magnitude));
            dir.y += upForce;
            _rigidbody.AddForce(dir * (Time.deltaTime * distForce * blowForce));
        }

        if (_rigidbody.velocity.y != 0)
        {
            _rigidbody.velocity -= new Vector3(0,_newGravity, 0) * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            _rigidbody.velocity = Vector3.zero;
        }
    }
}
