using System;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private const int ASCII_NUM = 48;
    [FormerlySerializedAs("_ghostMirror")] [SerializeField] private GhostMirror ghostMirror;
    [FormerlySerializedAs("_lightBeam")] [SerializeField] private SendLightBeam lightBeam;
    [FormerlySerializedAs("_cameraManager")] [SerializeField] private CameraManager cameraManager;
    [SerializeField] private StartStageAnimation startStageAnimation;
    [SerializeField] private GameObject player;
    private Player _playerMovement;
    private PlayerMoveMirror _playerMirror;
    private bool _didWin = false;

    private int _level = 0;
    private int _nextLevel;
    [SerializeField] private ManegeLevelsLocks locksmanger;

    [FormerlySerializedAs("_enemyMoveScripts")] [SerializeField] private Enemy[] enemyMoveScripts;

    private void Start()
    {
        _playerMirror = player.GetComponent<PlayerMoveMirror>();
        _playerMovement = player.GetComponent<Player>();
        _level = Convert.ToInt32(SceneManager.GetActiveScene().name[5]) - ASCII_NUM;
        if (_level == 0)
        {
            StartCoroutine(startStageAnimation.ShowIntro());
        }
        else
        {
            StartCoroutine(startStageAnimation.OpenDoor(true));
        }
    }

    void Update()
    {
        CheckWinCondition();
        CheckPlayerQuitGame();
    }

    private static void CheckPlayerQuitGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void CheckWinCondition()
    {
        if (_playerMovement.DidChangeDirectionRecently() && _playerMirror.IsMovingAnItem())
        {
            return;
        }
        if (ghostMirror.IsActive && lightBeam.Caught && !_didWin)
        {
            _didWin = true;
            locksmanger.AddLevel();
            _playerMovement.StopMovement();
            StopEnemies();
            StartCoroutine(LevelComplete());
        }
    }

    IEnumerator LevelComplete()
    {
        
        yield return cameraManager.SwapCameraFocusToGhost();
        yield return ghostMirror.HandleEndingAnimation(lightBeam.GetPathInfo());
        yield return CharacterUtils.Dissolve(ghostMirror.GetRenderer());
        startStageAnimation.CloseDoor();
        yield return new WaitForSeconds(3f);
        //advance level
        _nextLevel = _level + 1;
        if (_nextLevel < 6) SceneManager.LoadSceneAsync("level" + _nextLevel);
    }
    
    private void StopEnemies() 
    {
        foreach (var enemy in enemyMoveScripts)
        {
            enemy.Stop();
        }
    }
}