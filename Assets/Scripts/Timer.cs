using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    public static Action OnTimerEnded;

    [SerializeField] private float time = 3.0f;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private Image timerBar;

    private bool timerStarted = false;
    private bool timerFinished = false;
    private float elapsedTime = 0.0f;

    public IEnumerator StartTimerUntilDone(bool showUI)
    {        
        StartTimer(showUI);
        while (timerFinished == false) 
        {
            yield return null;
        }
    }

    public void StartTimer(bool showUI)
    {
        if (showUI)
        {
            timer.gameObject.SetActive(true);
            timerBar.gameObject.SetActive(true);
        }
        else
        {
            timer.gameObject.SetActive(false);
            timerBar.gameObject.SetActive(false);
        }

        if (!timerStarted)
        {
            gameObject.SetActive(true);
            timerStarted = true;
            timerFinished = false;
            elapsedTime = time;
        }
    }

    void Update()
    {
        if (!timerStarted || GameManager.Instance.IsGamePaused) return;
        elapsedTime -= Time.deltaTime;
        UpdateTimerText();
        if (elapsedTime < 0) 
        {
            DisableTimer();
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
        timerBar.fillAmount = Mathf.Lerp(timerBar.fillAmount, elapsedTime / time, Time.deltaTime * 10f);
    }
}
