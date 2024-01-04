using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using CMF;
using UnityEngine.Serialization;

public class IslandCameraMove : MonoBehaviour
{
	[SerializeField] private CinemachineVirtualCamera playerCam;
	[SerializeField] private List<CinemachineVirtualCamera> nextIslandsCams;
	[SerializeField] private List<GameObject> bridges;
	//place [0] is for move from island 0 to island 1
	[SerializeField] private List<Transform> nextIslandCamPos;
	[SerializeField] private SmoothPosition posToMove;
	[SerializeField] private SmoothRotation rotToMove;
	[SerializeField] private Transform playerTransform;
	[SerializeField] private CameraController cameraController;
	[SerializeField] private Transform playerCamera;
	[SerializeField] private Camera transitionCamera;
	[SerializeField] private Rigidbody player;
	private bool _isCameraMoving;
	private Coroutine _transitionCoroutine;

	public void ShowBridge()
	{
		if (_transitionCoroutine != null) return; 
		_transitionCoroutine = StartCoroutine(MoveCameras(GameManager.Shared.stage));
	}

	public IEnumerator DisableBridgeAfterSeconds(int stage, float seconds)
	{
		yield return new WaitForSeconds(seconds);
		bridges[stage].SetActive(false);
	}

	private IEnumerator MoveCameras(int stage)
	{
		while (_isCameraMoving) yield return null;
		_isCameraMoving = true;
		GameManager.Shared.FreezePlayerMovement();
		player.constraints = RigidbodyConstraints.FreezeAll;
		bridges[stage].SetActive(true);
		yield return TransitionToLookAt(bridges[stage].transform.position, 1f, stage);
		GameManager.Shared.UnfreezePlayerMovement();
		player.constraints = RigidbodyConstraints.FreezeRotation;
		_isCameraMoving = false;
		_transitionCoroutine = null;
	}

	IEnumerator TransitionToLookAt(Vector3 worldPosition, float duration, int stage)
	{
		Vector3 startPos = playerCamera.position;
		Vector3 targetPos = nextIslandCamPos[stage].position;
		Quaternion startRot = playerCamera.rotation;
		Quaternion endRot = nextIslandCamPos[stage].rotation;
		transitionCamera.transform.position = startPos;
		transitionCamera.transform.rotation = startRot;
		transitionCamera.gameObject.SetActive(true);
		for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
		{
			transitionCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t / duration);
			transitionCamera.transform.position = Vector3.Lerp(startPos, targetPos, t / duration);
			yield return null;
		}
		transitionCamera.transform.rotation = endRot;
		transitionCamera.transform.position = targetPos;
		yield return new WaitForSeconds(2f);
		for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
		{
			transitionCamera.transform.rotation = Quaternion.Slerp(endRot, startRot, t / duration);
			transitionCamera.transform.position = Vector3.Lerp(targetPos, startPos, t / duration);
			yield return null;
		}
		transitionCamera.transform.rotation = startRot;
		transitionCamera.transform.position = startPos;
		yield return null;
		transitionCamera.gameObject.SetActive(false);
	}

	public bool IsMovingCamera()
	{
		return _isCameraMoving;
	}

	public void HideBridge()
	{
		bridges[GameManager.Shared.stage].SetActive(false);
	}

	public void DisableCameraTransition()
	{
		transitionCamera.gameObject.SetActive(false);
		if(_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
		GameManager.Shared.UnfreezePlayerMovement();
		player.constraints = RigidbodyConstraints.FreezeRotation;
		_isCameraMoving = false;
		_transitionCoroutine = null;
	}
}