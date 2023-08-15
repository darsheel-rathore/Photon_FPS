using System;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] GameObject bulletImpact;

    public float timeBetweenShots = 0.1f;
    private float shotCounter;

    [Header("Overheat Mechanism")]
    public float maxHeat = 10f;
    public float heatPerShot = 1f;
    public float coolRate = 4f;
    public float overHeatCoolRate = 5f;
    public float heatCounter;
    private bool isOverHeated;

    public Gun[] guns;
    private int selectedGun;

    private bool isMuzzleFlashIsActive;
    // Muzzle
    public float muzzleDisplayTime;
    private float muzzleCounter;

    private void Start()
    {
        selectedGun = 0;

        SwitchWeapon(selectedGun);
    }

    void Update()
    {
        CheckMuzzleFlash();

        UpdateWeaponHeat();

        SwitchWeaponWithScrollAndNumKeys();
    }

    private void CheckMuzzleFlash()
    {
        if (guns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;

            if (muzzleCounter < 0)
                guns[selectedGun].muzzleFlash.SetActive(false);
        }
    }

    private void SwitchWeaponWithScrollAndNumKeys()
    {
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun--;
            if (selectedGun < 0)
                selectedGun = guns.Length - 1;

            SwitchWeapon(selectedGun);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun++;
            if (selectedGun >= guns.Length)
                selectedGun = 0;

            SwitchWeapon(selectedGun);
        }

        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //    SwitchWeapon(0);
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //    SwitchWeapon(1);
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //    SwitchWeapon(2);
    }

    private void UpdateWeaponHeat()
    {

        // Check if the weapon is not overheated
        if (!isOverHeated)
        {
            // Check for primary fire input (Mouse0) to initiate shooting
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Fire();                
            }

            // Check if the primary fire button (Fire1) is held down
            if (Input.GetButton("Fire1") && guns[selectedGun].isAutomatic)
            {
                // Decrement the shot counter based on the time
                shotCounter -= Time.deltaTime;

                // Fire if the shot counter is below 0
                if (shotCounter < 0)
                    Fire();
            }

            // Cool down the weapon based on the cool rate
            heatCounter -= coolRate * Time.deltaTime;
        }
        else  // If the weapon is overheated
        {
            // Cool down the weapon faster when overheated
            heatCounter -= overHeatCoolRate * Time.deltaTime;

            UIController.instance.ToggleOverHeatMsg(true);
        }

        // Ensure the heat counter doesn't go below 0
        if (heatCounter < 0)
        {
            heatCounter = 0;
            isOverHeated = false;
            UIController.instance.ToggleOverHeatMsg(false);
        }

        // Update the overheat slider
        UIController.instance.OverHeatSliderUpdate(heatCounter);
    }

    private void Fire()
    {
        // Create a ray from the center of the screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Perform a raycast and check if it hits something
        bool isHit = Physics.Raycast(ray: ray, out RaycastHit hitInfo);

        // Check if the ray hits something
        if (isHit)
        {
            // Instantiate the bullet impact effect at the hit point with a slight offset
            GameObject bulletImpact = Instantiate(this.bulletImpact, hitInfo.point + (hitInfo.normal * 0.002f), Quaternion.LookRotation(hitInfo.normal, Vector3.up));

            // Destroy the bullet impact effect after a delay
            Destroy(bulletImpact, 3f);
        }

        // Reset the shot counter to control time between shots
        shotCounter = timeBetweenShots;

        // Increase weapon heat
        heatCounter += heatPerShot;

        // Check if the weapon is overheated
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            isOverHeated = true;
        }

        // Display muzzle flash
        guns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    private void SwitchWeapon(int index)
    {
        foreach (var gun in guns)
        {
            gun.gameObject.SetActive(false);
            gun.muzzleFlash.SetActive(false);
        }

        guns[index].gameObject.SetActive(true);

        // Change the values
        timeBetweenShots = guns[index].GetComponent<Gun>().timeBetweenShots;
        heatPerShot = guns[index].GetComponent<Gun>().heatPerShot;


    }
}