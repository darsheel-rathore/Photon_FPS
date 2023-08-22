using Photon.Pun;

public class PlayerDamageController : MonoBehaviourPunCallbacks
{
    // Singleton instance
    public static PlayerDamageController instance;

    // Maximum health and current health of the player
    public int maxHealth = 100;
    public int currentHealth;

    // Initialize the singleton instance
    private void Awake()
    {
        instance = this;
    }

    // Initialize health when the game starts for the local player
    private void Start()
    {
        if (photonView.IsMine)
        {
            currentHealth = maxHealth;
            UIController.instance.UpdateHealthSlider(currentHealth);
        }
    }

    // RPC method to deal damage to the player
    [PunRPC]
    public void DealDamage(string damager, int actorNumber, int damageAmount)
    {
        TakeDamage(damager, actorNumber, damageAmount);
    }

    // Function to handle taking damage
    private void TakeDamage(string damager, int actorNumber, int damageAmount)
    {
        if (photonView.IsMine)
        {
            ReduceHealth(damageAmount);

            // Check if player's health has reached zero
            if (currentHealth <= 0)
            {
                // Call Die method of PlayerSpawner instance
                PlayerSpawner.Instance.Die(damager);

                int kills = 0;
                int death = 1;
                int amountToAdd = 1;

                // Update kill count for the attacker
                MatchManager.Instance.UpdateStatsSend(actorNumber, kills, amountToAdd);

                // Update death count for the player who was killed
                MatchManager.Instance.UpdateStatsSend(photonView.Owner.ActorNumber, death, amountToAdd);
            }
        }
    }

    // Function to reduce player's health
    public void ReduceHealth(int value)
    {
        if (currentHealth > value)
            currentHealth -= value;
        else
            currentHealth = 0;

        UIController.instance.UpdateHealthSlider(currentHealth);
    }
}
