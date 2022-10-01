using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    public enum PlayerActionState {Idle, Studying, Playing, Napping};

    [SerializeField] Transform doorPos;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Timer timer;
    [SerializeField] Meter studyMeter;
    [SerializeField] Meter playMeter;
    [SerializeField] Meter boredomMeter;
    [SerializeField] Meter focusMeter;
    [SerializeField] Meter tiredMeter;
    [SerializeField] float studyGoal = 100.0f;
    [SerializeField] float playGoal = 100.0f;
    [SerializeField] float boredomMax = 6.0f;
    [SerializeField] float boredomIdleDecay = 0.05f;
    [SerializeField] float boredomPlayDecay = 1.0f;
    [SerializeField] float focusMax = 3.0f;
    [SerializeField] float focusDecay = 1.5f;
    [SerializeField] float tiredMax = 30.0f;
    [SerializeField] float tiredStudyRate = 1.5f;
    [SerializeField] float tiredDecay = 6.0f;
    [SerializeField] TextMeshProUGUI studyMultText;

    private float studyProgress = 0.0f;
    private float playProgress = 0.0f;
    private float boredomProgress = 0.0f;
    private float focusProgress = 0.0f;
    private float tiredProgress = 0.0f;

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
    }

    private void Update()
    {
        float boredomMult = 1.0f;
        float focusMult = 1.0f;
        string studyMults = "";

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
                tiredProgress = Mathf.Clamp(tiredProgress + Time.deltaTime, 0, tiredMax);
                tiredMeter.Value = tiredProgress;
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
                studyMultText.text = studyMults;

                studyProgress = Mathf.Clamp(studyProgress + Time.deltaTime * boredomMult * focusMult, 0, studyGoal);
                studyMeter.Value = studyProgress;
                boredomProgress = Mathf.Clamp(boredomProgress + Time.deltaTime, 0, boredomMax);
                boredomMeter.Value = boredomProgress;
                focusProgress = Mathf.Clamp(focusProgress + Time.deltaTime, 0, focusMax);
                focusMeter.Value = focusProgress;
                tiredProgress = Mathf.Clamp(tiredProgress + Time.deltaTime * tiredStudyRate, 0, tiredMax);
                tiredMeter.Value = tiredProgress;
                break;
            case PlayerActionState.Napping:
                tiredProgress = Mathf.Clamp(tiredProgress - Time.deltaTime * tiredDecay, 0, tiredMax);
                tiredMeter.Value = tiredProgress;
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
