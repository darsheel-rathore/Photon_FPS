using UnityEngine;

// This script represents a Gun object in the game.
public class Gun : MonoBehaviour
{
    // Determines if the gun is automatic or not.
    public bool isAutomatic;

    // The time interval between consecutive shots.
    public float timeBetweenShots = 0.1f;

    // Amount of heat generated per shot.
    public float heatPerShot = 5f;

    // Reference to the muzzle flash effect GameObject.
    public GameObject muzzleFlash;

    // The damage inflicted by each shot.
    public int shotDamage;

    // The amount of zoom applied when aiming down sights.
    public int adsZoom;

    // Reference to the audio source for gunshot sounds.
    public AudioSource shotSound;
}
