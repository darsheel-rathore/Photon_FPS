using UnityEngine;

public class Gun : MonoBehaviour
{
    public bool isAutomatic;
    public float timeBetweenShots = 0.1f;
    public float heatPerShot = 5f;
    public GameObject muzzleFlash;
    public int shotDamage;
    public int adsZoom;
    public AudioSource shotSound;

    //public Vector3 defaultPos;

    private void Start()
    {
        //defaultPos = transform.position;
    }
}
