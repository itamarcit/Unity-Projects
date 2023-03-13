using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
#region Fields
	[SerializeField] private int scorePerEnemyKilledByRock = 1000;
	[SerializeField] private int scorePerEnemyKilledByInflate = 250;
	[SerializeField] private int scorePerDig = 10;
	[SerializeField] private int scorePerPickup = 250;
	[SerializeField] private TextMeshProUGUI playerOneScore;
	[SerializeField] private TextMeshProUGUI highScore;
	[SerializeField] private GameObject scorePopupPrefab;
	[SerializeField] private GameObject canvas;
	[SerializeField] private int initialHighScore = 10000;
	private const String HIGH_SCORE = "highScore";
	public static ScoreManager Shared { get; private set; }
#endregion

	public enum Score
	{
		KillByRock,
		KillByInflate,
		Dig,
		PickupDrop
	}

#region Events
	private void Awake()
	{
		if (Shared == null)
		{
			Shared = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		if (!PlayerPrefs.HasKey(HIGH_SCORE))
		{
			PlayerPrefs.SetInt(HIGH_SCORE, initialHighScore);
			highScore.text = initialHighScore.ToString();
		}
		else
		{
			highScore.text = PlayerPrefs.GetInt(HIGH_SCORE).ToString();
		}
	}
#endregion

#region Methods
	public void AddScore(Score score, Vector3 pos, bool shouldPopup)
	{
		int currentScore = int.Parse(playerOneScore.text);
		int addition;
		int newScore = currentScore;
		switch (score)
		{
			case Score.KillByRock:
				newScore += scorePerEnemyKilledByRock;
				addition = scorePerEnemyKilledByRock;
				break;
			case Score.KillByInflate:
				newScore += scorePerEnemyKilledByInflate;
				addition = scorePerEnemyKilledByInflate;
				break;
			case Score.Dig:
				newScore += scorePerDig;
				addition = scorePerDig;
				break;
			case Score.PickupDrop:
				newScore += scorePerPickup;
				addition = scorePerPickup;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(score), score, null);
		}
		playerOneScore.text = newScore.ToString();
		if (newScore > PlayerPrefs.GetInt(HIGH_SCORE))
		{
			highScore.text = newScore.ToString();
			PlayerPrefs.SetInt(HIGH_SCORE, newScore);
		}
		if (shouldPopup)
		{
			GameObject obj = Instantiate(scorePopupPrefab, canvas.transform);
			obj.transform.position = pos;
			obj.GetComponent<TextMeshProUGUI>().SetText(addition.ToString());
		}
	}
#endregion
}