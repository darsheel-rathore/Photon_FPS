using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using System;

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

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager Instance;
    public List<PlayerInfo> allPlayerList;
    private List<LeaderboardPlayer> leaderboardPlayers;
    private int index;

    // Game State
    public enum GameState
    {
        WAITING,
        PLAYING,
        ENDING
    }
    public int killsToWin = 3;
    public Transform mapCamPoint;
    public GameState currentState = GameState.WAITING;
    public float waitAfterEnding = 5f;

    // Event Related
    public enum EventCode : byte
    {
        NEW_PLAYER,
        LIST_PLAYER,
        CHANGE_STATE,
        NEXT_MATCH,
        TIMER_SYNC
    }
    public EventCode eventCode;
    public bool perpetual = true;

    private float matchLength = 75f;
    public float currentMatchTime;
    private float sendTimer;

    #region UNITY_BUILTIN
    private void Awake()
    {
        Instance = this;

        allPlayerList = new List<PlayerInfo>();
        leaderboardPlayers = new List<LeaderboardPlayer>();
    }
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
            currentState = GameState.PLAYING;
        }

        SetUpTimer();
    }
    private void Update()
    {
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

        if (!PhotonNetwork.IsMasterClient)
            return;

        if(currentMatchTime > 0 && currentState == GameState.PLAYING) {
            currentMatchTime -= Time.deltaTime;
            if (currentMatchTime < 0)
            {
                currentMatchTime = 0;
                currentState = GameState.ENDING;
                ListPlayersSend();
                StateCheck();
            }

            UpdateTimerDisplay();

            sendTimer -= Time.deltaTime;

            if(sendTimer <= 0)
            {
                sendTimer += 1f;

                TimerSend();
            }
        }

    }
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    #endregion

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCode theEvent = (EventCode)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

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

    // Method is being called from the start, when this game object came into existance
    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];

        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCode.NEW_PLAYER,
            package,
            new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient },
            new SendOptions() { Reliability = true }
            );
    }

    // This method only runs on master client 
    public void NewPlayerReceive(object[] data)
    {
        PlayerInfo newPlayerInfo = new PlayerInfo(
                                            name: (string)data[0],
                                            actor: (int)data[1],
                                            kills: (int)data[2],
                                            deaths: (int)data[3]
                                        );
        allPlayerList.Add(newPlayerInfo);

        ListPlayersSend();
    }
    public void ListPlayersSend()
    {
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


        PhotonNetwork.RaiseEvent(
            (byte)EventCode.LIST_PLAYER,
            package,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
            new SendOptions() { Reliability = true }
            );
    }
    public void ListPlayersReceive(object[] data)
    {
        allPlayerList.Clear();

        currentState = (GameState)data[0];

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

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i - 1;
            }
        }

        StateCheck();
    }
    public void UpdateStatsSend(int actorNumber, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorNumber, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
            (byte)EventCode.CHANGE_STATE,
            package,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
            new SendOptions() { Reliability = true }
            );
    }
    public void UpdateStatsReceive(object[] data)
    {
        int actor = (int)data[0];
        int statType = (int)data[1];
        int amount = (int)data[2];

        for (int i = 0; i < allPlayerList.Count; i++)
        {
            if (allPlayerList[i].actor == actor)
            {
                // For Kills
                if (statType == 0)
                    allPlayerList[i].kills += amount;
                // For deaths
                else if (statType == 1)
                    allPlayerList[i].deaths += amount;

                if (i == index)
                {
                    UpdateStatsDisplay();
                }
                break;
            }
        }

        ScoreCheck();
    }
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
    public void NextMatchSend()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCode.NEXT_MATCH,
            null,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
            new SendOptions() { Reliability = true }
            );
    }
    public void NextMatchReceive()
    {
        currentState = GameState.PLAYING;

        UIController.instance.endScreen.SetActive(false);
        UIController.instance.leaderBoard.SetActive(false);

        foreach(PlayerInfo player in allPlayerList)
        {
            player.kills = 0; 
            player.deaths = 0;
        }

        UpdateStatsDisplay();
        SetUpTimer();
        PlayerSpawner.Instance.SpawnPlayerAtRandomSpawnPoints();
    }
    private void ShowLeaderBoard()
    {
        UIController.instance.leaderBoard.SetActive(true);

        foreach (var lp in leaderboardPlayers)
        {
            Destroy(lp.gameObject);
        }

        leaderboardPlayers.Clear();

        UIController.instance.leaderBoardPlayer.gameObject.SetActive(false);

        var sortedList = SortListBasedOnKillsDESC();

        foreach (var player in sortedList)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(UIController.instance.leaderBoardPlayer, UIController.instance.leaderBoardPlayer.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);
            
            var nickname = player.name;

            if (PhotonNetwork.NickName == nickname)
            {
                newPlayerDisplay.ChangeColorForLocalPlayer();
            }

            newPlayerDisplay.gameObject.SetActive(true);

            leaderboardPlayers.Add(newPlayerDisplay);
        }
    }
    private List<PlayerInfo> SortListBasedOnKillsDESC()
    {
        List<PlayerInfo> sortedList = new List<PlayerInfo>(allPlayerList);

        // Sort this list in desc order based on kills
        sortedList.Sort((player1, player2) => player2.kills.CompareTo(player1.kills));
        return sortedList;
    }
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

        if(winnerFound)
        {
            if(PhotonNetwork.IsMasterClient && currentState != GameState.ENDING)
            {
                currentState = GameState.ENDING;
                ListPlayersSend();
            }
        }
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }
    private void StateCheck()
    {
        if (currentState == GameState.ENDING)
            EndGame();
    }
    private void EndGame()
    {
        currentState = GameState.ENDING;

        UIController.instance.endScreen.SetActive(true);

        ShowLeaderBoard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = mapCamPoint.position;
        Camera.main.transform.rotation = mapCamPoint.rotation;

        StartCoroutine("EndCo");
    }
    IEnumerator EndCo()
    {
        yield return new WaitForSeconds(waitAfterEnding);

        if (!perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.DestroyAll();
                }

                if (!Launcher.Instance.changeMapBetweenRounds)
                {
                    NextMatchSend();
                }
                else
                {
                    int newLevel = UnityEngine.Random.Range(0, Launcher.Instance.allMaps.Length);

                    if (Launcher.Instance.allMaps[newLevel] == SceneManager.GetActiveScene().name)
                    {
                        NextMatchSend();
                    }
                    else
                    {
                        PhotonNetwork.LoadLevel(Launcher.Instance.allMaps[newLevel]);
                    }
                }
            }
        }
    }
    public void SetUpTimer()
    {
        if(matchLength > 0)
        {
            currentMatchTime = matchLength;
            UpdateTimerDisplay();
        }
    }
    public void UpdateTimerDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);

        UIController.instance.timerText.text = timeToDisplay.Minutes.ToString("00") + ":" + timeToDisplay.Seconds.ToString("00");
    }
    public void TimerSend()
    {
        object[] package = new object[] { (int) currentMatchTime, currentState };

        PhotonNetwork.RaiseEvent(
                (byte)EventCode.TIMER_SYNC,
                package,
                new RaiseEventOptions() { Receivers = ReceiverGroup.All },
                new SendOptions() { Reliability = true }
                );
    }
    public void TimerReceived(object[] data)
    {
        currentMatchTime = (int) data[0];
        currentState = (GameState) data[1];

        UpdateTimerDisplay();
    }
}
