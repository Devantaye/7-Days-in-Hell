using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour // Inherit from NetworkBehaviour
{
    // Variables
    public int maxHealth = 3;                          // Max health points
    public GameObject[] hearts;                         // Array of heart GameObjects
    public GameObject[] emptyHearts;                    // Array of empty heart GameObjects
    public GameObject playerModel;                      // Reference to the player model
    public Animator animator;                            // Reference to the animator
    public Transform spawnPoint;                        // Respawn point for the player
    private int currentHealth;                           // Local current health variable
    private bool takeDamageIndicator = true;            // Indicates if the player can take damage
    private bool isInvincible = true;                    // Indicates if the player is invincible
    private Knockback knockback;                         // Reference to knockback script
    private Flash flash;                                 // Reference to flash script
    private float knockBackThrustAmount = 1f;           // Thrust amount for knockback
    private float damageRecoveryTime = 3f;              // Time after taking damage before accepting more
    private float deathAnimationDuration = 0.73f;       // Duration of the death animation
    private float respawnTimer = 3f;                    // Duration of the respawn timer
    private NetworkVariable<int> networkHealth = new NetworkVariable<int>(3); // Network variable for health

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
    }

    void Start()
    {
        // Set health + Display health above characters
        currentHealth = maxHealth;
        UpdateHearts(); // Initialize heart display

        // Start invincibility coroutine
        StartCoroutine(InvincibilityCoroutine());
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        // Collision damage logic removed to prevent immediate damage on spawn
    }

    // Coroutine to handle invincibility after spawn
    private IEnumerator InvincibilityCoroutine()
    {
        yield return new WaitForSeconds(1f); // 1 second of invincibility
        isInvincible = false; // Set to false after the period
    }

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

    // Damage checker
    public void TakeDamage(int damageAmount)
    {
        if (!IsOwner || isInvincible) return; // Only the owner can take damage and not if invincible

        // Reduce health and update
        networkHealth.Value -= damageAmount;
        currentHealth = networkHealth.Value; // Sync local health with network health

        UpdateHearts(); // Update UI hearts animation
        UpdateHealthForClientsServerRpc(); // Notify other clients of the health update

        StartCoroutine(DamageRecoveryRoutine());

        // Check if player dies
        if (networkHealth.Value <= 0)
        {
            Die();
        }
    }

    // Function to sync health with all clients
    [ServerRpc]
    void UpdateHealthForClientsServerRpc() // Updated method name
    {
        UpdateHearts(); // Call on server to update health
        UpdateHealthClientRpc(networkHealth.Value); // Send updated health to all clients
        UpdateHeartsClientRpc(networkHealth.Value); // Sync heart status to clients
    }

    [ClientRpc]
    void UpdateHealthClientRpc(int newHealth)
    {
        networkHealth.Value = newHealth; // Set new health
        currentHealth = newHealth; // Sync local health variable
        UpdateHearts(); // Update health UI
    }

    [ClientRpc]
    void UpdateHeartsClientRpc(int health)
    {
        currentHealth = health; // Update local current health variable
        UpdateHearts(); // Update heart UI
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(damageRecoveryTime);
        takeDamageIndicator = true;
    }

    // When the player dies
    void Die()
    {
        if (!IsOwner) return;
        // Play death animation
        animator.SetTrigger("Death");

        // Notify other clients to play the death animation
        TriggerDeathAnimationClientRpc();

        // Disable player movement
        GetComponent<PlayerControls>().enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Destroy hearts
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(false);
            emptyHearts[i].SetActive(false);
        }

        // Death animation before player dies (object destroyed)
        StartCoroutine(WaitForDeathAnimation());
    }

    // ClientRpc to trigger death animation on all clients
    [ClientRpc]
    private void TriggerDeathAnimationClientRpc()
    {
        if (IsOwner) return; // Prevent the owner from calling this again

        // Trigger the death animation for the player who died
        animator.SetTrigger("Death");
    }

    // Coroutine to wait for the death animation to complete
    IEnumerator WaitForDeathAnimation()
    {
        Debug.Log("Waiting for death animation to complete...");
        yield return new WaitForSeconds(deathAnimationDuration);

        // Hide the player by disabling the renderer
        GetComponent<Renderer>().enabled = false;  // Make the player invisible
        Debug.Log("Player Renderer set to invisible.");

        yield return new WaitForSeconds(respawnTimer); // Wait for respawn timer
        Debug.Log("Waiting period before respawn completed.");

        Respawn();  // Call respawn function

        GetComponent<Renderer>().enabled = true;  // Make the player visible
        Debug.Log("Player Renderer set to visible. Player has respawned.");
    }

    void Respawn()
    {
        // Set player position to respawn point
        transform.position = spawnPoint.position;

        // Reset player health + enable hearts
        networkHealth.Value = maxHealth; // Reset network health
        currentHealth = networkHealth.Value; // Sync local health
        UpdateHearts(); // Update the UI to reflect the player's health

        // Enable player controls
        PlayerControls playerControls = GetComponent<PlayerControls>();
        playerControls.enabled = true; // Re-enable player controls

        // Reset idle animation
        Vector2 lastMovement = Vector2.down; // Reset last movement direction
        animator.SetFloat("LastHorizontal", lastMovement.x);
        animator.SetFloat("LastVertical", lastMovement.y);
    }

    // Update the heart icons based on the current health
    void UpdateHearts()
    {
        // Controls hearts (visible)
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].SetActive(true);  // Show heart if health point exists
            }
            else
            {
                hearts[i].SetActive(false); // Hide heart if health point is lost
            }
        }

        // Controls empty hearts (visible when health is lost)
        for (int i = 0; i < emptyHearts.Length; i++)
        {
            if (i >= currentHealth)
            {
                emptyHearts[i].SetActive(true);  // Show empty heart if health point is lost
            }
            else
            {
                emptyHearts[i].SetActive(false); // Hide empty heart if health point exists
            }
        }
    }

    void DeathAnimation()
    {
        // Handle death animation if needed
    }
}






