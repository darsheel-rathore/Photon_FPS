using Photon.Realtime;
using TMPro;
using UnityEngine;

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
