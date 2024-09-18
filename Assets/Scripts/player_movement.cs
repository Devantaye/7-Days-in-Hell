using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testplayer : MonoBehaviour
{
    // Variables Declared
    public float moveSpeed = 5f;      // Adjustable movement speed
    public Rigidbody2D rb;            // Rigid body
    public Animator animator;         // Reference to animator
    public SpriteRenderer spriteRenderer;    // Sprite renderer to mirror right_walk animation
    Vector2 movement;
    public Vector2 lastMovement;

    // Start Fucntion
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastMovement = Vector2.down; //Default idle animation
    }

    // Update every frame
    void Update()
    {
        // Controls player movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Controls walking animation
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // Tracks the last movement direction to setup idle animations
        if (movement != Vector2.zero)
        {
            lastMovement = movement;
        }

        // Set idle animation direction based on last movement
        animator.SetFloat("LastHorizontal", lastMovement.x);
        animator.SetFloat("LastVertical", lastMovement.y);

        // Flipping animation when player is moving
        spriteRenderer.flipX = movement.x < 0.01 ? true : false;

        // Flipping animation when player is moving
        if (lastMovement.x < 0)
        {
            spriteRenderer.flipX = true;  // Facing left
        }
        else if (lastMovement.x > 0)
        {
            spriteRenderer.flipX = false; // Facing right
        }
    }

    // Fixed Update
    void FixedUpdate()
    {
        // Tracks player movement
        // rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

        /* Use velocity for smoother movement 
         * by Alex
        */
        rb.velocity = movement.normalized * moveSpeed;
    }
}
