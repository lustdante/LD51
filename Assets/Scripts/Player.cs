using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    Vector2 rawInput;

    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] float paddingLeft;
    [SerializeField] float paddingRight;
    [SerializeField] float paddingTop;
    [SerializeField] float paddingBottom;
    [SerializeField] Collider2D studyCollider;
    [SerializeField] Collider2D playCollider;
    [SerializeField] Collider2D sleepCollider;
    [SerializeField] PlayArea playArea;

    private GameManager.PlayerActionState actionableState = GameManager.PlayerActionState.Idle;

    Vector2 minBounds;
    Vector2 maxBounds;

    void Start()
    {
        InitCameraBounds();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.PlayerState == GameManager.PlayerActionState.Idle)
        {
            Move();
        }
    }

    void InitCameraBounds()
    {
        Camera mainCamera = Camera.main;
        minBounds = mainCamera.ViewportToWorldPoint(new Vector2(0,0));
        maxBounds = mainCamera.ViewportToWorldPoint(new Vector2(1,1));
    }

    void Move()
    {
        Vector2 delta = rawInput;
        Vector2 newPos = new Vector2(transform.position.x + delta.x, transform.position.y + delta.y);
        newPos = Vector2.MoveTowards(transform.position, newPos, Time.deltaTime * moveSpeed);
        newPos.x = Mathf.Clamp(newPos.x, minBounds.x + paddingLeft, maxBounds.x - paddingRight);
        newPos.y = Mathf.Clamp(newPos.y, minBounds.y + paddingBottom, maxBounds.y - paddingTop);
        transform.position = newPos;
    }

    void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();
    }

    void OnMonitorOn()
    {
        playCollider.enabled = false;
        playCollider.enabled = true;
    }

    void OnConfirm(InputValue value)
    {
        if (!value.isPressed) return;
        if (actionableState == GameManager.PlayerActionState.Playing)
        {
            if (playArea.MonitorisOn || playArea.IsTurningOn)
            {
                playArea.TurnOffMonitor();
                displayText.text = "Pree Z to turn on Monitor";
            }
            else
            {
                playArea.TurnOnMonitor(OnMonitorOn);
                displayText.text = "Pree Z to turn off Monitor";
            }
        }
    }

    void OnAct(InputValue value)
    {
        if (value.isPressed)
        {
            switch (GameManager.Instance.PlayerState)
            {
                case GameManager.PlayerActionState.Idle:
                    GameManager.Instance.PlayerState = actionableState;
                    actionableState = GameManager.PlayerActionState.Idle;
                    studyCollider.enabled = false;
                    playCollider.enabled = false;
                    sleepCollider.enabled = false;
                    displayText.text = "";
                    break;            
            }
        }
        else
        {
            GameManager.Instance.PlayerState = GameManager.PlayerActionState.Idle;
            studyCollider.enabled = true;
            playCollider.enabled = true;
            sleepCollider.enabled = true;
        }
        
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        switch (other.tag)
        {
            case "StudyArea":
                actionableState = GameManager.PlayerActionState.Studying;
                displayText.text = "Hold X to Study";
                break;
            case "PlayArea":
                actionableState = GameManager.PlayerActionState.Playing;

                if (playArea.MonitorisOn)
                {
                    displayText.text = "Hold X to Play\nPress Z to turn off Monitor";
                }
                else
                {
                    if (!playArea.IsTurningOn) displayText.text = "Pree Z to turn on Monitor";
                }

                break;
            case "SleepArea":
                actionableState = GameManager.PlayerActionState.Napping;
                displayText.text = "Hold X to Nap";
                break;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "StudyArea":
            case "PlayArea":
            case "SleepArea":
                actionableState = GameManager.PlayerActionState.Idle;
                displayText.text = "";
                break;
        }
    }
}
