using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance; // Singleton instance
    public GameObject playerPrefab; // Prefab for the player character
    private GameObject player; // Reference to the spawned player object
    public GameObject deathVFX; // Death visual effects
    public GameObject playerDeadPanel; // UI panel for displaying death information
    public TMP_Text playerDeadText; // Text component to display information about player's death

    private void Awake()
    {
        Instance = this; // Set the singleton instance to this script
        playerDeadPanel.SetActive(false); // Deactivate the player death panel on awake
    }

    void Start()
    {
        // Spawn the player if connected to Photon Network
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Called From the start function. - Player Instantiate");
            SpawnPlayerAtRandomSpawnPoints();
        }
    }

    public void SpawnPlayerAtRandomSpawnPoints()
    {
        // Calculate spawn position with an offset
        var offset = transform.up * 2f;
        var spawnPoint = SpawnManager.instance.GetRandomSpawnPointPosition;
        var spawnPointWithOffset = offset + spawnPoint.position;

        // Instantiate the player at the calculated spawn position
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPointWithOffset, spawnPoint.transform.rotation);
        Debug.Log("Called - Player Instantiate");

        playerDeadPanel.SetActive(false); // Deactivate the player death panel
    }

    public void Die(string damager)
    {
        // Instantiate death visual effects
        PhotonNetwork.Instantiate(deathVFX.name, player.transform.position, Quaternion.identity);

        // Destroy the player object
        PhotonNetwork.Destroy(player);
        player = null;

        // Activate the player death panel
        playerDeadPanel.SetActive(true);

        // Display information about the player's death
        playerDeadText.text = $"Shot By : {damager}";

        // Respawn the player if the match is ongoing and the player object is null
        if (MatchManager.Instance.currentState == MatchManager.GameState.PLAYING && player == null)
            Invoke("SpawnPlayerAtRandomSpawnPoints", 3f); // Respawn after 3 seconds
    }
}
