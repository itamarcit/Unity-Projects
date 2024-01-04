using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
	[SerializeField] private BillboardType billboardType;

	[SerializeField] private Transform mainCamera;
	[SerializeField] private Transform moveToTransform;
	[SerializeField] private float lerpSpeed;
	private Animator _animator;
	private readonly List<Rigidbody> _rigidbodies = new();
	private readonly List<Collider> _colliders = new();
	private bool _moveToCenter;
	private bool _finishedMovement;
	private Vector3 _originalRotation;
	private Vector3 _targetPosition;
	[SerializeField] private float distanceMultiplier;
	[SerializeField] private float titleYOffset;
	[SerializeField] private Camera uiCamera;
	[SerializeField] private Camera playerCamera;
	[SerializeField] private float speedToCenter;
	private readonly List<BlowByBlower> _blowByBlower = new();

	private enum BillboardType
	{
		LookAtCamera,
		CameraForward
	}

	private void Awake()
	{
		// _originalRotation = transform.rotation.eulerAngles;
		_rigidbodies.AddRange(GetComponentsInChildren<Rigidbody>());
		_colliders.AddRange(GetComponentsInChildren<Collider>());
		_blowByBlower.AddRange(GetComponentsInChildren<BlowByBlower>());
		_animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		_targetPosition = moveToTransform.position;
		_moveToCenter = false;
		_finishedMovement = false;
		EnableAdditionalCamera();
		DisableRigidbodies();
		_animator.Rebind();
		_animator.Update(0);
		_animator.enabled = true;
		_animator.Play("Foo The Title", -1, 0f);
	}

	private void Update()
	{
		if (!_moveToCenter)
		{
			Vector3 pos = playerCamera.WorldToScreenPoint(mainCamera.transform.position + mainCamera.transform.forward * distanceMultiplier);
			pos.x = Mathf.Clamp(pos.x, 0.5f, 0.5f);
			pos.y = Mathf.Clamp(pos.y, 0.5f, titleYOffset);
			transform.position = Vector3.Lerp(transform.position, playerCamera.ViewportToWorldPoint(pos), Time.deltaTime * lerpSpeed); 
		}
		else if (_moveToCenter && !_finishedMovement)
		{
			if (Vector3.Distance(moveToTransform.position, transform.position) < 1)
			{
				_finishedMovement = true;
				ActivateRigidbodies();
				RemoveAdditionalCamera();
			}
			else
			{
				MoveToTarget();
			}
		}
	}

	private void RemoveAdditionalCamera() {
		uiCamera.cullingMask &=  ~(1 << LayerMask.NameToLayer("Last Stage Text"));
		uiCamera.cullingMask &=  ~(1 << LayerMask.NameToLayer("Player"));
	}

	private void EnableAdditionalCamera()
	{
		uiCamera.cullingMask |= 1 << LayerMask.NameToLayer("Last Stage Text");
		uiCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
	}

	private void ActivateRigidbodies()
	{
		for (var index = 0; index < _rigidbodies.Count; index++)
		{
			_colliders[index].isTrigger = false;
			_blowByBlower[index].enabled = true;
			var rigidbody1 = _rigidbodies[index];
			rigidbody1.isKinematic = false;
			rigidbody1.velocity = Vector3.zero;
			rigidbody1.angularVelocity = Vector3.zero;
		}
	}
	
	private void DisableRigidbodies()
	{
		for (var index = 0; index < _rigidbodies.Count; index++)
		{
			_colliders[index].isTrigger = true;
			_blowByBlower[index].enabled = false;
			var rigidbody1 = _rigidbodies[index];
			rigidbody1.isKinematic = true;
			rigidbody1.velocity = Vector3.zero;
			rigidbody1.angularVelocity = Vector3.zero;
		}
	}

	private void MoveToTarget()
	{
		transform.position = Vector3.MoveTowards(transform.position, _targetPosition, Time.deltaTime * speedToCenter);
	}

	private void LateUpdate()
	{
		if (!_moveToCenter)
		{
			// Vector3 targetPos = playerCamera.transform.position + _followOffset;
			// Vector3 pos = playerCamera.WorldToScreenPoint(mainCamera.transform.position + mainCamera.transform.forward * distanceMultiplier);
			// pos.x = Mathf.Clamp(pos.x, 0.5f, 0.5f);
			// pos.y = Mathf.Clamp(pos.y, 0.5f, 0.5f);
			// transform.position = playerCamera.ViewportToWorldPoint(pos);
			// Vector3 targetPos = mainCamera.transform.position + playerCamera. * distanceMultiplier;
			// targetPos.y = mainCamera.position.y - titleYOffset;
			// targetPos.x = mainCamera.position.x - titleXOffset;
			// transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * lerpSpeed);
			switch (billboardType)
			{
				case BillboardType.LookAtCamera:
					transform.LookAt(mainCamera.transform.position, Vector3.up);
					break;
				case BillboardType.CameraForward:
					transform.forward = mainCamera.transform.forward;
					break;
			}
			Vector3 rotation = transform.rotation.eulerAngles;
			transform.rotation = Quaternion.Euler(rotation);
		}
	}

	public IEnumerator Appear()
	{
		EnableAdditionalCamera();
		while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1) yield return null;
		
		_animator.enabled = false;
		yield return new WaitForSeconds(1f);
		_moveToCenter = true;
	}
}