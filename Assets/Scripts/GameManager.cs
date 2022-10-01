using System.Collections;
using UnityEngine;
using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
    public enum PlayerActionState {Idle, Studying, Playing};

    [SerializeField] Transform doorPos;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Timer timer;
    [SerializeField] Meter studyMeter;
    [SerializeField] Meter playMeter;
    [SerializeField] Meter boredomMeter;
    [SerializeField] Meter focusMeter;
    [SerializeField] float studyGoal = 100.0f;
    [SerializeField] float playGoal = 100.0f;
    [SerializeField] float boredomMax = 10.0f;
    [SerializeField] float boredomIdleDecay = 0.1f;
    [SerializeField] float boredomPlayDecay = 1.5f;
    [SerializeField] float focusMax = 3.0f;
    [SerializeField] float focusDecay = 1.5f;

    private float studyProgress = 0.0f;
    private float playProgress = 0.0f;
    private float boredomProgress = 0.0f;
    private float focusProgress = 0.0f;

    private PlayerActionState playerState = PlayerActionState.Idle;
    public PlayerActionState PlayerState {
        get { return this.playerState; }
        set {
            studyMeter.gameObject.SetActive(false);
            playMeter.gameObject.SetActive(false);

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
    }

    private void Update()
    {
        switch (playerState)
        {
            case PlayerActionState.Idle:
                focusProgress = Mathf.Clamp(focusProgress - focusDecay * Time.deltaTime, 0, focusMax);
                focusMeter.Value = focusProgress;
                boredomProgress = Mathf.Clamp(boredomProgress - boredomIdleDecay * Time.deltaTime, 0, boredomMax);
                boredomMeter.Value = boredomProgress;
                break;
            case PlayerActionState.Playing:
                playProgress = Mathf.Clamp(playProgress + Time.deltaTime, 0, playGoal);
                playMeter.Value = playProgress;
                boredomProgress = Mathf.Clamp(boredomProgress - boredomPlayDecay * Time.deltaTime, 0, boredomMax);
                boredomMeter.Value = boredomProgress;
                focusProgress = Mathf.Clamp(focusProgress + Time.deltaTime, 0, focusMax);
                focusMeter.Value = focusProgress;
                break;
            case PlayerActionState.Studying:
                studyProgress = Mathf.Clamp(studyProgress + Time.deltaTime, 0, studyGoal);
                studyMeter.Value = studyProgress;
                boredomProgress = Mathf.Clamp(boredomProgress + Time.deltaTime, 0, boredomMax);
                boredomMeter.Value = boredomProgress;
                focusProgress = Mathf.Clamp(focusProgress + Time.deltaTime, 0, focusMax);
                focusMeter.Value = focusProgress;
                break;
        }
    }

    private void OnEnable()
    {
        Timer.OnTimerEnded += TimerEnded;
    }

    private void OnDisable()
    {
        Timer.OnTimerEnded -= TimerEnded;
    }

    void TimerEnded()
    {
        StartCoroutine(RunTestEvent());
    }

    public void StartEventLoop()
    {
        // timer.gameObject.SetActive(true);
        // timer.StartTimer();
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
}
