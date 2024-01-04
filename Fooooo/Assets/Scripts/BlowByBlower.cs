using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlowByBlower : MonoBehaviour
{
	private bool _blowerActivated;
	private bool _blownAway;
	private Rigidbody _rigidbody;
	[SerializeField] private float blowForce;
	[SerializeField] private Transform blowerHead;
	[SerializeField] private float minDistToHead;
	[SerializeField] private float waitBeforeDestory;
	[SerializeField] private float rotationForce;
	[SerializeField] private AnimationCurve forceFallOff;
	[SerializeField] private float grannyGravity;
	[SerializeField] private bool gameOverIfFalling = false;
	private float _timeSinceEnabled;
	private Vector3 _randomTorque;
	private float _timeSinceBlownAway = 0f;
	private Vector3 _origPos;
	private Quaternion _origRotation;
	private const int GRANNY_STAGE = 2;
	private bool _isGrannyActive = true;
	private bool _didLeafWinStage;
	[SerializeField] private float nonGrannyGravity;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_randomTorque = Random.insideUnitSphere * rotationForce;
		_origPos = transform.position;
		_origRotation = transform.rotation;
	}

	private void Start()
	{
		if (CompareTag("Granny"))
		{
			_isGrannyActive = false;
			StartCoroutine(DisableGrannyUntilReachedStage());
		}
	}

	private IEnumerator DisableGrannyUntilReachedStage()
	{
		_rigidbody.isKinematic = true;
		while (GameManager.Shared.stage != GRANNY_STAGE)
		{
			yield return null;
		}
		_rigidbody.isKinematic = false;
		_isGrannyActive = true;
	}

	private void FixedUpdate()
	{
		if (!_isGrannyActive) return;
		_timeSinceEnabled += Time.deltaTime;
		if (Input.GetKey(KeyCode.Space))
		{
			if (_timeSinceEnabled > 1f)
			{
				_blowerActivated = true;
			}
		}
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
			// dir.y += 0.1f;
			_rigidbody.AddForce(dir * (blowForce));
			if (!gameObject.CompareTag("Granny") && !gameObject.CompareTag("Leaf") &&
			    !gameObject.CompareTag("Last Stage Text"))
			{
				_rigidbody.AddTorque(_randomTorque);
				StartCoroutine(WaitBeforeDestroy());
			}
		}
		if (_blownAway)
		{
			var dir = (transform.position - blowerHead.position).normalized;
			_rigidbody.AddForce(dir * (blowForce * forceFallOff.Evaluate(_timeSinceBlownAway)));
			if (!gameObject.CompareTag("Granny") && !gameObject.CompareTag("Leaf"))
			{
				_rigidbody.AddTorque(_randomTorque * (forceFallOff.Evaluate(_timeSinceBlownAway)));
				_timeSinceBlownAway += Time.deltaTime;
			}
		}
		if (CompareTag("Granny") || CompareTag("Leaf"))
		{
			_rigidbody.velocity += grannyGravity * Vector3.down;
		}
		else
		{
			_rigidbody.velocity += nonGrannyGravity * Vector3.down;
		}
	}

	IEnumerator WaitBeforeDestroy()
	{
		yield return new WaitForSeconds(waitBeforeDestory);
		gameObject.SetActive(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!gameOverIfFalling && !CompareTag("Leaf")) return;
		if (other.CompareTag("downBlock"))
		{
			if (CompareTag("Granny") && !GameManager.Shared.IsAdvancingStage())
			{
				GameManager.Shared.GameOver();
			}
			if (CompareTag("Leaf") && !_didLeafWinStage)
			{
				GameManager.Shared.LevelComplete();
				_didLeafWinStage = true;
			}
		}
	}

	private void OnEnable()
	{
		if (!CompareTag("Last Stage Text"))
		{
			_rigidbody.position = _origPos;
			_rigidbody.rotation = _origRotation;	
		}
		_rigidbody.velocity = Vector3.zero;
		_rigidbody.angularVelocity = Vector3.zero;
		_blownAway = false;
		_blowerActivated = false;
		_timeSinceEnabled = 0;
		_didLeafWinStage = false;
	}
}