using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum GameEvent
{
    EventAlert,
    EventStart,
    EventEnd
};

[Serializable]
public struct GameEventTime
{
    public GameEvent gameEvent;
    public float time;
    public float duration;
}

public class Clock : MonoBehaviour
{
    public static Action<GameEventTime> OnGameEvent;
    public static Action OnDayEnd;

    [SerializeField] private float debugTimeRate = 1.0f;
    [SerializeField] private int startHour = 5;
    [SerializeField] private int endHour = 10;
    [SerializeField] private float totalSeconds = 300.0f;
    [SerializeField] private TextMeshProUGUI clock;
    // [SerializeField] private TextMeshProUGUI debugClock;
    [SerializeField] List<GameEventTime> gameEventSchedule = new List<GameEventTime>();

    private int eventIndex = 0;
    private bool clockStarted = true;
    // private bool clockFinished = false;
    private float elapsedTime = 0.0f;

    void Update()
    {
        if (!clockStarted || GameManager.Instance.IsGamePaused) return;
        elapsedTime += Time.deltaTime * debugTimeRate;
        UpdateClockText();

        if (eventIndex < gameEventSchedule.Count)
        {
            if (gameEventSchedule[eventIndex].time < elapsedTime)
            {
                OnGameEvent?.Invoke(gameEventSchedule[eventIndex]);
                eventIndex += 1;
            }
        }

        if (elapsedTime > totalSeconds)
        {
            DisableClock();
        }
    }
    
    void DisableClock()
    {
        clockStarted = false;
        // clockFinished = true;
        OnDayEnd?.Invoke();
    }

    void UpdateClockText()
    {
        int hourDiff = endHour - startHour;
        float secondsPerHour = totalSeconds / hourDiff;
        int hourPassed = (int)(elapsedTime / secondsPerHour);
        float mod = elapsedTime % secondsPerHour;

        string text = $"{startHour + hourPassed}:";
        if (mod > (secondsPerHour * 5f / 6f)) text += "50 PM";
        else if (mod > (secondsPerHour * 4f / 6f)) text += "40 PM";
        else if (mod > (secondsPerHour * 3f / 6f)) text += "30 PM";
        else if (mod > (secondsPerHour * 2f / 6f)) text += "20 PM";
        else if (mod > (secondsPerHour * 1f / 6f)) text += "10 PM";
        else text += "00 PM";

        clock.text = text;
        // debugClock.text = elapsedTime.ToString("0.00") + " SEC";
    }
}
