using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void Start()
    {
        SpawnPlayerAtRandomSpawnPoints();
    }

    private void SpawnPlayerAtRandomSpawnPoints()
    {
        var offset = transform.up * 2f;
        transform.position = offset + SpawnManager.instance.GetRandomSpawnPointPosition.position;
    }
}
