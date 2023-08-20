using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardPlayer : MonoBehaviour
{
    public TMP_Text playerNameText, killsText, deathsText;
    public Color myColor = new Color(0.53f, 0.81f, 0.28f);

    public void SetDetails(string name, int kills, int deaths)
    {
        playerNameText.text = name;
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
    }

    public void ChangeColorForLocalPlayer()
    {
        GetComponent<Image>().color = myColor;
    }
}
