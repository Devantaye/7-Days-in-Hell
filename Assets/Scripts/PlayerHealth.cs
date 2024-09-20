using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // Variables
    public int maxHealth = 3;
    public GameObject[] hearts;
    public GameObject[] emptyHearts;
    public GameObject playerModel;
    public Animator animator;
    private int currentHealth;
    private float deathAnimationDuration = 0.73f;

    void Start()
    {
        currentHealth = maxHealth;
        for (int i = 0; i < currentHealth; i++) {
            emptyHearts[i].SetActive(false); // Hides empty hearts
        }
        
    }

    // Damage checker
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        UpdateHearts(); // UI hearts animation

        // Check if player dies
        if (currentHealth <= 0)
        {
            Die();
        }

        Debug.Log("Player took damage! Current health: " + currentHealth);
    }


    // When the player dies
    void Die()
    {
        // Play death animation
        animator.SetTrigger("Death");

        // Disable player movement
        GetComponent<PlayerControls>().enabled = false;
        GetComponent<EnemyPlayerAI>().enabled = false; // For walking enemy player script

        // Destroy hearts
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(false);
            emptyHearts[i].SetActive(false);
        }

        // Death animation before player dies (object destroyed)
        StartCoroutine(WaitForDeathAnimation());


    }

    // Coroutine to wait for the death animation to complete
    IEnumerator WaitForDeathAnimation()
    {
        // Wait for the animation to finish
        yield return new WaitForSeconds(deathAnimationDuration);

        // Destroy player model after animation finishes
        Destroy(playerModel);
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

    }
}


