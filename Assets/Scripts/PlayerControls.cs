using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerControls : NetworkBehaviour
{
    public static PlayerControls Instance;

    // Variables Declared
    public float moveSpeed = 5f;            // Adjustable movement speed
    public Rigidbody2D rb;                  // Rigid body
    public Animator animator;               // Reference to animator
    public SpriteRenderer spriteRenderer;   // Sprite renderer to mirror right_walk animation
    Vector2 movement;                       // For walking animations
    public Vector2 lastMovement;            // For attack/idle animation directions
    public GameObject heartEmote;           // Reference to heart emote
    public float emoteDuration = 1.5f;      // Controls emote length
    private Knockback knockback;

    // Start Fucntion
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastMovement = Vector2.down; // Default idle animation
        heartEmote.SetActive(false); // Starts with emote hidden
    }

    private void Awake()
    {
        Instance = this;
        knockback = GetComponent<Knockback>();
    }

    private void Move()
    {
        if (knockback.gettingPushedBack) { return; }

    }

    // Update every frame
    void Update()
    {
        if (!IsOwner) return;
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

        //Emote code
        if (movement == Vector2.zero && Input.GetKeyDown(KeyCode.B))
        {
            //heart emote code
            animator.SetTrigger("Heart"); // Add later for player emotes
            heartEmote.SetActive(true);
            Invoke("hideHeartEmote", emoteDuration);

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

    // Untoggles heart emote
    void hideHeartEmote()
    {
        heartEmote.SetActive(false);
    }
}
