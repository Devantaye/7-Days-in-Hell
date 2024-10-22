using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CoreHealth : NetworkBehaviour
{
    // Variables for core health
    public int maxHealth = 5; // Core starts with 5 health
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(); // Network synced health

    // Start is called before the first frame update
    void Start()
    {
        currentHealth.Value = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // You can add health bar or effects logic here based on core health
    }

    // Public method to take damage, can be called by any player
    public void TakeDamage(int damageAmount)
    {
        // Subtract health
        currentHealth.Value -= damageAmount;

        // Check if core health reaches 0 or below
        if (currentHealth.Value <= 0)
        {
            DestroyCore();
        }
    }

    // Method to destroy the core when health reaches 0
    private void DestroyCore()
    {
        Debug.Log("Core destroyed!");

        // Destroy the core (this removes the object from the scene)
        Destroy(gameObject);

        // Notify all clients about core destruction (optional, for effects)
        DestroyCoreClientRpc();
    }

    // Notify clients about core destruction
    [ClientRpc]
    private void DestroyCoreClientRpc()
    {
        Debug.Log("Core destroyed (on client side)!");
        // Add any client-side effects, animations, or UI updates here
    }

    // ServerRpc to apply damage from players
    [ServerRpc(RequireOwnership = false)] // Any player can call this, regardless of ownership
    public void PlayerAttackServerRpc(int damageAmount)
    {
        TakeDamage(damageAmount); // Server applies the damage
    }

    // Example of detecting when players collide with or attack the core
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the core collides with something that has the "Player" tag
        if (collision.gameObject.CompareTag("player"))
        {
            Debug.Log("Player has collided with the core.");
            // You can trigger damage here or from another method when the player attacks the core
            // Example: PlayerAttackServerRpc(1);  // Assuming player deals 1 damage
        }
    }
}

