using System;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    public GameObject loadingScreen, menubuttons;
    public TMP_Text loadingText;

    public GameObject createRoomScreen;
    public TMP_InputField roomNameInput;

    public GameObject roomPanel;
    public TMP_Text roomNameText;

    public GameObject errorPanel;
    public TMP_Text errorText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CloseMenu();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Network...";

        PhotonNetwork.ConnectUsingSettings();
    }

    private void CloseMenu()
    {
        loadingScreen.SetActive(false);
        menubuttons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomPanel.SetActive(false);
        errorPanel.SetActive(false);
    }

    // PUN Callbacks
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        loadingText.text = "Joining Lobby...";
    }
    public override void OnJoinedLobby()
    {
        CloseMenu();
        menubuttons.SetActive(true);
    }
    public override void OnJoinedRoom()
    {
        CloseMenu();
        roomPanel.SetActive(true);
        roomNameText.text = $"Joined - {PhotonNetwork.CurrentRoom.Name}";
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenu();
        errorPanel.SetActive(true);
        errorText.text = $" Error Code : {returnCode}\n{message}";
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        CloseMenu();
        errorPanel.SetActive(true);
        errorText.text = $" Error Code : {returnCode}\n{message}";
    }
    //


    // Button Callbacks
    public void OnFindRoomButtonClicked()
    {
        createRoomScreen.SetActive(true);
    }
    public void OnCreateRoomButtonClicked()
    {
        var roomName = (roomNameInput.text != string.Empty) ? roomNameInput.text : ("Room" + UnityEngine.Random.Range(1, 1000));

        if (roomName != string.Empty)
        {
            RoomOptions roomOptions = new RoomOptions()
            {
                IsOpen = true,
                IsVisible = true,
                MaxPlayers = 8,
                CleanupCacheOnLeave = true,
            };

            try
            {
                PhotonNetwork.CreateRoom(roomName, roomOptions);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            // Close the whole menu
            CloseMenu();
            loadingScreen.SetActive(true);
            loadingText.text = "Creating A Room...";
        }

        Debug.Log(roomName);
    }

    public void OnFailedRoomCreateButtonClicked()
    {
        roomNameInput.text = $"Room : {UnityEngine.Random.Range(0, 1000)}";
        OnCreateRoomButtonClicked();
    }
    ///
}
