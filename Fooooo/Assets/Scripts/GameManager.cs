using System.Collections;
using System.Collections.Generic;
using CMF;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	private const int GRANNY_STAGE = 2;
	private const int THREE_STARS = 3;
	private const int TWO_STARS = 2;
	private const int ONE_STAR = 1;
	private const int CARS_STAGE = 1;
	[FormerlySerializedAs("_islandCameraMove")] [SerializeField]
	private IslandCameraMove islandCameraMove;
	[SerializeField] private List<LeafControllerWrapper> leafControllers;
	[SerializeField] private GameObject gameOverImage;
	[SerializeField] private Image LevelCompleteImage;
	[SerializeField] private Image pauseMenuImage;
	[SerializeField] private GameObject timeOfScene;
	[SerializeField] private GameObject Player;
	[SerializeField] private List<Vector3> startPosForStages;
	[SerializeField] private Timer _timer;
	[SerializeField] private List<GameObject> stars;
	[SerializeField] private List<StageConfig> stageConfigs;
	[SerializeField] private AdvancedWalkerController playerController;
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private List<GameObject> firstButtons;
	[SerializeField] private FollowCamera fooTheTitle;
	[SerializeField] private GameObject tutorialKeys;
	[SerializeField] private GameObject loadingAnim;
	[SerializeField] private GameObject lastStageConfetti;
	private bool _shouldPlayEngineSound;
	public int stage;
	private bool _advancingStage;
	private bool _didWinStage;
	private float _time;
	public StageToStart sharedData;
	private bool inPause;
	private Quaternion playerStartRotation;
	public bool isStageOver;
	public bool finishedCars;
	private bool _isFrozen;
	private const int TUTORIAL = 0;
	private Coroutine _disableTutorial;
	public bool shouldLose;
	private bool _recentlySpawned;

	private const int FOO_THE_TITLE = 5;
	private const int OPENING_STAGE = -1;

	public static GameManager Shared { get; private set; }

	private void Awake()
	{
		if (Shared == null)
		{
			Shared = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		stage = sharedData.stage;
		playerStartRotation = Player.transform.rotation;
		isStageOver = false;
	}

	private void Start()
	{
		inPause = false;
		RestartStage(false, true);
	}

	public void LevelComplete()
	{
		// when the stage is complete.
		if (gameOverImage.gameObject.activeSelf || pauseMenuImage.gameObject.activeSelf || Time.timeScale == 0) return;
		Time.timeScale = 0f;
		if (stage == FOO_THE_TITLE)
		{
			lastStageConfetti.SetActive(true);
		}
		StartCoroutine(WaitAndWin());
	}

	public void DisableConfetti()
	{
		lastStageConfetti.SetActive(false);
	}

	private IEnumerator WaitAndWin()
	{
		yield return new WaitForSecondsRealtime(0.5f);
		if (stage == 1) finishedCars = true;
		isStageOver = true;
		float currentTime = _timer.getCurrentTime();
		int starsCount = CalculateStars(currentTime);
		setPlayerPrefs(starsCount);
		_timer.StopTimer();
		StartCoroutine(showStartsAndButtons(starsCount));
		LevelCompleteImage.gameObject.SetActive(true);
		timeOfScene.gameObject.SetActive(false);
	}

	public void GoToNextLevel()
	{
		// When the player presses to advance to next stage
		Time.timeScale = 1f;
		isStageOver = false;
		_advancingStage = true;
		LevelCompleteImage.gameObject.SetActive(false);
		timeOfScene.gameObject.SetActive(true);
		for (int i = 0; i < THREE_STARS; i++)
		{
			stars[i].gameObject.SetActive(false);
		}
		islandCameraMove.ShowBridge();
	}

	public void ChangeStage()
	{
		timeOfScene.gameObject.SetActive(true);
		GPUInstancing.RestartLeaves();
		_advancingStage = false;
		foreach (GameObject leafController in leafControllers[stage].leafControllers)
		{
			leafController.SetActive(false);
		}
		StartCoroutine(islandCameraMove.DisableBridgeAfterSeconds(stage, 1f));
		stageConfigs[stage].ToggleObjects(false);
		stage++;
		stageConfigs[stage].ToggleObjects(true);
		foreach (GameObject leafController in leafControllers[stage].leafControllers)
		{
			leafController.SetActive(true);
		}
		if (stage == FOO_THE_TITLE)
		{
			fooTheTitle.gameObject.SetActive(true);
			StartCoroutine(fooTheTitle.Appear());
		}
		_timer.RestartTimer();
	}
	
	public bool IsAdvancingStage()
	{
		return _advancingStage;
	}

	public void GameOver()
	{
		if (pauseMenuImage.gameObject.activeSelf || LevelCompleteImage.gameObject.activeSelf) return;
		Time.timeScale = 0f;
		_timer.StopTimer();
		StartCoroutine(SoundManager.Shared.PlayGameOverEffect());
		StartCoroutine(showRestartInGameOver());
		//eventSystem.SetSelectedGameObject(firstButtons[1]);
		//eventSystem.firstSelectedGameObject = firstButtons[1];
		gameOverImage.gameObject.SetActive(true);
		timeOfScene.gameObject.SetActive(false);
	}

	public void RestartStage(bool AlreadyToggled, bool restartLeaves)
	{
		shouldLose = false;
		LevelCompleteImage.gameObject.SetActive(false);
		timeOfScene.gameObject.SetActive(true);
		gameOverImage.gameObject.SetActive(false);
		pauseMenuImage.gameObject.SetActive(false);
		islandCameraMove.DisableCameraTransition();
		Time.timeScale = 1f;
		finishedCars = false;
		isStageOver = false;
		_timer.ResumeTimer();
		Rigidbody playerRigidbody = Player.GetComponent<Rigidbody>();
		playerRigidbody.isKinematic = true;
		StartCoroutine(RespawnPlayer());
		var col = stageConfigs[stage].enterToStageCollider;
		if (col)
		{
			col.enabled = false;
		}
		if (stage >= GRANNY_STAGE)
		{
			stageConfigs[stage].ToggleObjects(false);
			stageConfigs[stage].ToggleObjects(true);
		}
		if (restartLeaves)
		{
			_advancingStage = false;
			if (stage == CARS_STAGE)
			{
				stageConfigs[stage].ToggleObjects(false);
				stageConfigs[stage].ToggleObjects(true);
			}
			islandCameraMove.HideBridge();
			foreach (GameObject leafController in leafControllers[stage].leafControllers)
			{
				leafController.SetActive(false);
			}
			GPUInstancing.RestartLeaves();
			_timer.RestartTimer();
			foreach (GameObject leafController in leafControllers[stage].leafControllers)
			{
				leafController.SetActive(true);
			}
			if (stage == TUTORIAL)
			{
				if(!tutorialKeys.activeSelf) tutorialKeys.SetActive(true);
				if(_disableTutorial != null) StopCoroutine(_disableTutorial);
			}
		}
		if (!AlreadyToggled)
		{
			stageConfigs[stage].ToggleObjects(true);
		}
		if (stage == FOO_THE_TITLE)
		{
			fooTheTitle.gameObject.SetActive(true);
			StartCoroutine(fooTheTitle.Appear());
		}
		playerRigidbody.isKinematic = false;
	}

	private IEnumerator RemoveTutorial()
	{
		yield return new WaitForSeconds(3f);
		tutorialKeys.SetActive(false);
		_disableTutorial = null;
	}

	private IEnumerator RespawnPlayer()
	{
		while (islandCameraMove.IsMovingCamera()) yield return null;
		Player.transform.position = startPosForStages[stage];
		Player.transform.rotation = playerStartRotation;
		_recentlySpawned = true;
		StartCoroutine(PlayerSpawnedTimer());
	}

	private IEnumerator PlayerSpawnedTimer()
	{
		yield return new WaitForSeconds(0.2f);
		_recentlySpawned = false;
	}

	private int CalculateStars(float completionTime)
	{
		if (stage == 0) return THREE_STARS;
		float twoStarTime = stageConfigs[stage].maxTimeForTwoStars;     //_timeForStarsEachStage[stage][1];
		float threeStarTime = stageConfigs[stage].maxTimeForThreeStars; //_timeForStarsEachStage[stage][0];
		if (completionTime <= threeStarTime)
		{
			// Player completed the task within the three-star time limit
			return THREE_STARS;
		}
		else if (completionTime <= twoStarTime)
		{
			// Player completed the task within the two-star time limit
			return TWO_STARS;
		}
		else
		{
			// Player completed the task, but didn't meet the two-star or three-star time limits
			return ONE_STAR;
		}
	}

	public float GetPercentageNeeded()
	{
		return stageConfigs[stage].percentageToComplete;
	}

	private void setPlayerPrefs(int starsCount)
	{
		if (stage == 0)
		{
			PlayerPrefs.SetInt("Island_1_Unlocked", 1);
		}
		if (stage == 1)
		{
			PlayerPrefs.SetInt("Island_2_Unlocked", 1);
			if (starsCount >= PlayerPrefs.GetInt("Island_1_stars"))
			{
				PlayerPrefs.SetInt("Island_1_stars", starsCount);
			}
		}
		if (stage == 2)
		{
			PlayerPrefs.SetInt("Island_3_Unlocked", 1);
			if (starsCount >= PlayerPrefs.GetInt("Island_2_stars"))
			{
				PlayerPrefs.SetInt("Island_2_stars", starsCount);
			}
		}
		if (stage == 3)
		{
			PlayerPrefs.SetInt("Island_4_Unlocked", 1);
			if (starsCount >= PlayerPrefs.GetInt("Island_3_stars"))
			{
				PlayerPrefs.SetInt("Island_3_stars", starsCount);
			}
		}
		if (stage == 4)
		{
			PlayerPrefs.SetInt("Island_5_Unlocked", 1);
			if (starsCount >= PlayerPrefs.GetInt("Island_4_stars"))
			{
				PlayerPrefs.SetInt("Island_4_stars", starsCount);
			}
		}
		if (stage == 5)
		{
			PlayerPrefs.SetInt("Island_6_Unlocked", 1);
			if (starsCount >= PlayerPrefs.GetInt("Island_5_stars"))
			{
				PlayerPrefs.SetInt("Island_5_stars", starsCount);
			}
		}
	}

	public void PauseMenu()
	{
		if (gameOverImage.gameObject.activeSelf || LevelCompleteImage.gameObject.activeSelf) return;
		if (inPause)
		{
			Time.timeScale = 1f;
			pauseMenuImage.gameObject.SetActive(false);
			inPause = false;
		}
		else if (!gameOverImage.activeSelf)
		{
			inPause = true;
			Time.timeScale = 0f;
			eventSystem.SetSelectedGameObject(firstButtons[0]);
			//eventSystem.firstSelectedGameObject = firstButtons[0];
			pauseMenuImage.gameObject.SetActive(true);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			PauseMenu();
		}
		if (Input.GetKeyDown(KeyCode.Y))
		{
			ResetAllPlayerPrefs();
		}
		if (Time.timeScale > 0 || !pauseMenuImage.gameObject.activeSelf)
		{
			SoundManager.Shared.PlayEngine();
		}
		else
		{
			SoundManager.Shared.StopEngine();
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if(_disableTutorial != null) StopCoroutine(_disableTutorial);
			_disableTutorial = StartCoroutine(RemoveTutorial());
		}
	}

	private void ResetAllPlayerPrefs()
	{
		PlayerPrefs.SetInt("Island_1_Unlocked", 0);
		PlayerPrefs.SetInt("Island_2_Unlocked", 0);
		PlayerPrefs.SetInt("Island_3_Unlocked", 0);
		PlayerPrefs.SetInt("Island_4_Unlocked", 0);
		PlayerPrefs.SetInt("Island_5_Unlocked", 0);
		PlayerPrefs.SetInt("Island_1_stars", 0);
		PlayerPrefs.SetInt("Island_2_stars", 0);
		PlayerPrefs.SetInt("Island_3_stars", 0);
		PlayerPrefs.SetInt("Island_4_stars", 0);
		PlayerPrefs.SetInt("Island_5_stars", 0);
		StartCoroutine(LoadSceneAsync(OPENING_STAGE));
	}


	public bool DidWinCarStage()
	{
		return stage > CARS_STAGE || (stage == CARS_STAGE && _advancingStage);
	}

	public void FreezePlayerMovement()
	{
		_isFrozen = true;
		playerController.Freeze();
	}

	public void UnfreezePlayerMovement()
	{
		_isFrozen = false;
	}

	public bool PlayerIsFrozen()
	{
		return _isFrozen;
	}

	IEnumerator showStartsAndButtons(int starsCount)
	{
		eventSystem.SetSelectedGameObject(null);
		yield return new WaitForSecondsRealtime(0.7f);
		eventSystem.SetSelectedGameObject(firstButtons[2]);
		for (int i = 0; i < starsCount; i++)
		{
			stars[i].gameObject.SetActive(true);
		}
	}

	IEnumerator showRestartInGameOver()
	{
		eventSystem.SetSelectedGameObject(null);
		yield return new WaitForSecondsRealtime(0.7f);
		eventSystem.SetSelectedGameObject(firstButtons[1]);
	}

	public Bounds GetIslandBounds()
	{
		return stageConfigs[stage].islandBoundsCollider.bounds;
	}
	
	/// <summary>
	/// Loads the stage scene. If -1 then load opening scene
	/// </summary>
	/// <param name="stage">Stage. if -1 then load opening scene</param>
	/// <returns></returns>
	public IEnumerator LoadSceneAsync(int stage)
	{
		AsyncOperation asyncLoad;
		if (stage == OPENING_STAGE)
		{
			asyncLoad = SceneManager.LoadSceneAsync("Opening Scene", LoadSceneMode.Single);
		}
		else
		{
			sharedData.stage = stage;
			asyncLoad = SceneManager.LoadSceneAsync("Game 3", LoadSceneMode.Single);
		}
		asyncLoad.allowSceneActivation = false;
		loadingAnim.SetActive(true);
		while (asyncLoad.progress < 0.85f)
		{
			yield return null;
		}
		yield return new WaitForSecondsRealtime(1.5f);
		loadingAnim.SetActive(false);
		asyncLoad.allowSceneActivation = true;
	}

	public bool RecentlySpawned()
	{
		return _recentlySpawned;
	}
}