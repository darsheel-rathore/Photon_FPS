using Photon.Pun;
using UnityEngine;

public class PlayerShooting : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject bulletImpact;
    [SerializeField] GameObject playerHitImpact;

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

    // Gun Holder
    public Transform modelGunPoint;
    public Transform gunHolder;

    private void Start()
    {
        selectedGun = 0;
        //SwitchWeapon(selectedGun);  // Initialize the selected weapon
        photonView.RPC("SwitchWeaponAcrossNetwork", RpcTarget.All, selectedGun);

        if (photonView.IsMine)
        {

        }
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        CheckMuzzleFlash();  // Check if muzzle flash should be deactivated
        UpdateWeaponHeat();  // Manage weapon heat and shooting
        SwitchWeaponWithScrollAndNumKeys();  // Handle weapon switching
    }

    // Check if muzzle flash should be deactivated
    private void CheckMuzzleFlash()
    {
        if (guns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;
            if (muzzleCounter < 0)
                guns[selectedGun].muzzleFlash.SetActive(false);
        }
    }

    // Handle weapon switching using scroll wheel and number keys
    private void SwitchWeaponWithScrollAndNumKeys()
    {
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun--;
            if (selectedGun < 0)
                selectedGun = guns.Length - 1;
            //SwitchWeapon(selectedGun);
            photonView.RPC("SwitchWeaponAcrossNetwork", RpcTarget.All, selectedGun);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun++;
            if (selectedGun >= guns.Length)
                selectedGun = 0;
            //SwitchWeapon(selectedGun);+
            //SwitchWeaponAcrossNetwork(selectedGun);
            photonView.RPC("SwitchWeaponAcrossNetwork", RpcTarget.All, selectedGun);

        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedGun = 0;
            SwitchWeapon(selectedGun);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedGun = 1;
            SwitchWeapon(selectedGun);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedGun = 2;
            SwitchWeapon(selectedGun);
        }
    }

    // Update weapon heat and manage shooting
    private void UpdateWeaponHeat()
    {
        if (!isOverHeated)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Fire();  // Fire the weapon on Mouse0 click
            }
            if (Input.GetButton("Fire1") && guns[selectedGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter < 0)
                    Fire();
            }
            heatCounter -= coolRate * Time.deltaTime;  // Cool down the weapon
        }
        else
        {
            heatCounter -= overHeatCoolRate * Time.deltaTime;
            UIController.instance.ToggleOverHeatMsg(true);  // Display overheated message
        }

        // Ensure heat counter stays within limits
        if (heatCounter < 0)
        {
            heatCounter = 0;
            isOverHeated = false;
            UIController.instance.ToggleOverHeatMsg(false);  // Hide overheated message
        }

        // Update UI slider for overheating
        UIController.instance.OverHeatSliderUpdate(heatCounter);
    }

    // Handle firing the weapon
    private void Fire()
    {
        // Create a ray from the center of the screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        bool isHit = Physics.Raycast(ray: ray, out RaycastHit hitInfo);

        if (isHit)
        {
            if (hitInfo.collider.gameObject.tag == "Player")
            {
                GameObject playerBloodImpact = PhotonNetwork.Instantiate(
                    playerHitImpact.name,
                    hitInfo.point,
                    Quaternion.identity);
                Destroy(playerBloodImpact, 3f);

                hitInfo.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, photonView.Owner.ActorNumber, guns[selectedGun].shotDamage);
            }
            else
            {
                // Instantiate bullet impact effect at the hit point
                GameObject bulletImpact = Instantiate(
                    this.bulletImpact,
                    hitInfo.point + (hitInfo.normal * 0.002f),
                    Quaternion.LookRotation(hitInfo.normal,
                    Vector3.up));
                Destroy(bulletImpact, 3f);  // Destroy bullet impact effect after a delay
            }
        }

        shotCounter = timeBetweenShots;  // Reset shot counter for time between shots
        heatCounter += heatPerShot;  // Increase weapon heat

        // Check if weapon is overheated
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            isOverHeated = true;
        }

        // Display muzzle flash
        guns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    // Switch to the specified weapon index
    private void SwitchWeapon(int index)
    {
        foreach (var gun in guns)
        {
            gun.gameObject.SetActive(false);  // Deactivate all guns
            gun.muzzleFlash.SetActive(false);  // Deactivate muzzle flash
        }

        guns[index].gameObject.SetActive(true);  // Activate the selected gun

        // Update timing and heat values based on selected weapon
        timeBetweenShots = guns[index].GetComponent<Gun>().timeBetweenShots;
        heatPerShot = guns[index].GetComponent<Gun>().heatPerShot;
    }

    [PunRPC]
    public void SwitchWeaponAcrossNetwork(int index)
    {
        if (index < guns.Length)
        {
            selectedGun = index;
            SwitchWeapon(selectedGun);
        }
    }
}