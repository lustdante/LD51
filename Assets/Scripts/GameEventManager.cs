using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEventManager : MonoBehaviour
{
    [SerializeField] private Image eventAlert;
    [SerializeField] private Image eventStart;
    [SerializeField] private Image eventStartFill;

    private bool eventStarted = false;
    private float eventDuration = 1.0f;
    private float elapsedTime = 0.0f;

    void Update()
    {
        if (!eventStarted) return;
        elapsedTime += Time.deltaTime;
        eventStartFill.fillAmount = Mathf.Lerp(eventStartFill.fillAmount, (eventDuration - elapsedTime) / eventDuration, Time.deltaTime * 10f);
    }

    void HandleGameEvent(GameEventTime eventTime)
    {
        eventAlert.gameObject.SetActive(false);
        eventStart.gameObject.SetActive(false);

        switch (eventTime.gameEvent)
        {
            case GameEvent.EventAlert:
                eventAlert.gameObject.SetActive(true);
                break;
            case GameEvent.EventStart:
                eventStart.gameObject.SetActive(true);
                eventStarted = true;
                elapsedTime = 0.0f;
                eventDuration = eventTime.duration;
                eventStartFill.fillAmount = 1.0f;
                break;
            case GameEvent.EventEnd:
                eventStarted = false;
                break;
        }
    }

    private void OnEnable()
    {
        Clock.OnGameEvent += HandleGameEvent;
    }

    private void OnDisable()
    {
        Clock.OnGameEvent -= HandleGameEvent;
    }
}
