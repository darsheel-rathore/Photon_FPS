using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    public GameObject loadingScreen, menubuttons;
    public TMP_Text loadingText;

    public GameObject createRoomScreen;
    public TMP_InputField roomNameInput;

    public GameObject roomPanel;
    public TMP_Text roomNameText;
    public TMP_Text playerNameLabel;
    private List<TMP_Text> allPlayernames = new List<TMP_Text>();
    public GameObject playerListParent;


    public GameObject errorPanel;
    public TMP_Text errorText;

    public GameObject roomBrowserScreen;
    public RoomButton theRoomButton;
    public GameObject content;
    public List<RoomButton> roomButtons = new List<RoomButton>();



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

    private void Update()
    {
        Debug.Log("----" + PhotonNetwork.CountOfRooms);
    }

    private void CloseMenu()
    {
        loadingScreen.SetActive(false);
        menubuttons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomPanel.SetActive(false);
        errorPanel.SetActive(false);
        roomBrowserScreen.SetActive(false);
    }
    private void ListAllPlayers()
    {
        foreach (var player in allPlayernames)
        {
            Destroy(player.gameObject);
        }

        allPlayernames.Clear();

        Player[] players = PhotonNetwork.PlayerList;

        foreach (var newPlayer in players)
        {
            var newPlayerLabel = Instantiate(playerNameLabel, transform.position, Quaternion.identity, playerListParent.transform);
            newPlayerLabel.text = newPlayer.NickName;
            newPlayerLabel.gameObject.SetActive(true);
            allPlayernames.Add(newPlayerLabel);
        }

    }

    #region PUN-CALLBACKS
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        loadingText.text = "Joining Lobby...";
    }
    public override void OnJoinedLobby()
    {
        CloseMenu();
        menubuttons.SetActive(true);

        PhotonNetwork.NickName = "Player" + UnityEngine.Random.Range(0, 100);
    }
    public override void OnJoinedRoom()
    {
        CloseMenu();
        roomPanel.SetActive(true);
        roomNameText.text = $"Joined - {PhotonNetwork.CurrentRoom.Name}";

        ListAllPlayers();
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
    public override void OnLeftLobby()
    {
        CloseMenu() ;
        menubuttons.SetActive(true);
    }
    public override void OnLeftRoom()
    {
        CloseMenu();
        menubuttons.SetActive(true);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in roomButtons)
        {
            Destroy(rb.gameObject);
        }
        roomButtons.Clear();

        for(int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.position, Quaternion.identity, content.transform);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                roomButtons.Add(newButton);
            }
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        var newPlayerLabel = Instantiate(playerNameLabel, transform.position, Quaternion.identity, playerListParent.transform);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);
        allPlayernames.Add(newPlayerLabel);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer != null)
        {
            List<TMP_Text> playersToRemove = new List<TMP_Text>();

            foreach (var player in allPlayernames)
            {
                if (player.text == otherPlayer.NickName)
                {
                    playersToRemove.Add(player);
                }
            }

            foreach (var playerToRemove in playersToRemove)
            {
                allPlayernames.Remove(playerToRemove);
                Destroy(playerToRemove.gameObject);
            }
        }
    }
    #endregion

    #region Button OnClick Events
    public void OnFindRoomButtonClicked()
    {
        if (PhotonNetwork.CountOfRooms > 0)
        {
            OpenRoomBrowser();
        }
        else
        {
            createRoomScreen.SetActive(true);
        }
    }
    public void OnCreateRoomButtonClicked()
    {
        var roomName = (roomNameInput.text != string.Empty) ? roomNameInput.text : ("Room" + UnityEngine.Random.Range(1, 1000));
        //var roomName = roomNameInput?.text ?? ("Room" + UnityEngine.Random.Range(1, 1000));

        if (roomName != string.Empty)
        {
            RoomOptions roomOptions = new RoomOptions()
            {
                IsOpen = true,
                IsVisible = true,
                MaxPlayers = 5,
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
    public void OnLeaveRoomButtonClicked()
    {
        PhotonNetwork.LeaveRoom();

        CloseMenu();
        loadingScreen.SetActive(true);
        loadingText.text = "Leaving Room...";
    }
    public void OnLeaveLobbyButtonClicked()
    {
        CloseMenu();
        loadingText.text = "Leaving Lobby";
        loadingScreen.SetActive(true);
    }
    public void OpenRoomBrowser()
    {
        CloseMenu();
        roomBrowserScreen.SetActive(true);
    }
    public void CloseRoomBrowser()
    {
        CloseMenu();
        menubuttons.SetActive(true);
    }
    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenu();
        loadingText.text = "Joining Room...";
        loadingScreen.SetActive(true);
    }
    public void Quit()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
    #endregion
}
