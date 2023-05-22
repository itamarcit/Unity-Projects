using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    private int _score = 0;
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        PlayerPrefs.SetInt("Score1", 0);
        PlayerPrefs.SetInt("Score2", 0);
    }

    public void IncreaseScore(int playerNum, int scoreToAdd)
    {
        _score+= scoreToAdd;
        scoreText.text = PadWithZeroes(_score);
        PlayerPrefs.SetInt("Score"+playerNum, _score);
    }
    
    private static string PadWithZeroes(int num)
    {
        return num.ToString().PadLeft(4, '0');
    }

    public void SetScore(int score)
    {
        _score = score;
        scoreText.text = PadWithZeroes(_score);
    }

    public int GetScore()
    {
        return _score;
    }
}