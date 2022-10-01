using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    public static Action OnTimerEnded;

    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private Image timerBar;

    private bool timerStarted = false;
    private float elapsedTime = 10.0f;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void StartTimer()
    {
        gameObject.SetActive(true);
        if (!timerStarted)
        {
            timerStarted = true;
            elapsedTime = 10.0f;
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
        elapsedTime = 0.0f;
        timerBar.fillAmount = 1.0f;
        OnTimerEnded?.Invoke();
        gameObject.SetActive(false);
    }

    void UpdateTimerText()
    {
        timer.text = ((int)(elapsedTime + 1)).ToString();
        timerBar.fillAmount = Mathf.Lerp(timerBar.fillAmount, elapsedTime / 10.0f, Time.deltaTime * 10f);
    }
}
