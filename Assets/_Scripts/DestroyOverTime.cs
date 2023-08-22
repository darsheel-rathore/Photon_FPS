using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    public float lifeTime = 1.5f; // The time (in seconds) after which the GameObject will be destroyed

    void Start()
    {
        // Destroy the GameObject after the specified lifetime
        Destroy(gameObject, lifeTime);
    }
}
