using UnityEngine;
using UnityEngine.UI;

public class Meter : MonoBehaviour
{
    [SerializeField] private Image bar;

    public float MaxValue = 1.0f;
    private float _value = 0.0f;

    public float Value
    {
        get { return _value; }
        set {
            bar.fillAmount = Mathf.Lerp(bar.fillAmount, value / MaxValue, Time.deltaTime * 10f);
            this._value = value;
        }
    }

    private void Start()
    {
        bar.fillAmount = 0.0f;
    }
}
