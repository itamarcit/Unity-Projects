using System;
using UnityEngine;

public class ClockScript : MonoBehaviour
{
    public GameObject hourHand;
    public GameObject minuteHand;
    public GameObject secondHand;

    /* Calculations for the angle changes per second for each of the hands */
    private const float HOURS_TO_SECONDS_ANGLES = 1 / 120f;
    private const float MINUTES_TO_SECONDS_ANGLES = 1 / 10f;
    private const float SECONDS_TO_ANGLES = 6f;
    /* Calculations for the initial angle for each hand */
    private const float MINUTES_ADDITIONS_TO_HOUR_ANGLE = 1f / 2f;
    private const float SECONDS_ADDITIONS_TO_MINUTES_ANGLE = 1f / 10f;

    private void Awake()
    {
        DateTime time = DateTime.Now;
        hourHand.transform.eulerAngles = new Vector3(0f, 0f, time.Hour * -360f / 12f - 
                                                             (MINUTES_ADDITIONS_TO_HOUR_ANGLE * time.Minute));
        minuteHand.transform.eulerAngles = new Vector3(0f, 0f, time.Minute * -360f / 60f - 
                                                               (SECONDS_ADDITIONS_TO_MINUTES_ANGLE * time.Second));
        secondHand.transform.eulerAngles = new Vector3(0f, 0f, time.Second * -360f / 60f);
    }

    void Update()
    { 
        hourHand.transform.eulerAngles += new Vector3(0f, 0f, -Time.deltaTime * HOURS_TO_SECONDS_ANGLES);
        minuteHand.transform.eulerAngles += new Vector3(0f, 0f, -Time.deltaTime * MINUTES_TO_SECONDS_ANGLES);
        secondHand.transform.eulerAngles += new Vector3(0f, 0f, -Time.deltaTime * SECONDS_TO_ANGLES);
    }
}
