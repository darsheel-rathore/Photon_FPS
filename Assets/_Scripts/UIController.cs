using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviourPunCallbacks
{
    public static UIController instance; // Singleton instance

    // UI elements
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
        instance = this; // Set the singleton instance to this script
        optionsPanel.SetActive(false); // Deactivate the options panel on awake
    }

    private void Update()
    {
        // Check for Escape key press to show/hide options
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }
    }

    // Toggle the overheat message
    public void ToggleOverHeatMsg(bool value)
    {
        overHeatedMessage.enabled = value;
    }

    // Update the weapon overheat slider
    public void OverHeatSliderUpdate(float value)
    {
        weaponTempSlider.value = value;
    }

    // Update the health slider
    public void UpdateHealthSlider(float value)
    {
        healthSlider.value = value;
    }

    // Show or hide the options panel
    public void ShowHideOptions()
    {
        if (!optionsPanel.activeInHierarchy)
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

    // Return to the main menu
    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    // Quit the game
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
