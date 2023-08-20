using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    public List<PlayerInfo> allPlayerList = new List<PlayerInfo>();
    private int index;
    public enum EventCode : byte
    {
        NEW_PLAYER,
        LIST_PLAYER,
        CHANGE_STATE
    }

    public EventCode eventCode;

    #region UNITY_BUILTIN
    private void Awake()
    {
        Instance = this;
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

    // This methods only runs on master client 
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
        object[] package = new object[allPlayerList.Count];

        for (int i = 0; i < allPlayerList.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayerList[i].name;
            piece[1] = allPlayerList[i].actor;
            piece[2] = allPlayerList[i].kills;
            piece[3] = allPlayerList[i].deaths;

            package[i] = piece;
        }


        PhotonNetwork.RaiseEvent(
            (byte)EventCode.LIST_PLAYER,
            package,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
            new SendOptions() { Reliability = true}
            );
    }
    public void ListPlayersReceive(object[] data) 
    {
        allPlayerList.Clear();

        for(int i = 0; i < data.Length; i++)
        {
            object[] piece = (object[]) data[i];

            PlayerInfo player = new PlayerInfo(
                name: (string)piece[0],
                actor: (int)piece[1],
                kills: (int)piece[2],
                deaths: (int)piece[3]
                );

            allPlayerList.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i;
            }
        }
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

        for(int i = 0; i < allPlayerList.Count; i++)
        {
            if (allPlayerList[i].actor == actor)
            {
                // For Kills
                if (statType == 0)
                    allPlayerList[i].kills += amount;
                // For deaths
                else if (statType == 1)
                    allPlayerList[i].deaths += amount;

                if(i == index)
                {
                    UpdateStatsDisplay();
                }
                break;
            }
        }
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
}
