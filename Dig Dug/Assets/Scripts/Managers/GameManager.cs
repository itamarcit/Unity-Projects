using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
#region Fields
	[SerializeField] private float roundCooldownTimer = 3;
	[SerializeField] private float gameOverTextDelay = 2;
	[SerializeField] private PlayerMovement player;
	[SerializeField] private List<EnemyController> enemies;
	[SerializeField] private GameObject gameOverText;
	[SerializeField] private GameObject startText;
	[SerializeField] private GameObject threeText;
	[SerializeField] private GameObject twoText;
	[SerializeField] private GameObject oneText;
	[SerializeField] private AudioSource gameThemeMusic;
	public static GameManager Shared { get; private set; }
	private int totalEnemiesLeft;
	private float _pauseTimer;
	private bool _isGameOver;
	private const float DEATH_ANIM_CUT = 1.5f;
	private const float HALF_A_SECOND = 0.5f;
	private const int WAIT_UNTIL_SET_AGAIN = 100;
#endregion

#region Events
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

	private void Update()
	{
		_pauseTimer -= Time.deltaTime;
		if (_isGameOver && Input.GetKeyDown(KeyCode.Space))
		{
			_isGameOver = false;
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		else if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
#endregion
#region Methods
	public float GetGamePausedTimer()
	{
		if (_isGameOver)
		{
			return 1;
		}
		return _pauseTimer;
	}

	public void KillPlayerByRock()
	{
		LifeManager.Shared.LowerLife();
		player.KillPlayerByRock();
		Animator anim = player.GetAnimator();
		StartCoroutine(PauseGameUntilAnimFinished(anim));
	}

	public void KillPlayerByEnemy()
	{
		LifeManager.Shared.LowerLife();
		player.KillPlayerByEnemy();
		Animator anim = player.GetAnimator();
		StartCoroutine(PauseGameUntilAnimFinished(anim));
	}

	public void KillEnemyByRock(GameObject enemy)
	{
		ScoreManager.Shared.AddScore(ScoreManager.Score.KillByRock, enemy.transform.position, true);
		EnemyController enemyController = enemy.GetComponent<EnemyController>();
		enemyController.DieByRock();
		PowerUpManager.Shared.AddEnergy(PowerUpManager.Shared.energyForRockKill);
		StartCoroutine(AnimateAndDeactivate(enemyController.GetAnimator()));
	}

	private IEnumerator AnimateAndDeactivate(Animator anim)
	{
		yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0)[0].clip.length);
		if (anim)
		{
			anim.gameObject.SetActive(false);
		}
	}

	private IEnumerator PauseGameUntilAnimFinished(Animator anim)
	{
		_pauseTimer = WAIT_UNTIL_SET_AGAIN;
		yield return new WaitForSeconds(HALF_A_SECOND);
		player.StartPlayerDeathAnim();
		yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0)[0].clip.length - DEATH_ANIM_CUT);
		player.gameObject.SetActive(false);
		player.gameObject.SetActive(true);
		ResetStageUponDeath();
	}

	private void ResetStageUponDeath()
	{
		SpawnManager.Shared.RestartSpawns(); // remove all added spawns
		foreach (EnemyController enemy in enemies)
		{
			enemy.gameObject.SetActive(true);
			enemy.RestartEnemy();
		}
		player.ResetPlayer();
		_pauseTimer = roundCooldownTimer;
		if (!LifeManager.Shared.IsGameOver())
		{
			StartCoroutine(ShowCountdown());
		}
	}

	private IEnumerator ShowCountdown()
	{
		threeText.SetActive(true);
		yield return new WaitForSeconds(1f);
		threeText.SetActive(false);
		twoText.SetActive(true);
		yield return new WaitForSeconds(1f);
		twoText.SetActive(false);
		oneText.SetActive(true);
		yield return new WaitForSeconds(1f);
		oneText.SetActive(false);
		startText.SetActive(true);
		yield return new WaitForSeconds(3f);
		startText.SetActive(false);
	}

	public void WasEnemyFreed(Vector2Int playerMove)
	{
		foreach (EnemyController enemy in enemies)
		{
			if (enemy.GetOriginalHolePositions().Contains(playerMove))
			{
				enemy.ChasePlayer();
			}
		}
	}

	public Vector3 GetPlayerPosition()
	{
		return player.transform.position;
	}

	public List<Vector2Int> GetPlayerGridPosition()
	{
		return player.GetGridPositions();
	}

	public GridManager.Direction GetPlayerFacingDirection()
	{
		Vector2Int direction = player.GetFacingDirection();
		if (direction == Vector2Int.down) return GridManager.Direction.South;
		if (direction == Vector2Int.left) return GridManager.Direction.West;
		if (direction == Vector2Int.right) return GridManager.Direction.East;
		return GridManager.Direction.North;
	}

	public Vector2Int GetPlayerFacingDirectionVector()
	{
		return player.GetFacingDirection();
	}

	public List<MyTileData> FindRouteToPlayer(Vector2Int chaserPos, bool isFlying)
	{
		Vector2Int playerPos = GetPlayerGridPosition()[0];
		return GridManager.Shared.FindPath(chaserPos.x, chaserPos.y, playerPos.x, playerPos.y, isFlying);
	}

	public bool CanPlayerShoot()
	{
		return player.CanPlayerShoot() && _pauseTimer <= 0;
	}

	public void SetPlayerFiring(bool isFiring)
	{
		player.SetFiring(isFiring);
	}

	public void GameOver()
	{
		_pauseTimer = int.MaxValue;
		StartCoroutine(ShowGameOverText());
		_isGameOver = true;
	}

	private IEnumerator ShowGameOverText()
	{
		yield return new WaitForSeconds(gameOverTextDelay);
		gameOverText.SetActive(true);
	}

	public void SetMusic(bool isOn)
	{
		if (isOn && !gameThemeMusic.isPlaying)
		{
			gameThemeMusic.Play();
		}
		else if (!isOn && gameThemeMusic.isPlaying)
		{
			gameThemeMusic.Pause();
		}
	}
#endregion
}