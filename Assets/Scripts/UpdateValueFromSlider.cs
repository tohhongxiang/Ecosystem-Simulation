using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdateValueFromSlider : MonoBehaviour
{
    public Slider slider;
    public TMP_Text text;
    public bool isInt = false;

    // Start is called before the first frame update
    void Start()
    {
        text.text = slider.value.ToString();
        slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    void ValueChangeCheck()
    {
        string value = isInt ? slider.value.ToString() : slider.value.ToString("0.00");
        text.text = value;
    }
}
