using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    public Animator animator;            // Reference to animator for attack animations
    public float attackRange = 1f;       // Range of attack
    public LayerMask enemyLayers;        // Layer of enemies (for attack detection)
    private testplayer playerMovement;   // Reference to player_movement script

    //Variables for attack cooldowns (to prevent spamming
    public float playerAttackCooldown = 1.0f; // Time in seconds between attacks
    private float lastAttackTime = 0f;  // Time when the last attack occurred

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to the player's movement script
        playerMovement = GetComponent<testplayer>();
        lastAttackTime = -playerAttackCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryAttack();
        }
        
    }

    // Checks if attack is off cooldown
    void TryAttack()
    {
        if (Time.time - lastAttackTime >= playerAttackCooldown)
        {
            Attack();
            lastAttackTime = Time.time; // Reset the last attack time immediately
        }
    }

    // Attack Animation + Hit detection etc
    void Attack()
    {
        // Play attack animation
        animator.SetTrigger("Attack");
        animator.SetFloat("LastHorizontal", playerMovement.lastMovement.x);
        animator.SetFloat("LastVertical", playerMovement.lastMovement.y);

        // Detect enemies
        Vector2 attackDirection = playerMovement.lastMovement;

        //Apply damage
        //damage code here
        animator.SetTrigger("AttackEnd");
    }
}
