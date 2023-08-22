// Import necessary Photon-related libraries
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using System;

// Serializable class to store player information
[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor;
    public int kills;
    public int deaths;

    public PlayerInfo(string name, int actor, int kills, int deaths)
    {
        this.name = name;
        this.actor = actor;
        this.kills = kills;
        this.deaths = deaths;
    }
}

// Main class for managing the match
public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager Instance; // Singleton instance
    public List<PlayerInfo> allPlayerList; // List to store player information
    private List<LeaderboardPlayer> leaderboardPlayers; // List for leaderboard players
    private int index;

    // Enum to define game states
    public enum GameState
    {
        WAITING,
        PLAYING,
        ENDING
    }
    public int killsToWin = 3; // Number of kills needed to win
    public Transform mapCamPoint; // Transform for camera position
    public GameState currentState = GameState.WAITING; // Current game state
    public float waitAfterEnding = 5f; // Time to wait after the game ends

    // Enum to define custom event codes
    public enum EventCode : byte
    {
        NEW_PLAYER,
        LIST_PLAYER,
        CHANGE_STATE,
        NEXT_MATCH,
        TIMER_SYNC
    }
    public EventCode eventCode;
    public bool perpetual = true; // Whether the game mode is perpetual

    private float matchLength = 75f; // Total match length
    public float currentMatchTime; // Current match time
    private float sendTimer; // Timer for sending updates

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        Instance = this; // Set the singleton instance

        allPlayerList = new List<PlayerInfo>(); // Initialize the player list
        leaderboardPlayers = new List<LeaderboardPlayer>(); // Initialize the leaderboard list
    }

    // Start is called before the first frame update
    void Start()
    {
        // Check if connected to Photon network
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0); // Load the main menu scene
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName); // Send new player information
            currentState = GameState.PLAYING; // Set the game state to playing
        }

        SetUpTimer(); // Set up the match timer
    }

    // Update is called once per frame
    private void Update()
    {
        // Toggle leaderboard display with Tab key
        if (Input.GetKeyDown(KeyCode.Tab) && currentState != GameState.ENDING)
        {
            if (UIController.instance.leaderBoard.activeInHierarchy)
            {
                UIController.instance.leaderBoard.SetActive(false);
            }
            else
            {
                ShowLeaderBoard();
            }
        }

        // Check if the player is the master client (host)
        if (!PhotonNetwork.IsMasterClient)
            return;

        // Update match timer and check for match ending conditions
        if (currentMatchTime > 0 && currentState == GameState.PLAYING)
        {
            currentMatchTime -= Time.deltaTime;
            if (currentMatchTime < 0)
            {
                currentMatchTime = 0;
                currentState = GameState.ENDING;
                ListPlayersSend(); // Send player list to clients
                StateCheck(); // Check game state
            }

            UpdateTimerDisplay(); // Update timer display

            sendTimer -= Time.deltaTime;

            if (sendTimer <= 0)
            {
                sendTimer += 1f;
                TimerSend(); // Send timer update
            }
        }
    }

    // Called when the script becomes enabled and active
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    // Called when the script becomes disabled or inactive
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // Method for handling incoming Photon events
    public void OnEvent(EventData photonEvent)
    {
        // Check if the event code is within a certain range
        if (photonEvent.Code < 200)
        {
            EventCode theEvent = (EventCode)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            // Switch statement to handle different event codes
            switch (theEvent)
            {
                case EventCode.NEW_PLAYER:
                    NewPlayerReceive(data);
                    break;
                case EventCode.LIST_PLAYER:
                    ListPlayersReceive(data);
                    break;
                case EventCode.CHANGE_STATE:
                    UpdateStatsReceive(data);
                    break;
                case EventCode.NEXT_MATCH:
                    NextMatchReceive();
                    break;
                case EventCode.TIMER_SYNC:
                    TimerReceived(data);
                    break;
            }
        }
    }

    // Method to send new player information to the master client
    public void NewPlayerSend(string username)
    {
        // Create a package with player information
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        // Raise a Photon event to notify the master client
        PhotonNetwork.RaiseEvent(
            (byte)EventCode.NEW_PLAYER,
            package,
            new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient },
            new SendOptions() { Reliability = true }
        );
    }

    // Method to handle new player information received by the master client
    public void NewPlayerReceive(object[] data)
    {
        // Create a new PlayerInfo object from received data
        PlayerInfo newPlayerInfo = new PlayerInfo(
            name: (string)data[0],
            actor: (int)data[1],
            kills: (int)data[2],
            deaths: (int)data[3]
        );

        // Add the new player to the player list
        allPlayerList.Add(newPlayerInfo);

        // Send updated player list to all clients
        ListPlayersSend();
    }

    // Method to send the current player list to all clients
    public void ListPlayersSend()
    {
        // Create a package with player information for each player
        object[] package = new object[allPlayerList.Count + 1];
        package[0] = currentState;

        for (int i = 0; i < allPlayerList.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = allPlayerList[i].name;
            piece[1] = allPlayerList[i].actor;
            piece[2] = allPlayerList[i].kills;
            piece[3] = allPlayerList[i].deaths;

            package[i + 1] = piece;
        }

        // Raise a Photon event to update the player list for all clients
        PhotonNetwork.RaiseEvent(
            (byte)EventCode.LIST_PLAYER,
            package,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
            new SendOptions() { Reliability = true }
        );
    }

    // Method to handle the received player list from the master client
    public void ListPlayersReceive(object[] data)
    {
        // Clear the current player list
        allPlayerList.Clear();

        currentState = (GameState)data[0];

        // Loop through received data to reconstruct the player list
        for (int i = 1; i < data.Length; i++)
        {
            object[] piece = (object[])data[i];

            PlayerInfo player = new PlayerInfo(
                name: (string)piece[0],
                actor: (int)piece[1],
                kills: (int)piece[2],
                deaths: (int)piece[3]
            );

            allPlayerList.Add(player);

            // Check if the current player's actor number matches the local player's actor number
            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i - 1; // Store the index of the local player
            }
        }

        // Check the game state based on the received information
        StateCheck();
    }

    // Method to send updated player statistics to all clients
    public void UpdateStatsSend(int actorNumber, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorNumber, statToUpdate, amountToChange };

        // Raise a Photon event to update player statistics for all clients
        PhotonNetwork.RaiseEvent(
            (byte)EventCode.CHANGE_STATE,
            package,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
            new SendOptions() { Reliability = true }
        );
    }

    // Method to handle received player statistic updates
    public void UpdateStatsReceive(object[] data)
    {
        int actor = (int)data[0];
        int statType = (int)data[1];
        int amount = (int)data[2];

        // Iterate through the player list to find the player and update statistics
        for (int i = 0; i < allPlayerList.Count; i++)
        {
            if (allPlayerList[i].actor == actor)
            {
                // Update player kills or deaths based on the received statType
                if (statType == 0)
                    allPlayerList[i].kills += amount;
                else if (statType == 1)
                    allPlayerList[i].deaths += amount;

                // If the updated player is the local player, update the UI display
                if (i == index)
                {
                    UpdateStatsDisplay();
                }
                break;
            }
        }

        // Check for score-based conditions, like victory
        ScoreCheck();
    }

    // Update the statistics displayed on the UI
    public void UpdateStatsDisplay()
    {
        if (allPlayerList.Count > index)
        {
            UIController.instance.killsText.text = "Kills : " + allPlayerList[index].kills;
            UIController.instance.deathsText.text = "Deaths : " + allPlayerList[index].deaths;
        }
        else
        {
            UIController.instance.killsText.text = "Kills : 0";
            UIController.instance.deathsText.text = "Deaths : 0";
        }
    }

    // Method to send a message indicating the start of the next match
    public void NextMatchSend()
    {
        // Raise a Photon event to signal the start of the next match to all clients
        PhotonNetwork.RaiseEvent(
            (byte)EventCode.NEXT_MATCH,
            null,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
            new SendOptions() { Reliability = true }
        );
    }

    // Method to handle the start of the next match received by clients
    public void NextMatchReceive()
    {
        currentState = GameState.PLAYING;

        // Hide end screen and leaderboard
        UIController.instance.endScreen.SetActive(false);
        UIController.instance.leaderBoard.SetActive(false);

        // Reset player kills and deaths for the next match
        foreach (PlayerInfo player in allPlayerList)
        {
            player.kills = 0;
            player.deaths = 0;
        }

        // Update UI stats display, set up timer, and respawn players
        UpdateStatsDisplay();
        SetUpTimer();
        PlayerSpawner.Instance.SpawnPlayerAtRandomSpawnPoints();
    }

    // Method to show the leaderboard on the UI
    private void ShowLeaderBoard()
    {
        // Activate the leaderboard UI element
        UIController.instance.leaderBoard.SetActive(true);

        // Clear existing leaderboard entries
        foreach (var lp in leaderboardPlayers)
        {
            Destroy(lp.gameObject);
        }
        leaderboardPlayers.Clear();

        // Deactivate the template leaderboard player UI object
        UIController.instance.leaderBoardPlayer.gameObject.SetActive(false);

        // Get a sorted list of players based on kills in descending order
        var sortedList = SortListBasedOnKillsDESC();

        // Instantiate and display new leaderboard player UI objects
        foreach (var player in sortedList)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(UIController.instance.leaderBoardPlayer, UIController.instance.leaderBoardPlayer.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);

            var nickname = player.name;

            // Change color if the player is the local player
            if (PhotonNetwork.NickName == nickname)
            {
                newPlayerDisplay.ChangeColorForLocalPlayer();
            }

            newPlayerDisplay.gameObject.SetActive(true);
            leaderboardPlayers.Add(newPlayerDisplay);
        }
    }

    // Method to sort the player list based on kills in descending order
    private List<PlayerInfo> SortListBasedOnKillsDESC()
    {
        List<PlayerInfo> sortedList = new List<PlayerInfo>(allPlayerList);

        // Sort this list in descending order based on kills
        sortedList.Sort((player1, player2) => player2.kills.CompareTo(player1.kills));
        return sortedList;
    }

    // Method to check if a player has reached the victory condition
    private void ScoreCheck()
    {
        bool winnerFound = false;

        foreach (var player in allPlayerList)
        {
            if (player.kills >= killsToWin && killsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if (PhotonNetwork.IsMasterClient && currentState != GameState.ENDING)
            {
                currentState = GameState.ENDING;
                ListPlayersSend(); // Notify clients of the game ending
            }
        }
    }

    // Called when the local player leaves the room
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0); // Load the main menu scene
    }

    // Method to check the game state and trigger necessary actions
    private void StateCheck()
    {
        if (currentState == GameState.ENDING)
            EndGame(); // Trigger the end of the game
    }

    // Method to handle the end of the game
    private void EndGame()
    {
        currentState = GameState.ENDING;

        UIController.instance.endScreen.SetActive(true); // Show the end screen

        ShowLeaderBoard(); // Display the leaderboard

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Move the camera to the designated point and rotation
        Camera.main.transform.position = mapCamPoint.position;
        Camera.main.transform.rotation = mapCamPoint.rotation;

        StartCoroutine("EndCo"); // Start the coroutine to handle game ending
    }

    // Coroutine to handle end game actions after a delay
    IEnumerator EndCo()
    {
        yield return new WaitForSeconds(waitAfterEnding);

        if (!perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom(); // Leave the current room (match)
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Clear all instantiated Photon objects if the game is perpetual
                PhotonNetwork.DestroyAll();

                if (!Launcher.Instance.changeMapBetweenRounds)
                {
                    NextMatchSend(); // Start the next match immediately
                }
                else
                {
                    int newLevel = UnityEngine.Random.Range(0, Launcher.Instance.allMaps.Length);

                    if (Launcher.Instance.allMaps[newLevel] == SceneManager.GetActiveScene().name)
                    {
                        NextMatchSend(); // Start the next match immediately
                    }
                    else
                    {
                        PhotonNetwork.LoadLevel(Launcher.Instance.allMaps[newLevel]); // Load a new map
                    }
                }
            }
        }
    }

    // Method to set up the match timer
    public void SetUpTimer()
    {
        if (matchLength > 0)
        {
            currentMatchTime = matchLength;
            UpdateTimerDisplay();
        }
    }

    // Method to update the timer display on the UI
    public void UpdateTimerDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);

        UIController.instance.timerText.text = timeToDisplay.Minutes.ToString("00") + ":" + timeToDisplay.Seconds.ToString("00");
    }

    // Method to send the current match timer to all clients
    public void TimerSend()
    {
        object[] package = new object[] { (int)currentMatchTime, currentState };

        PhotonNetwork.RaiseEvent(
                (byte)EventCode.TIMER_SYNC,
                package,
                new RaiseEventOptions() { Receivers = ReceiverGroup.All },
                new SendOptions() { Reliability = true }
                );
    }

    // Method to handle the received match timer update
    public void TimerReceived(object[] data)
    {
        currentMatchTime = (int)data[0];
        currentState = (GameState)data[1];

        UpdateTimerDisplay();
    }
}

