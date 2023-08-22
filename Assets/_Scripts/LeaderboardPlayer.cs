using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardPlayer : MonoBehaviour
{
    // UI elements to display player details
    public TMP_Text playerNameText, killsText, deathsText;

    // Color to highlight the local player's row
    public Color myColor = new Color(0.53f, 0.81f, 0.28f);

    // Set player details on the leaderboard row
    public void SetDetails(string name, int kills, int deaths)
    {
        playerNameText.text = name;
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
    }

    // Change the color of the leaderboard row for the local player
    public void ChangeColorForLocalPlayer()
    {
        GetComponent<Image>().color = myColor;
    }
}
