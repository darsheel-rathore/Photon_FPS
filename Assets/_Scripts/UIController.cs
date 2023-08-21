using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviourPunCallbacks
{
    public static UIController instance;

    public Slider weaponTempSlider;
    public TMP_Text overHeatedMessage;

    public Slider healthSlider;

    public TMP_Text killsText;
    public TMP_Text deathsText;

    public GameObject leaderBoard;
    public LeaderboardPlayer leaderBoardPlayer;

    public GameObject endScreen;

    public TMP_Text timerText;

    public GameObject optionsPanel;

    private void Awake()
    {
        instance = this;
        optionsPanel.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }
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

    public void ShowHideOptions()
    {
        if(!optionsPanel.activeInHierarchy)
        {
            optionsPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            optionsPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
