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

    void Update()
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
            if (Input.GetButton("Fire1"))
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

    // Method for firing the weapon
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
    }
}