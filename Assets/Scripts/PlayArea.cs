using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayArea : MonoBehaviour
{
    [SerializeField] SpriteRenderer monitorImage;
    [SerializeField] Image bar;
    [SerializeField] float turnonTime = 3.0f;

    private float turnonProgress = 0.0f;
    private IEnumerator coroutine;

    public bool IsTurningOn = false;
    public bool MonitorisOn = false;

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
        MonitorisOn = false;
        monitorImage.color = new Color32(0, 0, 0 ,255);
        bar.fillAmount = 0.0f;
        IsTurningOn = false;
        if (coroutine != null) StopCoroutine(coroutine);
    }

    IEnumerator TurnOn(Action callback)
    {
        yield return new WaitForSeconds(turnonTime);
        MonitorisOn = true;
        monitorImage.color = new Color32(255, 255, 0 ,255);
        callback();
        IsTurningOn = false;
        coroutine = null;
    }
}
