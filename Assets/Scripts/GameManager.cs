using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public enum PlayerActionState {Idle, Studying, Playing, Napping};

public class GameManager : Singleton<GameManager>
{
    public static Action<int> OnPlayerLevelUp;

    [SerializeField] Transform doorPos;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Timer timer;
    [SerializeField] Meter studyMeter;
    [SerializeField] Meter playMeter;
    [SerializeField] Meter boredomMeter;
    [SerializeField] Meter focusMeter;
    [SerializeField] Meter tiredMeter;
    [SerializeField] int playMaxLevel = 99;
    [SerializeField] int playLevels = 20;
    [SerializeField] float studyGoal = 100.0f;
    [SerializeField] float playGoal = 100.0f;
    [SerializeField] float boredomMax = 6.0f;
    [SerializeField] float boredomIdleDecay = 0.05f;
    [SerializeField] float boredomPlayDecay = 1.0f;
    [SerializeField] float focusMax = 5.0f;
    [SerializeField] float focusDecay = 1.5f;
    [SerializeField] float tiredMax = 30.0f;
    [SerializeField] float tiredStudyRate = 1.5f;
    [SerializeField] float tiredDecay = 6.0f;
    [SerializeField] TextMeshProUGUI studyMultText;
    [SerializeField] TextMeshProUGUI eventText;
    [SerializeField] TextMeshProUGUI gameLevel;
    [SerializeField] TextMeshProUGUI studyGrade;

    private bool gameEventStarted = false;
    private float studyProgress = 0.0f;
    private float playProgress = 0.0f;
    private float boredomProgress = 0.0f;
    private float focusProgress = 0.0f;
    private float tiredProgress = 0.0f;

    private int currentGrade = -1;
    private int currentLevel = -1;

    private PlayerActionState playerState = PlayerActionState.Idle;
    public PlayerActionState PlayerState {
        get { return this.playerState; }
        set {
            studyMeter.gameObject.SetActive(false);
            playMeter.gameObject.SetActive(false);
            studyMultText.text = "";

            switch (value)
            {
                case PlayerActionState.Idle:
                    break;
                case PlayerActionState.Studying:
                    studyMeter.gameObject.SetActive(true);
                    break;
                case PlayerActionState.Playing:
                    playMeter.gameObject.SetActive(true);
                    break;
            }

            this.playerState = value;
        }
    }

    private void Start()
    {
        studyMeter.MaxValue = studyGoal;
        playMeter.MaxValue = playGoal;
        boredomMeter.MaxValue = boredomMax;
        focusMeter.MaxValue = focusMax;
        tiredMeter.MaxValue = tiredMax;

        UpdatePlayerLevelText();
        UpdateStudyGradeText();
    }

    private void Update()
    {
        float boredomMult = 1.0f;
        float focusMult = 1.0f;
        float tiredMult = 1.0f;
        float gameMult = 1.0f;
        string studyMults = "";

        switch (playerState)
        {
            case PlayerActionState.Idle:
                // Focus is decayed naturally
                focusProgress = Mathf.Clamp(focusProgress - focusDecay * Time.deltaTime, 0, focusMax);
                focusMeter.Value = focusProgress;

                boredomProgress = Mathf.Clamp(boredomProgress - boredomIdleDecay * Time.deltaTime, 0, boredomMax);
                boredomMeter.Value = boredomProgress;
                break;
            case PlayerActionState.Playing:
                if (gameEventStarted) gameMult = 2.0f;
                // Focus is decayed naturally
                focusProgress = Mathf.Clamp(focusProgress - focusDecay * Time.deltaTime, 0, focusMax);
                focusMeter.Value = focusProgress;

                playProgress = Mathf.Clamp(playProgress + Time.deltaTime * gameMult, 0, playGoal);
                playMeter.Value = playProgress;
                boredomProgress = Mathf.Clamp(boredomProgress - boredomPlayDecay * Time.deltaTime, 0, boredomMax);
                boredomMeter.Value = boredomProgress;

                tiredProgress = Mathf.Clamp(tiredProgress + Time.deltaTime, 0, tiredMax);
                tiredMeter.Value = tiredProgress;

                UpdatePlayerLevelText();
                break;
            case PlayerActionState.Studying:
                if (boredomProgress < (boredomMax / 2))
                {
                    boredomMult = 1.5f;
                    studyMults += "x 1.5 Not bored\n";
                }
                else if (boredomProgress < (boredomMax - float.Epsilon))
                {
                    boredomMult = 1.2f;
                    studyMults += "x 1.2 A bit bored\n";
                }
                else
                {
                    boredomMult = 0.5f;
                    studyMults += "x 0.5 Very bored\n";
                }

                if (focusProgress < (focusMax * 0.8))
                {
                    focusMult = 0.5f;
                    studyMults += "x 0.5 Not focused\n";
                }
                else
                {
                    focusMult = 1.5f;
                    studyMults += "x 1.5 Focused\n";
                }

                if (tiredProgress > (tiredMax * 0.9))
                {
                    tiredMult = 0.2f;
                    studyMults += "x 0.2 Very tired\n";
                }
                else if (tiredProgress > (tiredMax * 0.5))
                {
                    tiredMult = 0.8f;
                    studyMults += "x 0.8 A bit tired\n";
                }

                studyMultText.text = studyMults;

                studyProgress = Mathf.Clamp(studyProgress + Time.deltaTime * boredomMult * focusMult * tiredMult, 0, studyGoal);
                studyMeter.Value = studyProgress;
                boredomProgress = Mathf.Clamp(boredomProgress + Time.deltaTime, 0, boredomMax);
                boredomMeter.Value = boredomProgress;
                focusProgress = Mathf.Clamp(focusProgress + Time.deltaTime, 0, focusMax);
                focusMeter.Value = focusProgress;
                tiredProgress = Mathf.Clamp(tiredProgress + Time.deltaTime * tiredStudyRate, 0, tiredMax);
                tiredMeter.Value = tiredProgress;

                UpdateStudyGradeText();
                break;
            case PlayerActionState.Napping:
                // Focus is decayed naturally
                focusProgress = Mathf.Clamp(focusProgress - focusDecay * Time.deltaTime, 0, focusMax);
                focusMeter.Value = focusProgress;

                // Decay tired meter
                tiredProgress = Mathf.Clamp(tiredProgress - Time.deltaTime * tiredDecay, 0, tiredMax);
                tiredMeter.Value = tiredProgress;
                break;
        }
    }

    private void OnEnable()
    {
        Timer.OnTimerEnded += TimerEnded;
        Clock.OnGameEvent += HandleGameEvent;
    }

    private void OnDisable()
    {
        Timer.OnTimerEnded -= TimerEnded;
        Clock.OnGameEvent -= HandleGameEvent;
    }

    void TimerEnded()
    {
        StartCoroutine(RunTestEvent());
    }

    void HandleGameEvent(GameEventTime eventTime)
    {
        switch (eventTime.gameEvent)
        {
            case GameEvent.EventStart:
                gameEventStarted = true;
                eventText.gameObject.SetActive(true);
                break;
            case GameEvent.EventEnd:
                gameEventStarted = false;
                eventText.gameObject.SetActive(false);
                break;
        }
    }

    public void StartEventLoop()
    {
        StartCoroutine(EventLoop());
    }

    IEnumerator EventLoop()
    {
        while (true) {
            yield return timer.StartTimerUntilDone();
            yield return RunTestEvent();
        }
    }

    IEnumerator RunTestEvent()
    {
        GameObject obj = Instantiate(enemyPrefab, doorPos.position, Quaternion.Euler(0, 0, 180), transform);
        obj.transform.DOMoveX(5, 2.0f).SetEase(Ease.OutBounce).WaitForCompletion();
        yield return new WaitForSeconds(3.0f);
        Destroy(obj);
        yield return new WaitForSeconds(0.5f);
    }

    void UpdatePlayerLevelText()
    {
        int nextLevel = (int)((playProgress / playGoal) * playLevels);
        if (nextLevel > currentLevel)
        {
            currentLevel = nextLevel;
            gameLevel.text = $"{playMaxLevel - playLevels + currentLevel}";
        }
    }

    void UpdateStudyGradeText()
    {
        float studyPercentile = studyProgress / studyGoal;
        int nextGrade = 0;
        string letterGrade = "F";

        if (studyPercentile < 0.6f) { nextGrade = 0; letterGrade = "F"; }
        else if (studyPercentile < 0.675f) { nextGrade = 1; letterGrade = "D"; }
        else if (studyPercentile < 0.705f) { nextGrade = 2; letterGrade = "C-"; }
        else if (studyPercentile < 0.745f) { nextGrade = 3; letterGrade = "C"; }
        else if (studyPercentile < 0.775f) { nextGrade = 4; letterGrade = "C+"; }
        else if (studyPercentile < 0.815f) { nextGrade = 5; letterGrade = "B-"; }
        else if (studyPercentile < 0.845f) { nextGrade = 6; letterGrade = "B"; }
        else if (studyPercentile < 0.885f) { nextGrade = 7; letterGrade = "B+"; }
        else if (studyPercentile < 0.915f) { nextGrade = 8; letterGrade = "A-"; }
        else if (studyPercentile < 0.966f) { nextGrade = 9; letterGrade = "A"; }
        else { nextGrade = 10; letterGrade = "A+"; }

        if (nextGrade > currentGrade)
        {
            currentGrade = nextGrade;
            studyGrade.text = letterGrade;
        }
    }
}
