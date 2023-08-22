using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints; // Array of spawn points for player spawning
    public static SpawnManager instance; // Singleton instance

    private void Awake()
    {
        instance = this; // Set the singleton instance to this script
    }

    private void Start()
    {
        // Deactivate all spawn points on Start
        foreach (var spawnPoint in spawnPoints)
            spawnPoint.gameObject.SetActive(false);
    }

    // Property that returns a random spawn point's position
    public Transform GetRandomSpawnPointPosition
    {
        get
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)]; // Return a random spawn point
        }
    }
}
