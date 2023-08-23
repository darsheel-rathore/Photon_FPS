using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public GameObject nameInputScreen;
    public TMP_InputField nameInput;
    public static bool _hasSetNickname;

    public string levelToPlay;

    public GameObject startGameButton;
    public GameObject testButton;

    public string[] allMaps;
    public bool changeMapBetweenRounds = true;

    public GameObject howToPlayButton;
    public GameObject howToPlayInstructionPanel;

    #region UNITY_BUILTIN
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        // Close any active UI screens
        CloseMenu();

        // Display loading screen while connecting to the network
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Network...";

        // Connect to Photon network if not already connected
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

#if UNITY_EDITOR
        // Activate the test button in the Unity Editor
        testButton.SetActive(true);
#endif

        // Reset cursor state and make it visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        //Debug.Log("Rooms Count: " + PhotonNetwork.CountOfRooms);
    }
    #endregion

    private void CloseMenu()
    {
        // Deactivate all the UI screens to close the menu
        loadingScreen.SetActive(false);
        menubuttons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomPanel.SetActive(false);
        errorPanel.SetActive(false);
        roomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
        howToPlayButton.SetActive(false);   
        howToPlayInstructionPanel.SetActive(false);
    }
    private void ListAllPlayers()
    {
        // Clear the previous player name labels
        foreach (var player in allPlayernames)
        {
            Destroy(player.gameObject);
        }
        allPlayernames.Clear();

        // Get the current list of players in the room
        Player[] players = PhotonNetwork.PlayerList;

        // Create and display a player name label for each player
        foreach (var newPlayer in players)
        {
            // Instantiate a new player name label and set its properties
            var newPlayerLabel = Instantiate(playerNameLabel, transform.position, Quaternion.identity, playerListParent.transform);
            newPlayerLabel.text = newPlayer.NickName;
            newPlayerLabel.gameObject.SetActive(true);
            allPlayernames.Add(newPlayerLabel); // Add the label to the list
        }
    }

    #region PUN-CALLBACKS
    // Photon PUN callbacks
    public override void OnConnectedToMaster()
    {
        // Automatically sync scenes when connected to master server
        PhotonNetwork.AutomaticallySyncScene = true;

        // Join the lobby after connecting
        PhotonNetwork.JoinLobby();

        // Display loading message
        loadingText.text = "Joining Lobby...";
    }
    public override void OnJoinedLobby()
    {
        // When joined the lobby, enable the main menu buttons
        CloseMenu();
        menubuttons.SetActive(true);
        howToPlayButton.SetActive(true);

        // If nickname is not set, show the name input screen
        if (!_hasSetNickname)
        {
            CloseMenu();
            nameInputScreen.SetActive(true);
        }
    }
    public override void OnJoinedRoom()
    {
        // When joined a room, show the room panel and set its properties
        CloseMenu();
        roomPanel.SetActive(true);
        roomNameText.text = $"Joined - {PhotonNetwork.CurrentRoom.Name}";

        // List all players currently in the room
        ListAllPlayers();

        // Show start game button for the master client
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // If creating a room failed, show error panel with error details
        CloseMenu();
        errorPanel.SetActive(true);
        errorText.text = $" Error Code : {returnCode}\n{message}";
    }

    // Other Photon PUN callbacks are handled here
    // ...

    // Called when the room list is updated
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Clear existing room buttons
        foreach (RoomButton rb in roomButtons)
        {
            Destroy(rb.gameObject);
        }
        roomButtons.Clear();

        // Create new room buttons based on the updated room list
        for (int i = 0; i < roomList.Count; i++)
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

    // Called when a new player enters the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Instantiate a player name label for the new player
        var newPlayerLabel = Instantiate(playerNameLabel, transform.position, Quaternion.identity, playerListParent.transform);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);
        allPlayernames.Add(newPlayerLabel);
    }

    // Called when a player leaves the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer != null)
        {
            // Remove the player's name label from the list and destroy it
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

    // Called when the master client switches
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Show or hide start game button based on whether the local player is now the master client
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }
    }
    #endregion

    #region Button OnClick Events
    public void OnFindRoomButtonClicked()
    {
        // Open room browser if there are available rooms, otherwise show room creation screen
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
        // Get the desired room name from input, or generate a random one
        var roomName = (roomNameInput.text != string.Empty) ? roomNameInput.text : ("Room" + UnityEngine.Random.Range(1, 1000));

        // Create a new room with specified options
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

            // Close the entire menu and show loading screen
            CloseMenu();
            loadingScreen.SetActive(true);
            loadingText.text = "Creating A Room...";
        }
    }
    public void OnFailedRoomCreateButtonClicked()
    {
        // Automatically set a random room name and attempt to create a room
        roomNameInput.text = $"Room : {UnityEngine.Random.Range(0, 1000)}";
        OnCreateRoomButtonClicked();
    }
    public void OnLeaveRoomButtonClicked()
    {
        // Leave the current room and show loading screen
        PhotonNetwork.LeaveRoom();

        CloseMenu();
        loadingScreen.SetActive(true);
        loadingText.text = "Leaving Room...";
    }
    public void OnLeaveLobbyButtonClicked()
    {
        // Close the menu and show loading screen when leaving the lobby
        CloseMenu();
        loadingText.text = "Leaving Lobby";
        loadingScreen.SetActive(true);
    }
    public void OpenRoomBrowser()
    {
        // Open the room browser screen and close other menus
        CloseMenu();
        roomBrowserScreen.SetActive(true);
    }
    public void CloseRoomBrowser()
    {
        // Close the room browser and show the main menu buttons
        CloseMenu();
        menubuttons.SetActive(true);
    }
    public void JoinRoom(RoomInfo inputInfo)
    {
        // Join a specific room and show loading screen
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenu();
        loadingText.text = "Joining Room...";
        loadingScreen.SetActive(true);
    }
    public void Quit()
    {
        // Quit the application (or stop play mode in Unity Editor)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
    public void SetNickName()
    {
        // Set the player's nickname and show the main menu buttons
        PhotonNetwork.NickName = (!string.IsNullOrEmpty(nameInput.text)) ? nameInput.text : ("Player" + UnityEngine.Random.Range(1, 1000));

        if (PhotonNetwork.NickName != string.Empty)
        {
            _hasSetNickname = true;
            CloseMenu();
            menubuttons.SetActive(true);
            howToPlayButton.SetActive(true);    
        }
        else
        {
            Debug.Log("Something Wrong with the nickname");
        }
    }
    public void StartGame()
    {
        // Load a random map from the available map list
        PhotonNetwork.LoadLevel(allMaps[UnityEngine.Random.Range(0, 2)]);
    }
    public void QuickJoin()
    {
        // Create a room named "Test" for quick joining and show loading screen
        PhotonNetwork.CreateRoom("Test");
        CloseMenu();
        loadingText.text = "Creating Room...";
        loadingScreen.SetActive(true);
    }
    #endregion
}
