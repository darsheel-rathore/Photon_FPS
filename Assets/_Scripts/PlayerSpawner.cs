using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;
    public GameObject playerPrefab;
    private GameObject player;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
            SpawnPlayerAtRandomSpawnPoints();
        }
    }

    public void SpawnPlayer()
    {

    }

    private void SpawnPlayerAtRandomSpawnPoints()
    {
        // Setting Spawn Point of the instantiated player
        var offset = transform.up * 2f;
        var spawnPoint = SpawnManager.instance.GetRandomSpawnPointPosition;
        var spawnPointWithOffset = offset + spawnPoint.position;

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPointWithOffset, spawnPoint.transform.rotation);
    }
}
