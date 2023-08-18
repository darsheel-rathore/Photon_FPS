using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerDamageController : MonoBehaviourPunCallbacks
{
    public static PlayerDamageController instance;
    public int maxHealth = 100;
    public int currentHealth;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            currentHealth = maxHealth;
            UIController.instance.UpdateHealthSlider(currentHealth);
        }
    }

    [PunRPC]
    public void DealDamage(string damager, int damageAmount)
    {
        TakeDamage(damager, damageAmount);
    }

    private void TakeDamage(string damager, int damageAmount)
    {
        if (photonView.IsMine)
        {
            ReduceHealth(damageAmount);

            if(currentHealth <= 0)
                PlayerSpawner.Instance.Die(damager);
        }   
    }

    public void ReduceHealth(int value)
    {
        if (currentHealth > value)
            currentHealth -= value;
        else
            currentHealth = 0;

        UIController.instance.UpdateHealthSlider(currentHealth);
    }
}
