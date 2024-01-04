using System.Collections;
using DissolveShader;
using ItamarRamon;
using UnityEngine;
using WalkingSimulator;

namespace Flocking
{
	public class PeepHitController : MonoBehaviour
	{
		[SerializeField] private float explosionForce = 200000;
		[SerializeField] private float explosionRadius = 1000;
		[SerializeField] private float disappearDelay = 3;
		private const string ENEMY_TAIL = "TailTip";
		private const float MAX_PITCH_HIT_SOUND = 1f;
		private WiggleTail enemyTail;
		private PeepMainController _peepMainController;
		private MovementController _movementController;
		private Rigidbody _rb;
		private WaitForSeconds _waitForSeconds;
		private DissolveController _dissolveController;
		private AudioSource _audioSource;
		private bool _isDisappearing;
		private float MIN_PITCH_HIT_SOUND;

		public PeepHitController()
		{
			MIN_PITCH_HIT_SOUND = 0.4f;
		}

		private void Awake()
		{
			_dissolveController = GetComponent<DissolveController>();
			_peepMainController = GetComponent<PeepMainController>();
			_movementController = GetComponent<MovementController>();
			_audioSource = GetComponent<AudioSource>();
			_rb = GetComponent<Rigidbody>();
			_waitForSeconds = new WaitForSeconds(disappearDelay);
		}

		private void Start()
		{
			enemyTail = GameManager.Shared.monsterTail;
		}

		private void OnEnable()
		{
			_isDisappearing = false;
			if (!_peepMainController.enabled) _peepMainController.enabled = true;
			if (!_movementController.enabled) _movementController.enabled = true;
			_rb.freezeRotation = true;
			_audioSource.pitch = Random.Range(MIN_PITCH_HIT_SOUND, MAX_PITCH_HIT_SOUND);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!other.CompareTag(ENEMY_TAIL)) return;
			if (!enemyTail.IsTailInAttackMode()) return;
			_peepMainController.enabled = false;
			_movementController.enabled = false;
			_rb.freezeRotation = false;
			KnockBack(other);
			if (!_isDisappearing)
			{
				_isDisappearing = true;
				StartCoroutine(DisappearWithDelay());
			}
		}

		private IEnumerator DisappearWithDelay()
		{
			_audioSource.Play();
			yield return _waitForSeconds;
			_dissolveController.SetVisibility(false);
			GameManager.Shared.IncreaseScore();
			yield return _waitForSeconds;
			gameObject.SetActive(false);
		}

		private void KnockBack(Collider other)
		{
			_rb.AddExplosionForce(explosionForce, other.transform.position, explosionRadius);
			_rb.AddExplosionForce(explosionForce / 3, transform.position, explosionRadius);
		}

		public bool IsHit()
		{
			return _isDisappearing;
		}
	}
}