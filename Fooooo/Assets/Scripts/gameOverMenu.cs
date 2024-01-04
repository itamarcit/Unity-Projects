using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class gameOverMenu : MonoBehaviour
{
    [SerializeField] private Animator gameOverAnimator;
    private static readonly int PressedRestart = Animator.StringToHash("PressedRestart");
    private bool _playingRestartingAnim = false;


    public void Restart()
    {
        EventSystem.current.SetSelectedGameObject(null);
        gameOverAnimator.SetBool(PressedRestart, true);
        if(!_playingRestartingAnim) StartCoroutine(WaitForAnimToFinishThenRestart());
        _playingRestartingAnim = true;
    }

    private IEnumerator WaitForAnimToFinishThenRestart()
    {
        yield return null;
        while (gameOverAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) yield return null;
        GameManager.Shared.RestartStage(true, false);
        _playingRestartingAnim = false;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(GameManager.Shared.LoadSceneAsync(-1));
    }
}
