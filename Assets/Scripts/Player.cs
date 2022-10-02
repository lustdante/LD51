using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    Vector2 rawInput;

    [SerializeField] private Image speechBubble;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] float paddingLeft;
    [SerializeField] float paddingRight;
    [SerializeField] float paddingTop;
    [SerializeField] float paddingBottom;

    private PlayArea playArea;
    private PlayerActionState actionableState = PlayerActionState.Idle;
    private Rigidbody2D body;

    Vector2 minBounds;
    Vector2 maxBounds;
    
    public string DisplayText {
        get { return displayText.text; }
        set {
            if (value == "") speechBubble.gameObject.SetActive(false);
            else speechBubble.gameObject.SetActive(true);
            displayText.text = value;
        }
    }

    void Start()
    {
        InitCameraBounds();
        body = GetComponent<Rigidbody2D>();
        playArea = GameManager.Instance.PlayArea;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.IsGamePaused) return;
        if (GameManager.Instance.PlayerState == PlayerActionState.Idle)
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
        body.MovePosition(newPos);
        // transform.position = newPos;
    }

    void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();
    }

    void OnMonitorOn()
    {
        GameManager.Instance.EnableColliders(false);
        GameManager.Instance.EnableColliders(true);
    }

    void OnConfirm(InputValue value)
    {
        if (!value.isPressed) return;
        if (actionableState == PlayerActionState.Playing)
        {
            if (playArea.MonitorisOn || playArea.IsTurningOn)
            {
                playArea.TurnOffMonitor();
                DisplayText = "Pree Z to turn on Monitor";
            }
            else
            {
                playArea.TurnOnMonitor(OnMonitorOn);
                DisplayText = "Pree Z to turn off Monitor";
            }
        }
    }

    void OnAct(InputValue value)
    {
        if (GameManager.Instance.IsGamePaused) return;

        if (value.isPressed)
        {
            if (!GameManager.Instance.IsGameStarted)
            {
                bool canStartGame =
                    GameManager.Instance.PlayerState == PlayerActionState.Idle &&
                    actionableState == PlayerActionState.Studying &&
                    !playArea.MonitorisOn;

                // Game loops starts when first start studying
                if (canStartGame) GameManager.Instance.StartGame();
            }

            switch (GameManager.Instance.PlayerState)
            {
                case PlayerActionState.Idle:
                    if (actionableState == PlayerActionState.Playing && !playArea.MonitorisOn) break;
                    GameManager.Instance.PlayerState = actionableState;
                    actionableState = PlayerActionState.Idle;
                    GameManager.Instance.EnableColliders(false);
                    DisplayText = "";
                    break;            
            }
        }
        else
        {
            GameManager.Instance.PlayerState = PlayerActionState.Idle;
            GameManager.Instance.EnableColliders(true);
        }
        
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (!GameManager.Instance.IsGameStarted)
        {
            switch (other.tag)
            {
                case "StudyArea":
                    actionableState = PlayerActionState.Studying;
                    if (playArea.MonitorisOn) DisplayText = "You must turn off the monitor";
                    else DisplayText = "Hold X to Study and Start Day";
                    break;
                case "PlayArea":
                    actionableState = PlayerActionState.Playing;
                    if (playArea.MonitorisOn) DisplayText = "Hold X to Play\nZ to turn it off";
                    else if (!playArea.IsTurningOn) DisplayText = "Pree Z to turn on Monitor";
                    else DisplayText = "Pree Z to turn off Monitor";
                    break;
            }
            return;
        }

        switch (other.tag)
        {
            case "StudyArea":
                actionableState = PlayerActionState.Studying;
                DisplayText = "Hold X to Study";
                break;
            case "PlayArea":
                actionableState = PlayerActionState.Playing;
                if (playArea.MonitorisOn) DisplayText = "Hold X to Play\nZ to turn it off";
                else if (!playArea.IsTurningOn) DisplayText = "Pree Z to turn on Monitor";
                else DisplayText = "Pree Z to turn off Monitor";
                break;
            case "SleepArea":
                actionableState = PlayerActionState.Napping;
                DisplayText = "Hold X to Nap";
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
                actionableState = PlayerActionState.Idle;
                DisplayText = "";
                break;
        }
    }
}
