using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints;
    public static SpawnManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        foreach (var spawnPoint in spawnPoints)
            spawnPoint.gameObject.SetActive(false);
    }

    public Transform GetRandomSpawnPointPosition
    {
        get
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }
    }
}
