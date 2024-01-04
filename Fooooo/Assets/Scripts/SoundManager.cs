using System;
using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public static SoundManager Shared;
	[SerializeField] private AudioSource musicSource;
	[SerializeField] private AudioSource effectSource;
	[SerializeField] private AudioSource blowerSource;
	[SerializeField] private AudioSource engineSource;
	[SerializeField] private AudioClip gameOverClip;

	private bool _playingBlower;
	private Coroutine _fadeOutBlowerCoroutine;
	private float _blowerOrigVolume;

	private void Awake()
	{
		if (Shared == null)
		{
			Shared = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		_blowerOrigVolume = engineSource.volume;
	}

	private void Update()
	{
		if (Time.timeScale == 0)
		{
			if(blowerSource.isPlaying) blowerSource.Stop();
			return;
		}
		if (Input.GetKeyDown(KeyCode.Space) && Time.timeScale > 0)
		{
			if(_fadeOutBlowerCoroutine != null) StopCoroutine(_fadeOutBlowerCoroutine);
			blowerSource.volume = _blowerOrigVolume;
			_fadeOutBlowerCoroutine = null;
			_playingBlower = true;
			blowerSource.Play();
		}
		if (Input.GetKeyUp(KeyCode.Space) && _playingBlower && _fadeOutBlowerCoroutine == null)
		{
			_playingBlower = false;
			_fadeOutBlowerCoroutine = StartCoroutine(FadeOut(1f));
		}
	}

	public IEnumerator PlayGameOverEffect()
	{
		musicSource.volume /= 2f;
		effectSource.PlayOneShot(gameOverClip);
		yield return null;
		yield return WaitForSecondsOrRestartStage(gameOverClip.length);
		musicSource.volume *= 2f;
	}

	private IEnumerator WaitForSecondsOrRestartStage(float seconds)
	{
		float timer = seconds;
		while (timer > 0 && Time.timeScale == 0)
		{
			timer -= Time.unscaledDeltaTime;
			yield return null;
		}
	}

	public void PlayEngine()
	{
		if(!engineSource.isPlaying)
			engineSource.Play();
	}

	public void StopEngine()
	{
		if(engineSource.isPlaying)
			engineSource.Stop();
	}

	private IEnumerator FadeOut(float fadeTime)
	{
		float startVolume = blowerSource.volume;
		while (blowerSource.volume > 0)
		{
			if (_playingBlower)
			{
				blowerSource.volume = _blowerOrigVolume;
				_fadeOutBlowerCoroutine = null;
				yield break;
			}
			blowerSource.volume -= startVolume * Time.unscaledDeltaTime / fadeTime;
			yield return null;
		}
		if (_playingBlower)
		{
			blowerSource.volume = _blowerOrigVolume;
			_fadeOutBlowerCoroutine = null;
			yield break;
		}
		blowerSource.Stop();
		blowerSource.volume = startVolume;
	}
}