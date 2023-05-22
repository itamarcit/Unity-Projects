using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostMirror : MonoBehaviour
{
	public bool IsActive { get; private set; }
	[Tooltip("Ghost transform to move once the stage is won. Object should be disabled.")] [SerializeField]
	private Transform ghostTransform;
	[Tooltip("Empty Mirror Sprite for when the ghost gets out of the mirror.")] [SerializeField]
	private Sprite emptyMirrorSprite;
	[SerializeField] private SpriteRenderer ghostMirrorRenderer;
	[SerializeField] private SpriteRenderer goodGhostRenderer;
	[SerializeField] private AudioSource _ghostAudio;
	private float _lastActive;

	void Update()
	{
		if (IsActive && Time.time - _lastActive >= 0.05)
		{
			IsActive = false;
		}
	}

	private void HitByRay()
	{
		_lastActive = Time.time;
		IsActive = true;
	}

	public IEnumerator HandleEndingAnimation(List<Vector3> pathList)
	{
		if (pathList == null) yield break;
		_ghostAudio.Play();
		EmptyMirrorAndEnableSprite();
		int i = 0;
		Vector3 startingPos = transform.position;
		foreach (Vector3 lightBreakPoint in pathList)
		{
			float timeElapsed = 0;
			float lerpDuration = 2f;
			while (timeElapsed < lerpDuration)
			{
				ghostTransform.position = Vector3.Lerp(startingPos, lightBreakPoint, timeElapsed / lerpDuration);
				timeElapsed += Time.deltaTime;
				yield return null;
			}
			ghostTransform.position = lightBreakPoint;
			startingPos = lightBreakPoint;
			i++;
		}
	}

	private void EmptyMirrorAndEnableSprite()
	{
		ghostTransform.gameObject.SetActive(true);
		ghostMirrorRenderer.sprite = emptyMirrorSprite;
		ghostMirrorRenderer.transform.localScale = Vector3.one * 3.87f;
	}

	public SpriteRenderer GetRenderer()
	{
		return goodGhostRenderer;
	}
}