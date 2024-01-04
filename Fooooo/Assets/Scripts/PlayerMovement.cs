using System;
using System.Collections;
using CMF;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField] private float blownByCarPower = 100f;
	[SerializeField] private AdvancedWalkerController controller;
	[SerializeField] private bool isUsingWind;
	[SerializeField] private ParticleSystem windParticles;
	private bool _gotHit;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("EnterToIsland"))
		{
			GameManager.Shared.ChangeStage();
			LevelTextManager.Shared.ShowText();
			other.enabled = false;
		}
		else if (other.CompareTag("downBlock"))
		{
			StartCoroutine(GameOver(false));
		}
		else if (other.CompareTag("Cat") && !GameManager.Shared.IsAdvancingStage())
		{
			Vector3 dir = other.transform.forward;
			dir.y += 1f;
			controller.AddMomentum(Time.deltaTime * dir * blownByCarPower);
			if (!_gotHit)
			{
				StartCoroutine(GameOver(true));
				_gotHit = true;
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Car") && !GameManager.Shared.finishedCars)
		{
			Vector3 dir = collision.gameObject.transform.forward;
			dir.y += 1f;
			controller.AddMomentum(Time.deltaTime * dir * blownByCarPower);
			if (!_gotHit)
			{
				StartCoroutine(GameOver(true));
				_gotHit = true;
			}
		}
	}

	IEnumerator GameOver(bool hitByCar)
	{
		GameManager.Shared.shouldLose = true;
		if (hitByCar) yield return new WaitForSeconds(0.5f);
		else yield return new WaitForSeconds(0f);
		if ((!hitByCar || !GameManager.Shared.IsAdvancingStage()) && GameManager.Shared.shouldLose)
		{
			GameManager.Shared.GameOver();
		}
		_gotHit = false;
	}

	private void Update()
	{
		if (isUsingWind)
		{
			HandleWindParticles();
		}
	}

	private void HandleWindParticles()
	{
		if (Input.GetKey(KeyCode.Space))
		{
			if (!windParticles.isPlaying) windParticles.Play();
		}
		else
		{
			if (windParticles.isPlaying) windParticles.Stop();
		}
	}
}