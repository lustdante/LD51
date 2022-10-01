using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Clock : MonoBehaviour
{
    [SerializeField] private int startHour = 5;
    [SerializeField] private int endHour = 10;
    [SerializeField] private float totalSeconds = 300.0f;
    [SerializeField] private TextMeshProUGUI clock;
    [SerializeField] private TextMeshProUGUI debugClock;

    private bool clockStarted = true;
    private bool clockFinished = false;
    private float elapsedTime = 0.0f;

    void Update()
    {
        if (clockStarted)
        {
            elapsedTime += Time.deltaTime;
            UpdateClockText();
        }
    }

    void UpdateClockText()
    {
        int hourDiff = endHour - startHour;
        float secondsPerHour = totalSeconds / hourDiff;
        int hourPassed = (int)(elapsedTime / secondsPerHour);
        float mod = elapsedTime % secondsPerHour;
        bool isHalf = mod > (secondsPerHour / 2);
        string text = $"{startHour + hourPassed}:";
        if (isHalf) text += "30 PM";
        else text += "00 PM";
        clock.text = text;

        debugClock.text = elapsedTime.ToString("0.00") + " SEC";
    }
}
