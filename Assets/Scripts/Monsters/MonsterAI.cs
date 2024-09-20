using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    private enum State
    {
        Roaming //Monster moves around
    }

    private State state;
    private MonsterPathfinding enemyPathfinding; //Links to path finding script

    [SerializeField] private LayerMask groundLayer;  // Reference to the ground layer
    [SerializeField] private float raycastDistance = 10f;  // How far to cast the ray for ground detection


    // When first the script is first loaded:
    private void Awake()
    {
        enemyPathfinding = GetComponent<MonsterPathfinding>();
        state = State.Roaming;
    }

    // When the game begins
    private void Start()
    {
        StartCoroutine(RoamingRoutine()); // Start roaming
    }

    private IEnumerator RoamingRoutine()
    {
        while (state == State.Roaming)
        {
            Vector2 roamPosition = GetRoamingPosition(); // Receive a random position on the map
            enemyPathfinding.MoveTo(roamPosition); // Move enemy to that position 
            yield return new WaitForSeconds(5f); // Wait for 5 second interval before relocating 
        }
    }

    // Getting a new roaming position
    private Vector2 GetRoamingPosition()
    {
        // Generate a random position
        Vector2 randomPosition = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        return randomPosition;
    }

    // Check if the position is on the ground layer so that monster doesn't walk off the map
    private bool IsPositionOnGround(Vector2 position)
    {
        // Raycast downwards from the target position to check for the ground
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, raycastDistance, groundLayer);
        return hit.collider != null;  // Return true if the ray hits something on the ground layer
    }
}
