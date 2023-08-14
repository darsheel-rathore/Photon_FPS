using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public Slider weaponTempSlider;
    public TMP_Text overHeatedMessage;

    private void Awake()
    {
        instance = this;
    }

    public void ToggleOverHeatMsg(bool value)
    {
        overHeatedMessage.enabled = value;
    }

    public void OverHeatSliderUpdate(float value)
    {
        weaponTempSlider.value = value;

    }
}
