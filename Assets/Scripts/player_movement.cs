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

    // Start Fucntion
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        //Existing code from the video
        spriteRenderer.flipX = movement.x < 0.01 ? true : false;    

    }

    // Fixed Update
    void FixedUpdate()
    {
        // Tracks player movement
        rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);
    }
}
