using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    public Animator animator;  // Reference to animator for attack animations
    public float attackRange = 1f; // Range of attack
    // public LayerMask enemyLayers; // Layer of enemies (Add later)
    private testplayer playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to the player's movement script
        playerMovement = GetComponent<testplayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
        
    }

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
    }
}
