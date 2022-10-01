using System.Collections;
using UnityEngine;
using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] Transform doorPos;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Timer timer;

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
