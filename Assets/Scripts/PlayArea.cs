using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayArea : MonoBehaviour
{
    [SerializeField] SpriteRenderer monitorImage;
    [SerializeField] Image bar;
    [SerializeField] float turnonTime = 2.0f;

    private float turnonProgress = 0.0f;
    private IEnumerator coroutine;

    public bool IsTurningOn = false;
    public bool MonitorisOn = false;

    void Start()
    {
        if (MonitorisOn) monitorImage.color = new Color(1f, 1f, 1f, 1f);
        else monitorImage.color = new Color(1f, 1f, 1f, 0f);
    }

    void Update()
    {
        if (IsTurningOn)
        {
            turnonProgress += Time.deltaTime;
            bar.fillAmount = Mathf.Lerp(bar.fillAmount, (turnonTime - turnonProgress) / turnonTime, Time.deltaTime * 10f);    
        }
    }

    public void TurnOnMonitor(Action callback)
    {
        if (IsTurningOn) return;
        bar.fillAmount = 1.0f;
        turnonProgress = 0.0f;
        IsTurningOn = true;
        coroutine = TurnOn(callback);
        StartCoroutine(coroutine);
    }

    public void TurnOffMonitor()
    {
        monitorImage.DOKill();
        MonitorisOn = false;
        monitorImage.color = new Color(1f, 1f, 1f, 0f);
        bar.fillAmount = 0.0f;
        IsTurningOn = false;
        if (coroutine != null) StopCoroutine(coroutine);
    }

    IEnumerator TurnOn(Action callback)
    {
        yield return monitorImage.DOFade(1.0f, turnonTime).SetEase(Ease.InElastic).WaitForCompletion();
        MonitorisOn = true;
        callback();
        IsTurningOn = false;
        coroutine = null;
    }
}
