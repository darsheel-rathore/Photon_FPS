using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public static PlayerController instance;

    private void Awake()
    {
        instance = this;
    }

    [PunRPC]
    public void DealDamage(string damager)
    {
        TakeDamage(damager);
    }

    private void TakeDamage(string damager)
    {
        Debug.Log($"{photonView.Owner.NickName} is hit by {damager}");

        gameObject.SetActive(false);
    }
}
