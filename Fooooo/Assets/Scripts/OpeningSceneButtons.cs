using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningSceneButtons : MonoBehaviour
{
	private const int OPENING_SCENE = -1;
	[SerializeField] private GameObject loadingAnim;
	public StageToStart sharedParameter;

	public void Island_0_Pressed()
	{
		StartCoroutine(LoadSceneAsync(0));
	}

	public void Island_1_Pressed()
	{
		if (PlayerPrefs.GetInt("Island_1_Unlocked") == 0) return;
		StartCoroutine(LoadSceneAsync(1));
	}

	public void Island_2_Pressed()
	{
		if (PlayerPrefs.GetInt("Island_2_Unlocked") == 0) return;
		StartCoroutine(LoadSceneAsync(2));
	}

	public void Island_3_Pressed()
	{
		if (PlayerPrefs.GetInt("Island_2_Unlocked") == 0) return;
		StartCoroutine(LoadSceneAsync(3));
	}

	public void Island_4_Pressed()
	{
		if (PlayerPrefs.GetInt("Island_4_Unlocked") == 0) return;
		StartCoroutine(LoadSceneAsync(4));
	}

	public void Island_5_Pressed()
	{
		if (PlayerPrefs.GetInt("Island_5_Unlocked") == 0) return;
		StartCoroutine(LoadSceneAsync(5));
	}

	private IEnumerator LoadSceneAsync(int stage)
	{
		AsyncOperation asyncLoad;
		if (stage == OPENING_SCENE)
		{
			asyncLoad = SceneManager.LoadSceneAsync("Opening Scene");
		}
		else
		{
			sharedParameter.stage = stage;
			asyncLoad = SceneManager.LoadSceneAsync("Game 3", LoadSceneMode.Single);
		}
		asyncLoad.allowSceneActivation = false;
		loadingAnim.SetActive(true);
		while (asyncLoad.progress < 0.85f)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1.5f);
		loadingAnim.SetActive(false);
		asyncLoad.allowSceneActivation = true;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Y))
		{
			ResetAllPlayerPrefs();
			StartCoroutine(LoadSceneAsync(OPENING_SCENE));
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			OpenAllLevels();
			StartCoroutine(LoadSceneAsync(OPENING_SCENE));
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
	}

	private void OpenAllLevels()
	{
		PlayerPrefs.SetInt("Island_1_Unlocked", 1);
		PlayerPrefs.SetInt("Island_2_Unlocked", 1);
		PlayerPrefs.SetInt("Island_3_Unlocked", 1);
		PlayerPrefs.SetInt("Island_4_Unlocked", 1);
		PlayerPrefs.SetInt("Island_5_Unlocked", 1);
		PlayerPrefs.SetInt("Island_1_stars", 0);
		PlayerPrefs.SetInt("Island_2_stars", 0);
		PlayerPrefs.SetInt("Island_3_stars", 0);
		PlayerPrefs.SetInt("Island_4_stars", 0);
		PlayerPrefs.SetInt("Island_5_stars", 0);
	}
}