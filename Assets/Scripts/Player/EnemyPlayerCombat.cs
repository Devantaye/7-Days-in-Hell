using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayerCombat : MonoBehaviour
{
    // Default variables
    public Animator animator;                 // Reference to animator for attack animations
    public LayerMask enemyLayers;             // Layer of enemies (for attack detection)
    public LayerMask enemyPlayerLayers;       // Layer of enemy players (for attack detection)

    // Variables for attack stuff
    public int attackDamage = 1;              // Damage dealt by the attack
    public float attackRange = 0.3f;          // Range of attack

    // Automatically called by the AI controller when the NPC is ready to attack
    public void TriggerAttack(int direction)
    {
        TryAttack(direction);
    }

    // Tries to attack in a specific direction (0 = no attack, 1 = right, 2 = left, 3 = up, 4 = down)
    public void TryAttack(int direction)
    {
        if (direction != 0) // Ensure there is a direction to attack
        {
            Attack(direction);
        }
    }

    void Attack(int direction)
    {
        // Play attack animation
        animator.SetTrigger("Attack");

        // Set animation direction based on input direction
        switch (direction)
        {
            case 1: // Right
                animator.SetFloat("LastHorizontal", 1);
                animator.SetFloat("LastVertical", 0);
                break;
            case 2: // Left
                animator.SetFloat("LastHorizontal", -1);
                animator.SetFloat("LastVertical", 0);
                break;
            case 3: // Up
                animator.SetFloat("LastHorizontal", 0);
                animator.SetFloat("LastVertical", 1);
                break;
            case 4: // Down
                animator.SetFloat("LastHorizontal", 0);
                animator.SetFloat("LastVertical", -1);
                break;
        }

        // Detect enemies and deal damage
        DetectEnemiesInCone(direction); // Monsters
        DetectPlayersInCone(direction); // Players
    }

    // Code to detect enemies + deal damage
    void DetectEnemiesInCone(int direction)
    {
        // Detect all enemies in area around player (circle)
        Collider2D[] enemies = Physics2D.OverlapCircleAll((Vector2)transform.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in enemies)
        {
            Vector2 toEnemy = (Vector2)enemy.transform.position - (Vector2)transform.position;

            // Check if enemy is in the attack range (cone-shaped)
            if (IsWithinCone(direction, toEnemy))
            {
                // Deal damage
                MonsterHealth MonsterHealth = enemy.GetComponent<MonsterHealth>();
                if (MonsterHealth != null)
                {
                    MonsterHealth.TakeDamage(attackDamage);
                }
                Debug.Log($"Hit {enemy.name} with {attackDamage} damage within cone attack area");
            }
        }
    }

    // Code to detect players + deal damage
    void DetectPlayersInCone(int direction)
    {
        // Detect all players in area around own player (circle)
        Collider2D[] enemyPlayers = Physics2D.OverlapCircleAll((Vector2)transform.position, attackRange, enemyPlayerLayers);

        foreach (Collider2D enemyPlayer in enemyPlayers)
        {
            Vector2 toEnemyPlayer = (Vector2)enemyPlayer.transform.position - (Vector2)transform.position;

            // Check if enemy player is in the attack range (cone-shaped)
            if (IsWithinCone(direction, toEnemyPlayer))
            {
                // Deal damage
                PlayerHealth playerHealth = enemyPlayer.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
                Debug.Log($"Hit {enemyPlayer.name} with {attackDamage} damage within cone attack area");
            }
        }
    }

    // Cone check for attack directions based on AI's movement direction
    bool IsWithinCone(int direction, Vector2 toEnemy)
    {
        if (direction == 1) // Right
            return toEnemy.x > 0 && Mathf.Abs(toEnemy.y) < attackRange;
        if (direction == 2) // Left
            return toEnemy.x < 0 && Mathf.Abs(toEnemy.y) < attackRange;
        if (direction == 3) // Up
            return toEnemy.y > 0 && Mathf.Abs(toEnemy.x) < attackRange;
        if (direction == 4) // Down
            return toEnemy.y < 0 && Mathf.Abs(toEnemy.x) < attackRange;

        return false;
    }
}


