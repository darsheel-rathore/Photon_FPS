using Photon.Pun;

public class PlayerDamageController : MonoBehaviourPunCallbacks
{
    public static PlayerDamageController instance;
    public int maxHealth = 100;
    public int currentHealth;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            currentHealth = maxHealth;
            UIController.instance.UpdateHealthSlider(currentHealth);
        }
    }

    [PunRPC]
    public void DealDamage(string damager, int actorNumber, int damageAmount)
    {
        TakeDamage(damager, actorNumber, damageAmount);
    }

    private void TakeDamage(string damager, int actorNumber, int damageAmount)
    {
        if (photonView.IsMine)
        {
            ReduceHealth(damageAmount);

            if (currentHealth <= 0)
            {
                PlayerSpawner.Instance.Die(damager);

                int kills = 0;
                int death = 1;
                int amountToAdd = 1;
                
                MatchManager.Instance.UpdateStatsSend(actorNumber, kills, amountToAdd);

                MatchManager.Instance.UpdateStatsSend(photonView.Owner.ActorNumber, death, amountToAdd);


            }
        }
    }

    public void ReduceHealth(int value)
    {
        if (currentHealth > value)
            currentHealth -= value;
        else
            currentHealth = 0;

        UIController.instance.UpdateHealthSlider(currentHealth);
    }
}
