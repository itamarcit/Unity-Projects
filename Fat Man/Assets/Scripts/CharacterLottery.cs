using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterLottery : MonoBehaviour
{
	enum Character
	{
		FatMan = 1, //1
		Coach       //2
	}

	private const int FATMAN = 0;
	private const int COACH = 1;

	[SerializeField] private Sprite fatManIconSprite;
	[SerializeField] private Sprite fatManNameSprite;

	[SerializeField] private Sprite coachIconSprite;
	[SerializeField] private Sprite coachNameSprite;

	[SerializeField] private SpriteRenderer p1Icon;
	[SerializeField] private SpriteRenderer p1Name;
	[SerializeField] private SpriteRenderer p2Icon;

	[SerializeField] private SpriteRenderer p2Name;
	private const string GAME_SCENE_NAME = "Game";
	[SerializeField] private float waitBeforeStartGame = 3;

	private Character _currP1Character;
	private int _modifier;

	void Awake()
	{
		//just any default values
		_currP1Character = Character.FatMan;
		p1Icon.sprite = fatManIconSprite;
		p1Name.sprite = fatManNameSprite;
		p2Icon.sprite = coachIconSprite;
		p2Name.sprite = coachNameSprite;
	}

	// Start is called before the first frame update
	private void Start()
	{
		_modifier = Random.Range(20, 22); //20 - Coach, 21 - Fatman 50/50 chances
		StartCoroutine(RunLottery());
	}

	private IEnumerator RunLottery()
	{
		for (int i = 1; i < _modifier; i++)
		{
			yield return new WaitForSeconds(0.01f * i);
			SwapCurrCharacter();
		}
		SaveForNextScene();
		yield return new WaitForSeconds(waitBeforeStartGame);
		SceneManager.LoadScene(GAME_SCENE_NAME);
	}

	private void SwapCurrCharacter()
	{
		if (_currP1Character == Character.FatMan)
		{
			//UI
			_currP1Character = Character.Coach;
			p1Icon.sprite = coachIconSprite;
			p1Name.sprite = coachNameSprite;
			p2Icon.sprite = fatManIconSprite;
			p2Name.sprite = fatManNameSprite;
			//CONTROLS
		}
		else
		{
			//UI
			_currP1Character = Character.FatMan;
			p1Icon.sprite = fatManIconSprite;
			p1Name.sprite = fatManNameSprite;
			p2Icon.sprite = coachIconSprite;
			p2Name.sprite = coachNameSprite;
			//CONTROLS
		}
	}

	private void SaveForNextScene()
	{
		if (_currP1Character == Character.FatMan)
		{
			print("_currP1Character is FATMAN!!!!!!!!!!!!");
			PlayerPrefs.SetInt("player1Character", FATMAN); //0 - FATMAN
			PlayerPrefs.SetInt("player2Character", COACH);  //1 - COACH
		}
		else
		{
			PlayerPrefs.SetInt("player1Character", COACH);
			PlayerPrefs.SetInt("player2Character", FATMAN);
		}
	}
}