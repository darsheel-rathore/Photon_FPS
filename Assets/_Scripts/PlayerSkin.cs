using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSkin : MonoBehaviourPunCallbacks
{
    public GameObject playerModel;
    public Renderer playerRenderer;
    public Material[] allSkins;

    void Start()
    {
        playerRenderer = playerModel.GetComponent<Renderer>();

        var randomIndex = Random.Range(0, 8);

        playerRenderer.material = allSkins[randomIndex];
    }
}
