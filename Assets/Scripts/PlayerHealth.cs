using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // Variables
    public int maxHealth = 5;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        
    }

    // Damage checker
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        // Check if player dies
        if (currentHealth <= 0)
        {
            Die();
        }

        Debug.Log("Player took damage! Current health: " + currentHealth);
    }

    // When player dies
    void Die()
    {
        Destroy(gameObject); //Temp code
    }

}
