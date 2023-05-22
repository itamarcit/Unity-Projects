using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToSceneAfterAnimEnds : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string sceneToChangeTo;
    [SerializeField] private float delayBeforeMovingScene;
    [SerializeField] private KeyCode keyToSkip;

    private bool _startedCoroutine;
    private void Update()
    {
        if (!_startedCoroutine && animator.GetCurrentAnimatorStateInfo(0).IsName("Finish"))
        {
            StartCoroutine(MoveSceneAfterDelay());
        }
        if (Input.GetKeyDown(keyToSkip))
        {
            SceneManager.LoadScene(sceneToChangeTo);
        }
    }

    private IEnumerator MoveSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeMovingScene);
        SceneManager.LoadScene(sceneToChangeTo);
    }
}
