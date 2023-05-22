using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartStageAnimation : MonoBehaviour
{
	private Animator _animator;
	[SerializeField] private GameObject leftDoor;
	[SerializeField] private GameObject rightDoor;
	[SerializeField] private Image stageNameText;
	[SerializeField] private float timeToFadeText = 2f;
	[SerializeField] private float timeBeforeFade = 3f;
	[SerializeField] private float timeForTextRead = 10f;
	[SerializeField] private GameObject firstText;
	[SerializeField] private GameObject secondText;
	[SerializeField] private float timeToWaitBetweenSpaces = 0.25f;


	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void CloseDoor()
	{
		_animator.Play("Door_closing");
	}

	public IEnumerator OpenDoor(bool shouldShowStageName)
	{
		leftDoor.SetActive(true);
		rightDoor.SetActive(true);
		_animator.Play("Door_opening");
		if (shouldShowStageName)
		{
			yield return FadeOutStageName();
		}
	}

	private IEnumerator FadeOutStageName()
	{
		yield return new WaitForSeconds(timeBeforeFade);
		float totalTime = timeToFadeText;
		while (totalTime > 0)
		{
			stageNameText.color = new Color(1, 1, 1, totalTime / timeToFadeText);
			totalTime -= Time.deltaTime;
			yield return null;
		}
		stageNameText.gameObject.SetActive(false);
	}

	private IEnumerator ShowBothTextScreens()
	{
		yield return WaitSecondsOrSpacePressed(timeForTextRead);
		firstText.SetActive(false);
		secondText.SetActive(true);
		yield return new WaitForSeconds(timeToWaitBetweenSpaces);
		yield return WaitSecondsOrSpacePressed(timeForTextRead);
	}

	public IEnumerator ShowIntro()
	{
		firstText.SetActive(true);
		yield return OpenDoor(false);
		yield return ShowBothTextScreens();
		CloseDoor();
		yield return new WaitForSeconds(3f);
		secondText.SetActive(false);
		yield return OpenDoor(true);
	}

	private IEnumerator WaitSecondsOrSpacePressed(float seconds)
	{
		float time = 0;
		while (time < seconds && !Input.GetKeyDown(KeyCode.Space))
		{
			time += Time.deltaTime;
			yield return null;
		}
	}
}