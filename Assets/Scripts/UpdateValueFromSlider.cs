using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdateValueFromSlider : MonoBehaviour
{
    public Slider slider;
    public TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        text.text = slider.value.ToString();
        slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    void ValueChangeCheck()
    {
        text.text = slider.value.ToString();
    }
}
