using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Shared { get; private set; }

    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject icePaddle;
    [SerializeField] private GameObject lavaPaddle;
    [SerializeField] private GameObject princessPaddle;
    private GameObject _activePaddle;
    [SerializeField] private GameObject endOfStageTextPrefab;
    [SerializeField] private GameObject gameOverParent;
    [SerializeField] private GameObject princess;
    [SerializeField] private GameObject winnerCanvas;
    private ShownObjects _uiElements;
    private GameObject _endOfStageText;
    private PaddleMovementScript _paddleScript;
    private BallMovement _ballScript;
    private bool _started;
    private bool _stageComplete;
    private int _currentStage = 1;
    private bool _isMovingStage = false;
    private const int LAVA_STAGE = 2;
    private const int PRINCESS_STAGE = 3;
    private const string UI_ELEMENTS_OBJECT_NAME = "UI Elements";
    private readonly Vector3 _endPaddleLocation = new(-0.8810114f, -0.9054866f, 0f);
    private readonly Vector3 _endPrincessLocation = new(1.758263f, -4.225486f, 0f);

    private readonly List<String> _stages = new List<string>
    {
        "Opening Screen",
        "Ice Stage",
        "Lava Stage",
        "Main Stage"
    };

    private bool _movePrincessToPaddle;
    private bool _movePrincessOffScreen;

    private void Start()
    {
        _activePaddle = icePaddle;
        _paddleScript = _activePaddle.GetComponent<PaddleMovementScript>();
        _ballScript = ball.GetComponent<BallMovement>();
        _endOfStageText = InstantiateEndOfStageText();
        _uiElements = GameObject.Find(UI_ELEMENTS_OBJECT_NAME).GetComponent<ShownObjects>();
    }

    private void Awake()
    {
        if (Shared == null)
        {
            Shared = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += this.OnSceneLoad;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= this.OnSceneLoad;
    }

    private void Update()
    {
        if (!_started && Input.GetKeyDown(KeyCode.Space))
        {
            _ballScript.StartMovement();
            _started = true;
        }

        if (_movePrincessToPaddle)
        {
            MovePrincessAndPaddle();
        }

        if (_movePrincessOffScreen)
        {
            princess.transform.localPosition += Vector3.right * Time.deltaTime;
            _activePaddle.transform.localPosition += Vector3.right * Time.deltaTime;
        }
    }

    private void MovePrincessAndPaddle()
    {
        _activePaddle.transform.localPosition =
            Vector3.MoveTowards(_activePaddle.transform.localPosition,
                _endPaddleLocation, Time.deltaTime * 5f);
        princess.transform.localPosition =
            Vector3.MoveTowards(princess.transform.localPosition,
                _endPrincessLocation, Time.deltaTime);
        if (Vector3.Distance(_activePaddle.transform.localPosition, _endPaddleLocation) < 0.01f
            && Vector3.Distance(princess.transform.localPosition, _endPrincessLocation) < 0.01f)
        {
            _movePrincessToPaddle = false;
            StartCoroutine(MovePrincessOffScreen());
        }
    }

    private IEnumerator MovePrincessOffScreen()
    {
        yield return new WaitForSeconds(2f);
        _movePrincessOffScreen = true;
        yield return new WaitForSeconds(1f);
        _movePrincessOffScreen = false;
        winnerCanvas.SetActive(true);
    }

    public void ResetGame()
    {
        _started = false;
        BrickManager.Shared.ResetRound();
        _paddleScript.ResetPaddle();
        _ballScript.ResetBall();
    }

    public void GameOver()
    {
        ball.SetActive(false);
        _activePaddle.SetActive(false);
        _started = false;
        gameOverParent.SetActive(true);
    }

    public bool DidGameStart()
    {
        return _started;
    }

    public void CompleteStage()
    {
        _endOfStageText.SetActive(true);
        _stageComplete = true;
    }

    public void AdvanceStage()
    {
        if (_stageComplete)
        {
            HideUIElements();
            StartCoroutine(ShowMapThenAdvanceStage());
        }
    }

    private void HideUIElements()
    {
        foreach (var obj in _uiElements.objects)
        {
            obj.SetActive(false);
        }

        _activePaddle.SetActive(false);
    }

    private IEnumerator ShowMapThenAdvanceStage()
    {
        yield return MapManager.Shared.ShowMap((MapManager.MoveName)_currentStage);
        yield return MapManager.Shared.HideMap();
        _currentStage++;
        _isMovingStage = true;
        SceneManager.LoadScene(_stages[_currentStage]);
    }

    /**
     * This is an event that runs on scene load.
     */
    private void OnSceneLoad(Scene scene, LoadSceneMode sceneMode)
    {
        if (_isMovingStage)
        {
            LoadNextStage();
            _isMovingStage = false;
            _stageComplete = false;
        }
    }

    private void LoadNextStage()
    {
        ResetGame();
        _uiElements = GameObject.Find(UI_ELEMENTS_OBJECT_NAME).GetComponent<ShownObjects>();
        _endOfStageText = InstantiateEndOfStageText();
        switch (_currentStage)
        {
            case LAVA_STAGE:
                LoadLavaStage();
                break;
            case PRINCESS_STAGE:
                LoadPrincessStage();
                break;
        }
    }

    private void LoadLavaStage()
    {
        icePaddle.SetActive(false);
        lavaPaddle.SetActive(true);
        _activePaddle = lavaPaddle;
        _paddleScript = lavaPaddle.GetComponent<PaddleMovementScript>();
    }

    private void LoadPrincessStage()
    {
        lavaPaddle.SetActive(false);
        princessPaddle.SetActive(true);
        _activePaddle = princessPaddle;
        _paddleScript = princessPaddle.GetComponent<PaddleMovementScript>();
        var cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        winnerCanvas.GetComponent<Canvas>().worldCamera = cam;
        InventoryManager.Shared.RegisterLocks();
    }

    private GameObject InstantiateEndOfStageText()
    {
        var canvas = GameObject.FindGameObjectWithTag("Canvas");
        var instantiated = Instantiate(endOfStageTextPrefab, Vector3.forward,
            Quaternion.identity, canvas.transform);
        instantiated.SetActive(false);
        instantiated.transform.localPosition = new Vector3(-955.8241f, -538.9188f, 0.09439122f);
        return instantiated;
    }

    public int GetStage()
    {
        return _currentStage;
    }

    public GameObject GetBall()
    {
        return ball;
    }

    public GameObject GetPaddle()
    {
        return _activePaddle;
    }


    /**
     * Called when a brick is destroyed in the game
     */
    public void BrickDestroyed()
    {
        int totalBricks = BrickManager.Shared.GetBricksDestroyedThisRound();
        if (totalBricks == 4)
        {
            _ballScript.SetSpeed(BallMovement.Speed.Low);
        }
        else if (totalBricks == 8)
        {
            _ballScript.SetSpeed(BallMovement.Speed.Medium);
        }
        else if (totalBricks == 12)
        {
            _ballScript.SetSpeed(BallMovement.Speed.Max);
        }
    }

    public void FinishedGame()
    {
        _started = false;
        princess.SetActive(true);
        _movePrincessToPaddle = true;
    }
}