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

    private bool canStudy = false;
    private bool canPlay = false;


    Vector2 minBounds;
    Vector2 maxBounds;

    // Start is called before the first frame update
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

    void OnConfirm(InputValue value)
    {
        if (!value.isPressed) return;
        switch (GameManager.Instance.PlayerState)
        {
            case GameManager.PlayerActionState.Studying:
                GameManager.Instance.PlayerState = GameManager.PlayerActionState.Idle;
                displayText.text = "";
                studyCollider.enabled = true;
                break;
            case GameManager.PlayerActionState.Playing:
                GameManager.Instance.PlayerState = GameManager.PlayerActionState.Idle;
                displayText.text = "";
                playCollider.enabled = true;
                break;
        }
    }

    void OnAct(InputValue value)
    {
        if (!value.isPressed) return;
        switch (GameManager.Instance.PlayerState)
        {
            case GameManager.PlayerActionState.Idle:
                if (canStudy)
                {
                    GameManager.Instance.PlayerState = GameManager.PlayerActionState.Studying;
                    canStudy = false;
                    studyCollider.enabled = false;
                    displayText.text = "Press Z to exit";
                }

                if (canPlay)
                {
                    GameManager.Instance.PlayerState = GameManager.PlayerActionState.Playing;
                    canPlay = false;
                    playCollider.enabled = false;
                    displayText.text = "Press Z to exit";;
                }
                break;            
        }
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.tag == "StudyArea")
        {
            canStudy = true;
            displayText.text = "Press X to Study";
        }

        if (other.tag == "PlayArea")
        {
            canPlay = true;
            displayText.text = "Press X to Play";
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "StudyArea")
        {
            canStudy = false;
            displayText.text = "";
        }

        if (other.tag == "PlayArea")
        {
            canPlay = false;
            displayText.text = "";
        }
    }
}
