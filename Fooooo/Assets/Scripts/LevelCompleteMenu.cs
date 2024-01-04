using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelCompleteMenu : MonoBehaviour
{
    private const int LAST_STAGE = 5;
    [SerializeField] private Animator advanceStageAnimator;
    private bool _isWaitingForAnim;
    private static readonly int PressedAdvanceStage = Animator.StringToHash("PressedAdvanceStage");
    

    public void NextLevel()
    {
        EventSystem.current.SetSelectedGameObject(null);
        advanceStageAnimator.SetBool(PressedAdvanceStage, true);
        if(!_isWaitingForAnim) StartCoroutine(WaitForAnimThenAdvance());
        _isWaitingForAnim = true;
    }

    private IEnumerator WaitForAnimThenAdvance()
    {
        yield return null;
        while (advanceStageAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) yield return null;
        if (GameManager.Shared.stage == LAST_STAGE)
        {
            StartCoroutine(GameManager.Shared.LoadSceneAsync(-1));
            yield break;
        }
        GameManager.Shared.GoToNextLevel();
        _isWaitingForAnim = false;
    }

    public void MainMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(GameManager.Shared.LoadSceneAsync(-1));
    }

    public void Resume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        GameManager.Shared.PauseMenu();
    }

    public void Restart()
    {
        GameManager.Shared.DisableConfetti();
        EventSystem.current.SetSelectedGameObject(null);
        GameManager.Shared.RestartStage(true, true);
    }
}
