using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    // Default variables
    public Animator animator;                 // Reference to animator for attack animations
    public LayerMask enemyLayers;             // Layer of enemies (for attack detection)
    public LayerMask enemyPlayerLayers;       // Layer of enemy players (for attack detection)
    private PlayerControls playerMovement;    // Reference to player_movement script

    // Variables for attack stuff
    public float playerAttackCooldown = 4f;   // Time in seconds between attacks
    private float lastAttackTime = 0f;        // Time when the last attack occurred
    public int attackDamage = 1;              // Damage dealt by the attack
    public float attackRange = 0.3f;          // Range of attack

    void Start()
    {
        // Get reference to the player's movement script
        playerMovement = GetComponent<PlayerControls>();
        lastAttackTime = -playerAttackCooldown; // Initialize last attack time
        Debug.Log("PlayerCombat initialized. Last attack time set to: " + lastAttackTime);
    }

    void Update()
    {
        if (!IsOwner) return; // Only the owner can attack

        // Attack key = Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Attack initiated by player.");
            TryAttack();
        }
    }

    // Checks if attack is off cooldown (prevents spam)
    void TryAttack()
    {
        if (Time.time - lastAttackTime >= playerAttackCooldown)
        {
            Debug.Log("Attack successful. Cooldown passed.");
            // Host also needs to execute this code for itself
            if (IsHost)
            {
                Attack(playerMovement.lastMovement); // Perform attack locally
                lastAttackTime = Time.time; // Update last attack time
                NotifyAttackClientRpc(playerMovement.lastMovement);
                Debug.Log("Host shit ran");
            }
            else
            {
                // Trigger attack on the server
                RequestAttackServerRpc(playerMovement.lastMovement); // Pass last movement direction
            }
        }
        else
        {
            Debug.Log("Attack on cooldown. Time remaining: " + (playerAttackCooldown - (Time.time - lastAttackTime)));
        }
    }

    // Server RPC to handle attack on the server side
    [ServerRpc]
    void RequestAttackServerRpc(Vector2 attackDirection)
    {
        // Perform attack on the server
        Debug.Log("Server processing attack request.");
        Attack(attackDirection);

        // If the server is also the host, ensure the host's own attack is processed
        if (IsHost)
        {
            lastAttackTime = Time.time;  // Set the attack cooldown for the host
            Debug.Log("Host attack processed. Last attack time updated to: " + lastAttackTime);
        }

        // Notify clients to play the attack animation
        NotifyAttackClientRpc(attackDirection);
    }

    // Notify clients to play attack animation with direction
    [ClientRpc]
    void NotifyAttackClientRpc(Vector2 attackDirection)
    {
        Debug.Log("Notifying clients of attack direction: " + attackDirection);
        // For the owner, play the attack animation directly
        if (IsOwner)
        {
            animator.SetFloat("LastHorizontal", attackDirection.x);
            animator.SetFloat("LastVertical", attackDirection.y);
            animator.SetTrigger("Attack"); // Play attack animation for the owner
        }
        else
        {
            // For other clients
            animator.SetFloat("LastHorizontal", attackDirection.x);
            animator.SetFloat("LastVertical", attackDirection.y);
            animator.SetTrigger("Attack"); // Play attack animation on all clients
        }
    }

    void Attack(Vector2 attackDirection)
    {
        Debug.Log("Executing attack with direction: " + attackDirection);
        // Detect enemies + deal damage
        DetectEnemiesInCone(attackDirection); // Monsters
        DetectPlayersInCone(attackDirection); // Players
    }

    // Code to detect enemies + deal damage
    void DetectEnemiesInCone(Vector2 attackDirection)
    {
        Debug.Log("Detecting enemies in attack cone.");
        // Detect all enemies in area around player (circle)
        Collider2D[] enemies = Physics2D.OverlapCircleAll((Vector2)transform.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in enemies)
        {
            Vector2 toEnemy = (Vector2)enemy.transform.position - (Vector2)transform.position;

            // Check if enemy is in the attack range (1 of 4 directions -> cone-shaped)
            if (IsWithinCone(attackDirection, toEnemy))
            {
                // Deal damage
                MonsterHealth monsterHealth = enemy.GetComponent<MonsterHealth>();
                if (monsterHealth != null)
                {
                    monsterHealth.TakeDamage(attackDamage);
                    Debug.Log($"Hit {enemy.name} with {attackDamage} damage within cone attack area");
                }
            }
        }
    }

    // Code to detect enemy players + deal damage
    void DetectPlayersInCone(Vector2 attackDirection)
    {
        Debug.Log("Detecting enemy players in attack cone.");
        // Detect all players in area around own player (circle)
        Collider2D[] enemyPlayers = Physics2D.OverlapCircleAll((Vector2)transform.position, attackRange, enemyPlayerLayers);

        foreach (Collider2D enemyPlayer in enemyPlayers)
        {
            Vector2 toEnemyPlayer = (Vector2)enemyPlayer.transform.position - (Vector2)transform.position;

            // Check if enemy is in the attack range (1 of 4 directions -> cone-shaped)
            if (IsWithinCone(attackDirection, toEnemyPlayer))
            {
                // Deal damage
                PlayerHealth playerHealth = enemyPlayer.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    Debug.Log($"Hit {enemyPlayer.name} with {attackDamage} damage within cone attack area");
                }
            }
        }
    }

    
    // Cone check for attack directions
    bool IsWithinCone(Vector2 attackDirection, Vector2 toEnemy)
    {
        if (attackDirection == Vector2.up)
            return toEnemy.y > 0 && Mathf.Abs(toEnemy.x) < attackRange;
        if (attackDirection == Vector2.down)
            return toEnemy.y < 0 && Mathf.Abs(toEnemy.x) < attackRange;
        if (attackDirection == Vector2.left)
            return toEnemy.x < 0 && Mathf.Abs(toEnemy.y) < attackRange;
        if (attackDirection == Vector2.right)
            return toEnemy.x > 0 && Mathf.Abs(toEnemy.y) < attackRange;

        return false;
    }
}








