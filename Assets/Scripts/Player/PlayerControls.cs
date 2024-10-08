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
    private Vector2 movement;                // For walking animations
    public Vector2 lastMovement;            // For attack/idle animation directions
    public GameObject heartEmote;           // Reference to heart emote
    public float emoteDuration = 1.5f;      // Controls emote length
    private Knockback knockback;

    // Network Variables for Animation Parameters
    private NetworkVariable<float> horizontal = new NetworkVariable<float>(0);
    private NetworkVariable<float> vertical = new NetworkVariable<float>(0);
    private NetworkVariable<float> speed = new NetworkVariable<float>(0);
    private NetworkVariable<float> lastHorizontal = new NetworkVariable<float>(0);
    private NetworkVariable<float> lastVertical = new NetworkVariable<float>(0);
    private NetworkVariable<bool> isFacingLeft = new NetworkVariable<bool>(false);




    // Start Function
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastMovement = Vector2.down;
        heartEmote.SetActive(false);
    }

    private void Awake()
    {
        Instance = this;
        knockback = GetComponent<Knockback>();
    }

    void Update()
    {
        if (!IsOwner) return;

        // Controls player movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Controls walking animation
        // Set animator parameters directly from local movement
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // Tracks the last movement direction
        if (movement != Vector2.zero)
        {
            lastMovement = movement;
        }

        // Set idle animation direction based on last movement
        animator.SetFloat("LastHorizontal", lastMovement.x);
        animator.SetFloat("LastVertical", lastMovement.y);


        // Sync animator parameters with server
        UpdateAnimatorParametersServerRpc(movement.x, movement.y, movement.sqrMagnitude, lastMovement.x, lastMovement.y);

        // Flipping animation based on last movement
        if (movement.x < 0)
        {
            spriteRenderer.flipX = true;  // Facing left
            UpdateFacingDirectionServerRpc(true); // Update server that the player is facing left
        }
        else if (movement.x > 0)
        {
            spriteRenderer.flipX = false; // Facing right
            UpdateFacingDirectionServerRpc(false); // Update server that the player is facing right
        }


        // Emote code
        if (movement == Vector2.zero && Input.GetKeyDown(KeyCode.B))
        {
            animator.SetTrigger("Heart");
            heartEmote.SetActive(true);
            Invoke("hideHeartEmote", emoteDuration);
        }
    }

    [ServerRpc]
    void UpdateAnimatorParametersServerRpc(float newHorizontal, float newVertical, float newSpeed, float newLastHorizontal, float newLastVertical)
    {
        horizontal.Value = newHorizontal;
        vertical.Value = newVertical;
        speed.Value = newSpeed;
        lastHorizontal.Value = newLastHorizontal;
        lastVertical.Value = newLastVertical;

        UpdateAnimatorParametersClientRpc(horizontal.Value, vertical.Value, speed.Value, lastHorizontal.Value, lastVertical.Value);
    }

    [ServerRpc]
    void UpdateFacingDirectionServerRpc(bool facingLeft)
    {
        isFacingLeft.Value = facingLeft; // Update facing direction on the server
        UpdateFacingDirectionClientRpc(isFacingLeft.Value); // Inform all clients
    }


    [ClientRpc]
    void UpdateAnimatorParametersClientRpc(float horizontal, float vertical, float speed, float lastHorizontal, float lastVertical)
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Speed", speed);
        animator.SetFloat("LastHorizontal", lastHorizontal);
        animator.SetFloat("LastVertical", lastVertical);
    }

    [ClientRpc]
    void UpdateFacingDirectionClientRpc(bool facingLeft)
    {
        spriteRenderer.flipX = facingLeft; // Set the sprite flip state based on the server's value
    }


    void FixedUpdate()
    {
        if (!IsOwner) return;

        rb.velocity = movement.normalized * moveSpeed;
    }

    void hideHeartEmote()
    {
        heartEmote.SetActive(false);
    }
}
