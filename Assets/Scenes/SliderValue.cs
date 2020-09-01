using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour
{
    public Slider slider;
    public Text value_text;
    float value;

    void Start()
    {
        slider = slider.GetComponent<Slider>();
    }

    void Update()
    {
        value_text.GetComponent<Text>().text = slider.value.ToString("f3");
    }
}
