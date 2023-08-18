using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public Slider weaponTempSlider;
    public TMP_Text overHeatedMessage;

    public Slider healthSlider;

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

    public void UpdateHealthSlider(float value)
    {
        healthSlider.value = value;
    }
}
