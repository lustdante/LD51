using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    const float MAX_TIME = 3.0f;

    public static Action OnTimerEnded;

    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private Image timerBar;

    private bool timerStarted = false;
    private bool timerFinished = false;
    private float elapsedTime = MAX_TIME;

    public IEnumerator StartTimerUntilDone()
    {        
        StartTimer();
        while (timerFinished == false) 
        {
            yield return null;
        }
    }

    public void StartTimer()
    {
        if (!timerStarted)
        {
            gameObject.SetActive(true);
            timerStarted = true;
            timerFinished = false;
            elapsedTime = MAX_TIME;
        }
    }

    void Update()
    {
        if (timerStarted)
        {
            elapsedTime -= Time.deltaTime;
            UpdateTimerText();
            if (elapsedTime < 0) 
            {
                DisableTimer();
            }
        }
    }

    void DisableTimer()
    {
        timerStarted = false;
        timerFinished = true;
        elapsedTime = 0.0f;
        timerBar.fillAmount = 1.0f;
        OnTimerEnded?.Invoke();
        gameObject.SetActive(false);
    }

    void UpdateTimerText()
    {
        timer.text = ((int)(elapsedTime + 1)).ToString();
        timerBar.fillAmount = Mathf.Lerp(timerBar.fillAmount, elapsedTime / MAX_TIME, Time.deltaTime * 10f);
    }
}
