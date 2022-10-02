using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum PlayerActionState {Idle, Studying, Playing, Napping};
public enum MotherState {Safe, Alert};

public class GameManager : Singleton<GameManager>
{
    public static Action<int> OnPlayerLevelUp;

    [SerializeField] Door door;
    [SerializeField] Transform doorPos;
    [SerializeField] public Transform StudyPos;
    [SerializeField] public Transform PlayPos;
    [SerializeField] public Transform NapPos;
    [SerializeField] public GameObject Chair;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject mother;
    [SerializeField] Timer timer;
    [SerializeField] Clock clock;
    [SerializeField] Image StatusPanel;
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
    [SerializeField] TextMeshProUGUI strikeText;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] Image gameOverPanel;
    [SerializeField] Image dayOverPanel;
    [SerializeField] Collider2D studyCollider;
    [SerializeField] Collider2D playCollider;
    [SerializeField] Collider2D sleepCollider;
    [SerializeField] PlayArea playArea;

    private bool expEventStarted = false;
    private bool isPlayingIntro = false;
    private float studyProgress = 0.0f;
    private float playProgress = 0.0f;
    private float boredomProgress = 0.0f;
    private float focusProgress = 0.0f;
    private float tiredProgress = 0.0f;
    private Player player;

    private int strikeCount = 0;
    private int currentGrade = -1;
    private int currentLevel = -1;
    private int currentCycle = 0;
    private IEnumerator mainGameLoop;
    private IEnumerator introCoroutine;

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

    private MotherState motherState = MotherState.Safe;

    public bool IsGameStarted = false;
    public bool IsGamePaused = true;

    public PlayArea PlayArea { get { return playArea; } }

    private void Start()
    {
        studyMeter.MaxValue = studyGoal;
        playMeter.MaxValue = playGoal;
        boredomMeter.MaxValue = boredomMax;
        focusMeter.MaxValue = focusMax;
        tiredMeter.MaxValue = tiredMax;

        UpdatePlayerLevelText();
        UpdateStudyGradeText();

        StartCoroutine(PlayerWalksIntoRoom());
    }

    IEnumerator PlayerWalksIntoRoom()
    {
        // Initial game delay
        yield return new WaitForSeconds(3.0f);
        door.OpenDoor();
        GameObject go = Instantiate(playerPrefab, doorPos.position, Quaternion.identity);
        player = go.GetComponent<Player>();

        // Delay after player walks in
        yield return new WaitForSeconds(1.0f);

        door.CloseDoor();
        yield return new WaitForSeconds(0.5f);

        IsGamePaused = false;
    }


    void PlayIntro()
    {
        introCoroutine = MotherWalksInFirstTime();
        StartCoroutine(introCoroutine);
    }

    IEnumerator MotherWalksInFirstTime()
    {
        // Knock time
        yield return new WaitForSeconds(1.0f);
        door.OpenDoor();
        mother.SetActive(true);
        
        // This is where mom talks about all the rules
    }

    IEnumerator MotherWalksOutFirstTime()
    {
        yield return new WaitForSeconds(0.5f);
        door.CloseDoor();
        mother.SetActive(false);
    }

    public void StartGame()
    {
        // If introCoroutine was playing, abort it
        if (introCoroutine != null)
        {
            StopCoroutine(introCoroutine);
            StartCoroutine(MotherWalksOutFirstTime());
        }
    
        IsGameStarted = true;
        StatusPanel.gameObject.SetActive(true);
        clock.gameObject.SetActive(true);
        currentCycle = 0;
        mainGameLoop = MainGameLoop();
        StartCoroutine(mainGameLoop);
    }

    private void Update()
    {
        float boredomMult = 1.0f;
        float focusMult = 1.0f;
        float tiredMult = 1.0f;
        float gameMult = 1.0f;
        string studyMults = "";

        if (IsGamePaused) return;

        if (motherState == MotherState.Alert)
        {
            if (playerState == PlayerActionState.Playing)
            {
                HandleGameOver();
                return;
            }
        }

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
                if (!IsGameStarted)
                {
                    playProgress = Mathf.Clamp(playProgress + Time.deltaTime * gameMult, 0, 1.0f);
                    playMeter.Value = playProgress;

                    if (!isPlayingIntro && playProgress >= 1.0f)
                    {
                        isPlayingIntro = true;
                        PlayIntro();
                    }
                }
                else
                {
                    if (expEventStarted) gameMult = 2.0f;
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
                }
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

    public void HandleGameOver()
    {
        IsGamePaused = true;
        if (mainGameLoop != null) StopCoroutine(mainGameLoop);
        gameOverPanel.gameObject.SetActive(true);
    }

    public void BackToTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }

    private void OnEnable()
    {
        Clock.OnGameEvent += HandleGameEvent;
        Clock.OnDayEnd += HandleDayOver;
    }

    private void OnDisable()
    {
        Clock.OnGameEvent -= HandleGameEvent;
        Clock.OnDayEnd -= HandleDayOver;
    }

    void HandleGameEvent(GameEventTime eventTime)
    {
        switch (eventTime.gameEvent)
        {
            case GameEvent.EventStart:
                expEventStarted = true;
                eventText.gameObject.SetActive(true);
                break;
            case GameEvent.EventEnd:
                expEventStarted = false;
                eventText.gameObject.SetActive(false);
                break;
        }
    }

    void HandleDayOver()
    {
        IsGamePaused = true;
        if (mainGameLoop != null) StopCoroutine(mainGameLoop);
        dayOverPanel.gameObject.SetActive(true);

        float studyPercentile = studyProgress / studyGoal;
        string output = "";

        if (studyPercentile < 0.6f) { output += "I failed the exam. My mom is going to kill me."; }
        else if (studyPercentile < 0.675f) { output += "I got D in the exam. My mom is very disappointed."; }
        else if (studyPercentile < 0.705f) { output += "I got C- in the exam. My mom is very disappointed."; }
        else if (studyPercentile < 0.745f) { output += "I got C in the exam. My mom is very disappointed."; }
        else if (studyPercentile < 0.775f) { output += "I got C+ in the exam. My mom is a bit disappointed."; }
        else if (studyPercentile < 0.815f) { output += "I got B- in the exam. My mom is a bit disappointed."; }
        else if (studyPercentile < 0.845f) { output += "I got B in the exam. My mom is slightly disappointed."; }
        else if (studyPercentile < 0.885f) { output += "I got B+ in the exam. My mom is slightly disappointed."; }
        else if (studyPercentile < 0.915f) { output += "I got A- in the exam. My mom is content."; }
        else if (studyPercentile < 0.966f) { output += "I got A in the exam. My mom is content."; }
        else { output += "I got A+! My mom will finally approve me!"; }

        output += "\n\n";

        if (playProgress == playGoal)
        {
            output += "I maxed out the level and I won the exclusive sword!";
        }
        else
        {
            output += "Despite all the effort, I could not win the exclusive event reward.";
        }

        resultText.text = output;
    }

    public void EnableColliders(bool enabled)
    {
        studyCollider.enabled = enabled;
        playCollider.enabled = enabled;
        sleepCollider.enabled = enabled;
    }

    IEnumerator MainGameLoop()
    {
        while (true) {
            yield return timer.StartTimerUntilDone(currentCycle < 8);
            yield return RunTestEvent();
            currentCycle += 1;
        }
    }

    IEnumerator RunTestEvent()
    {
        // Knock time
        yield return new WaitForSeconds(1.0f);
        door.OpenDoor();
        mother.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        motherState = MotherState.Alert;
        bool youGetStrike = (playerState != PlayerActionState.Studying && playerState != PlayerActionState.Playing) || playArea.MonitorisOn;
        if (youGetStrike)
        {
            IsGamePaused = true;
            yield return new WaitForSeconds(1.0f);
            IncrementAndUpdateStrikeText();
            if (strikeCount >= 3)
            {
                HandleGameOver();
                yield return null;
            }
            yield return new WaitForSeconds(1.0f);
            IsGamePaused = false;
            motherState = MotherState.Safe;
        }
        else
        {
            float stayingDuration = UnityEngine.Random.Range(1.0f, 3.0f);
            yield return new WaitForSeconds(stayingDuration);

            motherState = MotherState.Safe;
            yield return new WaitForSeconds(0.1f);
        }
        door.CloseDoor();
        mother.SetActive(false);
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

    void IncrementAndUpdateStrikeText()
    {
        strikeCount += 1;
        string text = "Strikes - ";
        foreach (var i in Enumerable.Range(0, 3 - strikeCount)) text += "0";
        foreach (var i in Enumerable.Range(0, strikeCount)) text += "X";
        strikeText.text = text;
    }
}
