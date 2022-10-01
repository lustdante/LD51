using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] Timer timer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTimer()
    {
        timer.StartTimer();
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
        Debug.Log("timer ended!");
    }
}
