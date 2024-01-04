using System.Collections;
using PathCreation;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
	[SerializeField] private float speed = 1f; // Speed of rotation
	[SerializeField] private Transform blowerHead;
	[SerializeField] private float minDistToHead = 5f;
	[SerializeField] private float blowForce = 100f;
	[SerializeField] private float deActiveDelay = 10f;
	[SerializeField] private float rotationForce;
	[SerializeField] private AnimationCurve forceFallOff;
	private AudioSource _carHonk;
	private Rigidbody _rigidbody;
	private bool _blowerActivated;
	private bool _blownAway;
	private bool _finishedDelay;
	private bool _finishedMovement;
	private float _timeSinceBlownAway = 0f;
	private Vector3 _randomTorque;
	private WaitForSeconds _delayBeforeReturnToPool;

	private PathCreator _path;
	private float _distanceTravelled;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_delayBeforeReturnToPool = new WaitForSeconds(deActiveDelay);
		_blowerActivated = false;
		_blownAway = false;
		_randomTorque = Random.insideUnitSphere * rotationForce;
		_carHonk = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.Space)) _blowerActivated = true;
		else _blowerActivated = false;
		var dirToPos = (blowerHead.position - transform.position).normalized;
		var headForward = blowerHead.forward;
		var angleToPos = Vector3.Angle(new Vector3(headForward.x, 0, headForward.z),
			new Vector3(dirToPos.x, 0, dirToPos.z));
		var distToHead = Vector3.Distance(new Vector3(blowerHead.position.x, 0, blowerHead.position.z),
			new Vector3(transform.position.x, 0, transform.position.z));
		if (distToHead <= minDistToHead && angleToPos <= 45 && _blowerActivated)
		{
			_blownAway = true;
			var dir = (transform.position - blowerHead.position).normalized;
			_rigidbody.AddForce(dir * (Time.deltaTime * blowForce));
			_rigidbody.AddTorque(_randomTorque * Time.deltaTime);
			_carHonk.Play();
			StartCoroutine(DeactivateTimer());
		}
		else if (!_blownAway)
		{
			Move();
		}
		if (_blownAway)
		{
			var dir = (transform.position - blowerHead.position).normalized;
			_rigidbody.AddForce(dir * (Time.deltaTime * blowForce * forceFallOff.Evaluate(_timeSinceBlownAway)));
			_rigidbody.AddTorque(_randomTorque * (Time.deltaTime * forceFallOff.Evaluate(_timeSinceBlownAway)));
			_timeSinceBlownAway += Time.deltaTime;
		}
	}

	private IEnumerator DeactivateTimer()
	{
		yield return _delayBeforeReturnToPool;
		_carHonk.Stop();
		_finishedDelay = true;
	}

	private void Move()
	{
		if (_distanceTravelled > _path.path.length)
		{
			_finishedMovement = true;
		}
		else
		{
			_distanceTravelled += speed * Time.deltaTime;
			_rigidbody.MovePosition(_path.path.GetPointAtDistance(_distanceTravelled, EndOfPathInstruction.Stop));
			_rigidbody.MoveRotation(_path.path.GetRotationAtDistance(_distanceTravelled, EndOfPathInstruction.Stop));			
		}
	}

	private void OnEnable()
	{
		_blownAway = false;
		_finishedMovement = false;
		_distanceTravelled = 0;
		_timeSinceBlownAway = 0;
		_finishedDelay = false;
		_rigidbody.velocity = Vector3.zero;
		_rigidbody.angularVelocity = Vector3.zero;
		_carHonk.pitch = Random.Range(0.8f, 1f);
		_carHonk.volume = Random.Range(0.1f, 0.2f);
	}

	public void SetPath(PathCreator path)
	{
		_path = path;
	}

	public void SetStartingPos(float pos)
	{
		transform.position = _path.path.GetPointAtTime(pos);
		_distanceTravelled += _path.path.length * pos;
	}

	public bool FinishedMovement()
	{
		return _finishedDelay || _finishedMovement;
	}

	public void InitBlowerHead(Transform playerBlower)
	{
		blowerHead = playerBlower;
	}

	
}