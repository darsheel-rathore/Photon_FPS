using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UIController : MonoBehaviourPunCallbacks
{
    public static UIController instance;

    public Slider weaponTempSlider;
    public TMP_Text overHeatedMessage;

    public Slider healthSlider;

    public TMP_Text killsText;
    public TMP_Text deathsText;

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
