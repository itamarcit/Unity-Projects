using System.Collections;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	private CinemachineTargetGroup _targetGroup;
	[Tooltip("The time it takes the camera to switch between focusing the player to focusing the friendly ghost.")]
	[SerializeField]
	private float cameraSwitchTime = 2f;

	private void Awake()
	{
		_targetGroup = GetComponent<CinemachineTargetGroup>();
	}

	public IEnumerator SwapCameraFocusToGhost()
	{
		float timeElapsed = 0;
		float lerpDuration = cameraSwitchTime;
		while (timeElapsed < lerpDuration)
		{
			_targetGroup.m_Targets[0].weight = Mathf.Lerp(1, 0, timeElapsed / lerpDuration);
			_targetGroup.m_Targets[1].weight = Mathf.Lerp(0, 1, timeElapsed / lerpDuration);
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		_targetGroup.m_Targets[0].weight = 0;
		_targetGroup.m_Targets[1].weight = 1;
	}
}