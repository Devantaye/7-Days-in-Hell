using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int fullHealth = 3;

    private int updatedHealth;

    private void Start()
    {
        updatedHealth = fullHealth;
    }

    public void TakeDamage(int damage)
    {
        updatedHealth -= damage;
        Debug.Log(updatedHealth);
        DetectDeath();
    }

    private void DetectDeath()
    {
        if (updatedHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
