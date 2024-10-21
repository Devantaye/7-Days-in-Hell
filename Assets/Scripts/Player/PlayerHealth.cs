using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour // Inherit from NetworkBehaviour
{
    // Default Variables
    public GameObject[] hearts;                       // Array of heart GameObjects
    public GameObject[] emptyHearts;                  // Array of empty heart GameObjects
    public GameObject playerModel;                    // Reference to the player model
    public Animator animator;                         // Reference to the animator
    public Transform spawnPoint;                      // Respawn point for the player
    public PlayerControls playerControls;             // Reference to player controls

    // Player health stuff
    public static int maxHealth = 3;                  // Max health points
    private int currentHealth;                        // Local current health variable
    private bool takeDamageIndicator = true;          // Indicates if the player can take damage
    private bool isInvincible = false;                // Indicates if the player is invincible
    private Knockback knockback;                      // Reference to knockback script
    private Flash flash;                              // Reference to flash script
    private float knockBackThrustAmount = 1f;         // Thrust amount for knockback
    private float damageRecoveryTime = 1f;            // Time after taking damage before accepting more
    private float deathAnimationDuration = 0.73f;     // Duration of the death animation
    private float respawnTimer = 3f;                  // Duration of the respawn timer
    public float invincibilityTimer = 2f;             // Invincibility duration (for respawn)
    private bool heartsFaded = false;

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
        playerControls = GetComponent<PlayerControls>();
    }

    void Start()
    {
        // Set health + Display health above characters
        currentHealth = maxHealth;
        UpdateHearts(); 
    }

    void Update()
    {
        if (!IsOwner) {
            return;
        }

        UpdateHearts();

        // Check invisibility status and sync heart visibility across all clients
        if (playerControls.isInvis)
        {
            if(heartsFaded == false)
            {
                StartCoroutine(FadeHearts(0.5f)); // Fade hearts to 50% for the local player
                SyncHeartsWithServerRpc(0.5f); // Sync this with the server
                heartsFaded = true;
            }
        }
        else
        {
            if(heartsFaded == true)
            {
                StartCoroutine(FadeHearts(1f)); // Fade hearts back to full opacity when player is visible
                SyncHeartsWithServerRpc(1f); // Sync this with the server
                heartsFaded = false;
            }
        }
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

    private IEnumerator FadeHearts(float targetAlpha)
    {
        float fadeDuration = 1f;
        float startAlpha = hearts[0].GetComponent<SpriteRenderer>().color.a;
        float endAlpha;

        if (IsOwner)
        {
            // If this is the local player, fade to the targetAlpha (50% or 100%)
            endAlpha = targetAlpha;
        }
        else
        {
            // For other players, only fade to 0% when the local player is invisible, and 100% otherwise
            endAlpha = targetAlpha == 0.5f ? 0f : 1f;  // Set to 0 if local player is invisible, 1 if visible
        }

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);

            // Apply alpha to active hearts
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i].activeSelf)
                {
                    SetHeartAlpha(hearts[i], newAlpha);
                }
            }

            for (int i = 0; i < emptyHearts.Length; i++)
            {
                if (emptyHearts[i].activeSelf)
                {
                    SetHeartAlpha(emptyHearts[i], newAlpha);
                }
            }

            yield return null;
        }

        // Ensure the final alpha is applied to all hearts
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i].activeSelf)
            {
                SetHeartAlpha(hearts[i], endAlpha);
            }
        }

        for (int i = 0; i < emptyHearts.Length; i++)
        {
            if (emptyHearts[i].activeSelf)
            {
                SetHeartAlpha(emptyHearts[i], endAlpha);
            }
        }
    }


    private void SetHeartAlpha(GameObject heart, float alpha)
    {
        SpriteRenderer sr = heart.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
        }
    }

    // Damage dealer - Player takes damage + checks death status
    public void TakeDamage(int damageAmount)
    {
        if (playerControls.isInvis)
        {
            Debug.Log("Player is invisible and cannot take damage.");
            return; // Player can't take damage while invisible
        }

        if (isInvincible || takeDamageIndicator == false)
        {
  
            return; //  Player cant take damage due to respawning,  damage invincibility or invis
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
        // Trigger animator functions
        this.animator.SetTrigger("Death");
        TriggerDeathAnimationClientRpc();

        // Disable necessary scripts (to prevent movement etc)
        GetComponent<PlayerControls>().enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Disable hearts ui
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(false);
            emptyHearts[i].SetActive(false);
        }

        // Hide all hearts (Server sided) + start death animation
        HideAllHeartsClientRpc();
        StartCoroutine(WaitForDeathAnimation());
    }

    // Wait for death animation to complete + additional functions
    IEnumerator WaitForDeathAnimation()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        // Hide the player (simulate death)
        GetComponent<Renderer>().enabled = false;  // Make the player invisible
        HidePlayerModelClientRpc();

        yield return new WaitForSeconds(respawnTimer); // Wait for respawn timer

        // Respawn + show player when alive
        Respawn();  
        GetComponent<Renderer>().enabled = true;  
    }

    // Respawn method - 
    void Respawn()
    {
        // Reset variables + enable components
        transform.position = spawnPoint.position;
        networkHealth.Value = maxHealth;
        currentHealth = networkHealth.Value;
        UpdateHearts();
        GetComponent<PlayerControls>().enabled = true;

        // Update hearts UI + set default parameters again
        UpdateHeartsClientRpc(networkHealth.Value);
        Vector2 lastMovement = Vector2.down;
        animator.SetFloat("LastHorizontal", lastMovement.x);
        animator.SetFloat("LastVertical", lastMovement.y);

        // Call ClientRpc to show player model + activate invicibility
        ShowPlayerModelClientRpc();
        StartCoroutine(InvincibilityCoroutine());
    }

    // Invincibility function (cant die during respawn)
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTimer); 
        isInvincible = false; 
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

    // ClientRpc to trigger death animation
    [ClientRpc]
    private void TriggerDeathAnimationClientRpc()
    {
        animator.SetTrigger("Death");
    }

    // ClientRpc to hide player model
    [ClientRpc]
    void HidePlayerModelClientRpc()
    {
        Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false; 
        }
    }


    // ClientRpc to show player model
    [ClientRpc]
    void ShowPlayerModelClientRpc()
    {
        Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = true; 
        }
    }

    // Sync heart visibility with the server
    [ServerRpc(RequireOwnership = false)]
    private void SyncHeartsWithServerRpc(float targetAlpha)
    {
        // Notify all clients to update the hearts' opacity
        SyncHeartsClientRpc(targetAlpha);
    }

    // Update heart visibility across all clients
    [ClientRpc]
    private void SyncHeartsClientRpc(float targetAlpha)
    {
        StartCoroutine(FadeHearts(targetAlpha));
    }

}







