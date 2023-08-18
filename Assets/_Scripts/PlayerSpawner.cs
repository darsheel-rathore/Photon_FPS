using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;
    public GameObject playerPrefab;
    private GameObject player;
    public GameObject deathVFX;
    public GameObject playerDeadPanel;
    public TMP_Text playerDeadText;

    private void Awake()
    {
        Instance = this;
        playerDeadPanel.SetActive(false);
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayerAtRandomSpawnPoints();
        }
    }

    private void SpawnPlayerAtRandomSpawnPoints()
    {
        // Setting Spawn Point of the instantiated player
        var offset = transform.up * 2f;
        var spawnPoint = SpawnManager.instance.GetRandomSpawnPointPosition;
        var spawnPointWithOffset = offset + spawnPoint.position;

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPointWithOffset, spawnPoint.transform.rotation);

        playerDeadPanel.SetActive(false);
    }

    public void Die(string damager)
    {
        PhotonNetwork.Instantiate(deathVFX.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        playerDeadPanel.SetActive(true);
        playerDeadText.text = $"Shot By : {damager}";

        Invoke("SpawnPlayerAtRandomSpawnPoints", 3f);
    }
}
