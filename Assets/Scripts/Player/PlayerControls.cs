using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class PlayerControls : NetworkBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vc;
    [SerializeField] private AudioListener listener;
    // Default variables
    public Rigidbody2D rb;                     // Rigid body
    public Animator animator;                  // Reference to animator
    public SpriteRenderer spriteRenderer;      // Sprite renderer to mirror right_walk animation
    public GameObject heartEmote;              // Reference to heart emote (only works interally atm)

    // Player control stuff
    public float moveSpeed = 5f;               // Adjustable movement speed
    private Vector2 movement;                  // For walking animations
    public Vector2 lastMovement;               // For attack/idle animation directions
    public float emoteDuration = 1.5f;         // Controls emote length
    private Knockback knockback;               // For monster knockback
    public static PlayerControls Instance;     // Reference to instance for network stuff

    public CoinManager Cm; // Reference to the CoinManager 

    // Network Variables for Animation syncing (local multiplayer)
    private NetworkVariable<float> horizontal = new NetworkVariable<float>(0);
    private NetworkVariable<float> vertical = new NetworkVariable<float>(0);
    private NetworkVariable<float> speed = new NetworkVariable<float>(0);
    private NetworkVariable<float> lastHorizontal = new NetworkVariable<float>(0);
    private NetworkVariable<float> lastVertical = new NetworkVariable<float>(0);
    private NetworkVariable<bool> isFacingLeft = new NetworkVariable<bool>(false);

    /*   ==========================================
         LIFECYCLE METHODS - Start() , Update() etc
         ==========================================   */

    void Start()
    {
        // Get reference to sprite renderer + set default idle pose
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastMovement = Vector2.down;
        heartEmote.SetActive(false);
    }

    // Awake function - For knockback
    public override void OnNetworkSpawn()
    {
        if (IsOwner) {
            listener.enabled = true;
            vc.Priority = 1;
        }else {
            vc.Priority = 0;
        }
    }
    private void Awake()
    {
        Instance = this;
        knockback = GetComponent<Knockback>();
    }

    void Update()
    {
        if (!IsOwner) return;

        // Controls player movement + animations
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // Tracks the last movement direction (for animations
        if (movement != Vector2.zero)
        {
            lastMovement = movement;
        }

        // Set idle animation direction based on last movement
        animator.SetFloat("LastHorizontal", lastMovement.x);
        animator.SetFloat("LastVertical", lastMovement.y);


        // Sync animator with server
        UpdateAnimatorParametersServerRpc(movement.x, movement.y, movement.sqrMagnitude, lastMovement.x, lastMovement.y);

        // Flipping animation based on last movement
        if (movement.x < 0) // Facing left
        {
            spriteRenderer.flipX = true;  
            UpdateFacingDirectionServerRpc(true); 
        }
        else if (movement.x > 0) // Facing right
        {
            spriteRenderer.flipX = false; 
            UpdateFacingDirectionServerRpc(false); 
        }

        // Emote code - *For fun*
        if (movement == Vector2.zero && Input.GetKeyDown(KeyCode.B))
        {
            animator.SetTrigger("Heart");
            heartEmote.SetActive(true);
            Invoke("hideHeartEmote", emoteDuration);
        }
    }

    //Fixed update function - normalizes movement speed
    void FixedUpdate()
    {
        if (!IsOwner) return;

        rb.velocity = movement.normalized * moveSpeed;
    }


    /*   ===============================================
         NETCODE METHODS - For animation/action syncing
         ===============================================   */

    [ServerRpc] // Update player animator
    void UpdateAnimatorParametersServerRpc(float newHorizontal, float newVertical, float newSpeed, float newLastHorizontal, float newLastVertical)
    {
        horizontal.Value = newHorizontal;
        vertical.Value = newVertical;
        speed.Value = newSpeed;
        lastHorizontal.Value = newLastHorizontal;
        lastVertical.Value = newLastVertical;

        UpdateAnimatorParametersClientRpc(horizontal.Value, vertical.Value, speed.Value, lastHorizontal.Value, lastVertical.Value); // Informs clients (below)
    }

    [ServerRpc] // Update player facing direction (sprite flipper)
    void UpdateFacingDirectionServerRpc(bool facingLeft)
    {
        isFacingLeft.Value = facingLeft; 
        UpdateFacingDirectionClientRpc(isFacingLeft.Value); // Informs clients (below)
    }


    [ClientRpc] // Update own direction
    void UpdateAnimatorParametersClientRpc(float horizontal, float vertical, float speed, float lastHorizontal, float lastVertical)
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Speed", speed);
        animator.SetFloat("LastHorizontal", lastHorizontal);
        animator.SetFloat("LastVertical", lastVertical);
    }

    [ClientRpc] // Update own facing direction
    void UpdateFacingDirectionClientRpc(bool facingLeft)
    {
        spriteRenderer.flipX = facingLeft; 
    }

    /*   ======================================
         ADDITIONAL STUFF - Other functions etc
         ======================================   */
    void hideHeartEmote() //For funsies (only displays locally atm)
    {
        heartEmote.SetActive(false);
    }

    // Increase coin count when player walks over coin
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            Cm.coinCount++;
        }

    }
}
