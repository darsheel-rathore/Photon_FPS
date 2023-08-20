using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    public float lifeTime = 1.5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
