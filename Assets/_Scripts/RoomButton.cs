using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public TMP_Text buttonText;
    private RoomInfo info;

    public void SetButtonDetails(RoomInfo info)
    {
        this.info = info;
        buttonText.text = this.info.Name;
    }

    public void OpenRoom()
    {
        Launcher.Instance.JoinRoom(info);
    }
}
