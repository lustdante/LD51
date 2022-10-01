using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    Vector2 rawInput;

    [SerializeField] float paddingLeft;
    [SerializeField] float paddingRight;
    [SerializeField] float paddingTop;
    [SerializeField] float paddingBottom;

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
        Move();
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
        if (value.isPressed)
        {
            // timer.gameObject.SetActive(true);
            // Timer.Instance.gameObject.SetActive(true);
            // Timer.Instance.StartTimer();
            GameManager.Instance.StartTimer();
        }
    }
}
