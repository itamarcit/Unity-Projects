using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    [SerializeField] private ScoreText p1Score;
    [SerializeField] private ScoreText p2Score;
    private const float TIME_TO_ADD_IF_EQUAL = 30f;
    public TextMeshProUGUI timerText;
    public float startingTime = 60f;
    private float _remainingTime;
    private bool _timerStarted;
    private const string END_SCENE_NAME = "End";

    private void Start()
    {
        _remainingTime = startingTime;
        UpdateTimerText();
    }

    private void Update()
    {
        if (_timerStarted)
        {
            _remainingTime -= Time.deltaTime;
            if (_remainingTime > 0)
            {
                UpdateTimerText();
            }
            else
            {
                if (p1Score.GetScore() == p2Score.GetScore())
                {
                    _remainingTime = TIME_TO_ADD_IF_EQUAL;
                }
                else
                {
                    SceneManager.LoadScene(END_SCENE_NAME);
                }
            }
        }
    }

    public void StartTimer()
    {
        _timerStarted = true;
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(_remainingTime / 60f);
        int seconds = Mathf.FloorToInt(_remainingTime % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
