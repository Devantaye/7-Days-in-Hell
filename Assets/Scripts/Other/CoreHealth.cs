using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoreHealthNotreal : NetworkBehaviour // Inherit from NetworkBehaviour
{
    // Default Variables
    public GameObject[] hearts;                       // Array of heart GameObjects
    public GameObject[] emptyHearts;                  // Array of empty heart GameObjects
    public Animator animator;                         // Reference to the animator

    // Player health stuff
    public static int maxHealth = 3;                  // Max health points
    private int currentHealth;                        // Local current health variable
    private bool takeDamageIndicator = true;          // Indicates if the core can take damage
    private bool isInvincible = false;                // Indicates if the player is invincible
    private Knockback knockback;                      // Reference to knockback script
    private Flash flash;                              // Reference to flash script
    private float knockBackThrustAmount = 1f;         // Thrust amount for knockback
    private float damageRecoveryTime = 1f;            // Time after taking damage before accepting more


    // Network variables
    private NetworkVariable<int> networkHealth = new NetworkVariable<int>(maxHealth); // Network variable for health

    /*   ==========================================
         LIFECYCLE METHODS - Start() , Update() etc
         ==========================================   */

    private void Awake()
    {
        // Gets references to components
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
    }

    void Start()
    {
        // Set health + Display health above characters
        currentHealth = maxHealth;
        UpdateHearts(); 
    }

    /*   =========================================
         HEALTH METHODS - Taking damage, death etc
         =========================================   */

    // Update the heart icons based on the current health
    void UpdateHearts()
    {
        // Controls hearts (visible)
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].SetActive(true);  
            }
            else
            {
                hearts[i].SetActive(false); 
            }
        }

        // Controls empty hearts (visible when health is lost)
        for (int i = 0; i < emptyHearts.Length; i++)
        {
            if (i >= currentHealth)
            {
                emptyHearts[i].SetActive(true);  
            }
            else
            {
                emptyHearts[i].SetActive(false); 
            }
        }
    }

    // Damage dealer - Player takes damage + checks death status
    public void TakeDamage(int damageAmount)
    {
        if (isInvincible || takeDamageIndicator == false)
        {
            return; //  Player cant take damage due to respawning or damage invincibility
        }

        // Reduce health and update variables
        takeDamageIndicator = false; // Cant take damage
        networkHealth.Value -= damageAmount;
        currentHealth = networkHealth.Value; // Sync local health with network health

        // Update server stuff
        UpdateHearts(); // Update UI hearts
        UpdateHealthForClientsServerRpc(); // Notify other clients of the health update
        StartCoroutine(DamageRecoveryRoutine()); // Prevent more damage

        // Check if player dies
        if (networkHealth.Value <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(damageRecoveryTime);
        takeDamageIndicator = true;
    }

    // Death function - Kills player + respawn functions
    void Die()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(false);
            emptyHearts[i].SetActive(false);
        }

        // Hide all hearts (Server sided) + start death animation
        HideAllHeartsClientRpc();
    }


    /*   ===============================================
         NETCODE METHODS - For animation/action syncing
         ===============================================   */

    // ServerRpc to trigger attacks (monsters)
    [ServerRpc]
    void AttackServerRpc(ulong enemyNetworkObjectId)
    {
        // Get the PlayerHealth component using the NetworkObjectId
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetworkObjectId, out var enemyNetworkObject))
        {
            var enemyHealth = enemyNetworkObject.GetComponent<PlayerHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1); // Call TakeDamage on the enemy
                knockback.GetKnockedBack(enemyHealth.transform, knockBackThrustAmount);
                StartCoroutine(flash.FlashRoutine());
            }
        }
    }

    // ServerRpc to sync health with all clients
    [ServerRpc(RequireOwnership = false)]
    void UpdateHealthForClientsServerRpc() 
    {
        UpdateHearts(); 
        UpdateHealthClientRpc(networkHealth.Value); 
        UpdateHeartsClientRpc(networkHealth.Value); 
    }

    // ClientRpc to update health
    [ClientRpc]
    void UpdateHealthClientRpc(int newHealth)
    {
        networkHealth.Value = newHealth; 
        currentHealth = newHealth; 
        UpdateHearts(); 
    }

    // ClientRpc to update hearts
    [ClientRpc]
    void UpdateHeartsClientRpc(int health)
    {
        currentHealth = health; 
        UpdateHearts(); 
    }

    // ClientRpc to Hide hearts 
    [ClientRpc]
    void HideAllHeartsClientRpc()
    {
        // Hide all full hearts
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(false);
        }

        // Hide all empty hearts
        for (int i = 0; i < emptyHearts.Length; i++)
        {
            emptyHearts[i].SetActive(false);
        }
    }

}







