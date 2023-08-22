using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomButton : MonoBehaviour
{
    public TMP_Text buttonText; // Text component to display room name on the button
    private RoomInfo info; // Reference to the room information

    // Set the details of the button based on the provided room information
    public void SetButtonDetails(RoomInfo info)
    {
        this.info = info; // Store the provided room information
        buttonText.text = this.info.Name; // Display the room name on the button
    }

    // Called when the button is clicked to open/join the room
    public void OpenRoom()
    {
        Launcher.Instance.JoinRoom(info); // Call the JoinRoom method from the Launcher instance and pass the room info
    }
}
