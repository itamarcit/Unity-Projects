using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float timer;
    private bool isTimerRunning;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        // Start the timer
        StartTimer();
    }

    private void Update()
    {
        // Update the timer if it is running
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        // Convert the timer value to minutes and seconds
        int minutes = (int)(timer / 60);
        int seconds = (int)(timer % 60);

        // Display the time or use it as needed
        string currentTime = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.text = currentTime;
    }

    public void StartTimer()
    {
        // Start the timer from 00:00
        timer = 0f;
        isTimerRunning = true;
        UpdateTimerUI();
    }

    public void StopTimer()
    {
        // Stop the timer
        isTimerRunning = false;
    }

    public void ResumeTimer()
    {
        // Resume the timer
        isTimerRunning = true;
    }

    public void RestartTimer()
    {
        // Restart the timer
        timer = 0f;
        isTimerRunning = true;
        UpdateTimerUI();
    }

    public float getCurrentTime()
    {
        int minutes = (int)(timer / 60);
        int seconds = (int)(timer % 60);
        return 60 * minutes + seconds;
    }
}