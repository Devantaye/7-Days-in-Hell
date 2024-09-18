using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
    //Enemies full health
    [SerializeField] private int fullHealth = 2;

    // Variable for updated health 
    private int updatedHealth;

    private Knockback knockback;

    private void Awake()
    {
        knockback = GetComponent<Knockback>();
    }

    //Loot to drop after death
    [SerializeField] private GameObject LootPrefab;

    //Called when game is started
    private void Start()
    {
        updatedHealth = fullHealth; //Set the health to full when the game is started
    }

    //Used to remove health and set it to the updatedhealth when monster takes damage
    public void TakeDamage(int damage)
    {
        updatedHealth -= damage; //Decrease the monster health by the damage that was taken
        knockback.GetKnockedBack(PlayerControls.Instance.transform, 15f);
        DetectDeath(); //Check if enemy should be 'deleted' after death
    }

    //Checks to see if enemies health is less than or equal to 0
    private void DetectDeath()
    {
        if (updatedHealth <= 0)
        {
            DropLoot();
            Destroy(gameObject); //Destroy the monster game object 
        }
    }


    private void DropLoot()
    {
        if (LootPrefab != null)
        {
            Instantiate(LootPrefab, transform.position, Quaternion.identity); // Spawn loot at monster's position
        }
        else
        {
            Debug.LogWarning("No loot prefab assigned!");
        }
    }
}

