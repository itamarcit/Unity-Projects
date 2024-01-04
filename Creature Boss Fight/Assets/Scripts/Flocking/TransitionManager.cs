using Flocking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class TransitionManager : MonoBehaviour
{
    [FormerlySerializedAs("gamePause")] [SerializeField] private PauseManager pause;
    [SerializeField] private GameObject startButton; 
    [SerializeField] private GameObject mainGameUI; 
    [SerializeField] private GameObject endGameUI;
    [SerializeField] private CameraOrbit cameraOrbit;
    [SerializeField] private TextMeshProUGUI endScore;
    private static bool _isFirstLoad = true;
    private bool _startGamePressed;
    
    private void Start()
    {
        _startGamePressed = false;
        if (_isFirstLoad)
        {
            TransitionToOpeningScreen();
            _isFirstLoad = false;
        }
        else
        {
            TransitionToGameScreen();
        }
    }

    private void TransitionToOpeningScreen()
    {
        mainGameUI.SetActive(false);
        endGameUI.SetActive(false);
        pause.PauseAll();
        cameraOrbit.StartCameraRotation();
    }
    private void TransitionToGameScreen()
    {
        startButton.SetActive(false);
        endGameUI.SetActive(false);
        StartGame();
    }
    
    private void TransitionToEndScreen()
    {
        mainGameUI.SetActive(false);
        endGameUI.SetActive(true);
        pause.PauseAll();
        cameraOrbit.StartCameraRotation();
        endScore.text = "You died! " + "\n" + "Score:" + GameManager.Shared.GetPlayerScore();
    }

    public void StartGame()
    {
        _startGamePressed = true;
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // loads current scene
    }


    private void Update()
    {
        if (_startGamePressed)
        {
            cameraOrbit.StopCameraRotation();
            _startGamePressed = false;
            mainGameUI.SetActive(true);
        }
        else if (cameraOrbit.IsCameraInGamePosition()) // Wait for the camera to return to normal, then start game
        {
            pause.UnpauseAll();
            startButton.SetActive(false);
        }
        if (GameManager.Shared.IsGameOver())
        {
            TransitionToEndScreen();
        }
    }
}
