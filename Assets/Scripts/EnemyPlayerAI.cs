using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayerAI : MonoBehaviour
{
    public float moveSpeed = 2f;               // Speed of movement
    public float waitTime = 1f;                // Time to wait at each waypoint
    public int attackCount = 2;                // Number of attacks at each waypoint
    public float attackDelay = 0.5f;           // Delay between attacks
    public float squareSideLength = 1f;        // Size of the square to walk

    private Vector2[] waypoints;               // Waypoints for the square path
    private int currentWaypointIndex = 0;      // Current waypoint the NPC is moving towards
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;      // Sprite renderer to flip the sprite
    private EnemyPlayerCombat enemyCombat;     // Reference to combat script

    private bool isWaiting = false;
    private int currentDirection = 1;          // Current movement direction (1=right, 2=left, 3=up, 4=down)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCombat = GetComponent<EnemyPlayerCombat>();

        // Set up the four waypoints for the square
        waypoints = new Vector2[4];
        waypoints[0] = (Vector2)transform.position + new Vector2(squareSideLength, 0);
        waypoints[1] = waypoints[0] + new Vector2(0, squareSideLength);
        waypoints[2] = waypoints[1] - new Vector2(squareSideLength, 0);
        waypoints[3] = waypoints[2] - new Vector2(0, squareSideLength);

        StartCoroutine(Patrol());
    }

    IEnumerator Patrol()
    {
        while (true)
        {
            if (!isWaiting)
            {
                Vector2 nextWaypoint = waypoints[currentWaypointIndex];
                Vector2 direction = (nextWaypoint - (Vector2)transform.position).normalized;

                rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

                // Determine movement direction for animation and attack direction
                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    // Horizontal movement
                    if (direction.x > 0)
                    {
                        currentDirection = 1; // Right
                        spriteRenderer.flipX = false;  // Facing right
                    }
                    else
                    {
                        currentDirection = 2; // Left
                        spriteRenderer.flipX = true;   // Facing left
                    }
                }
                else
                {
                    // Vertical movement
                    if (direction.y > 0)
                    {
                        currentDirection = 3; // Up
                    }
                    else
                    {
                        currentDirection = 4; // Down
                    }
                }

                // Set animator parameters based on movement direction
                animator.SetFloat("Horizontal", direction.x);
                animator.SetFloat("Vertical", direction.y);
                animator.SetFloat("Speed", moveSpeed);

                // If close to the next waypoint, start waiting and attacking
                if (Vector2.Distance(transform.position, nextWaypoint) < 0.1f)
                {
                    isWaiting = true;
                    StartCoroutine(WaitAndAttack());
                }
            }
            yield return null;
        }
    }

    IEnumerator WaitAndAttack()
    {
        animator.SetFloat("Speed", 0);

        // Attack multiple times
        for (int i = 0; i < attackCount; i++)
        {
            enemyCombat.TriggerAttack(currentDirection);  // Pass the current direction for the attack
            yield return new WaitForSeconds(attackDelay);
        }

        yield return new WaitForSeconds(waitTime);

        // Move to the next waypoint
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        isWaiting = false;
    }
}

