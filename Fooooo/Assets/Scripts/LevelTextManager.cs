using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTextManager : MonoBehaviour
{
    [SerializeField] private List<Animator> startStageTextAnimators;
    [SerializeField] private float waitBeforeShowingTitle = 1f;
    private WaitForSeconds _delayTitle;

    public static LevelTextManager Shared { get; private set; }

    private void Awake()
    {
        if (Shared == null)
        {
            Shared = this;
            _delayTitle = new WaitForSeconds(waitBeforeShowingTitle);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(ShowAnimThenDisable()); // Show the stage spawned in.
    }   

    public void ShowText()
    {
        StartCoroutine(ShowAnimThenDisable());
    }

    private IEnumerator ShowAnimThenDisable()
    {
        if (GameManager.Shared.stage >= startStageTextAnimators.Count) yield break;
        yield return _delayTitle;
        Animator correctAnimator = startStageTextAnimators[GameManager.Shared.stage];
        correctAnimator.gameObject.SetActive(true);
        yield return null;
        while (correctAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.7f) yield return null;
        correctAnimator.gameObject.SetActive(false);
    }
}
