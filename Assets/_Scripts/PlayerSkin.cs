using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSkin : MonoBehaviourPunCallbacks
{
    public GameObject playerModel;   // Reference to the player model game object
    public Renderer playerRenderer;  // Renderer component to change the player's material
    public Material[] allSkins;      // Array of different materials for player skins

    void Start()
    {
        playerRenderer = playerModel.GetComponent<Renderer>(); // Get the Renderer component from the player model

        var randomIndex = Random.Range(0, 8); // Generate a random index within the range of available skins

        playerRenderer.material = allSkins[randomIndex]; // Apply the randomly selected skin material to the player model
    }
}
