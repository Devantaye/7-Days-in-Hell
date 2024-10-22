using System;
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

    // Player abilities
    // [1] Dash
    private float dashSpeed = 13f;       // Dash speed
    private float dashDuration = 0.1f;   // Dash duration
    private float dashCooldown = 15f;     // Dash cooldown
    private bool isDashing = false;     // Variable to control dash toggle
    private float dashTime;             // controls dashcooldown
    private float lastDashTime;     // Also controls dashcooldown
    // [2] Invis
    public bool isInvis = false;
    private float invisDuration = 4f;   // Dash duration
    private float invisCooldown = 15f;     // Dash cooldown
    private float invisTime;             // controls dashcooldown
    private float lastInvisTime;     // Also controls dashcooldown

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
        lastDashTime = -dashCooldown;
        lastInvisTime = -invisCooldown;

   
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

        // Update the player's position based on the movement
        Vector3 moveVector = new Vector2(movement.x, movement.y) * moveSpeed * Time.deltaTime;
        transform.position += moveVector; // Move the player

        // Sync animator with server
        UpdateAnimatorParametersServerRpc(movement.x, movement.y, movement.sqrMagnitude, lastMovement.x, lastMovement.y);

        // only flips if player isnt dashing
        if (!isDashing)
        {
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
        }

        // Dash ability code
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && Time.time - lastDashTime > dashCooldown)
        {
            StartDash();
        }

        if (isDashing)
        {
            ContinueDash();
        }

        // Invis ability code
        if (Input.GetKeyDown(KeyCode.E) && !isInvis && Time.time - lastInvisTime > invisCooldown)
        {
            StartInvis();
        }

        // Emote code - *For fun*
        if (movement == Vector2.zero && Input.GetKeyDown(KeyCode.B))
        {
            animator.SetTrigger("Heart");
            heartEmote.SetActive(true);
            Invoke("hideHeartEmote", emoteDuration);
        }

    }

    private void StartDash()
    {
        isDashing = true;
        dashTime = Time.time + dashDuration;
        lastDashTime = Time.time;

        Debug.Log("Time since last dash: " + (Time.time - lastDashTime));
        Debug.Log("Current dash speed: " + dashSpeed);
   


        // Sync dash start with the server
        StartDashServerRpc();
    }

    private void ContinueDash()
    {
        if (Time.time < dashTime)
        {
            transform.position += (Vector3)lastMovement.normalized * dashSpeed * Time.deltaTime;
        }
        else
        {
            isDashing = false;
        }
    }

    private void StartInvis()
    {
        if (isInvis) return;

        isInvis = true;
        lastInvisTime = Time.time;

        // Start fading out
        StartCoroutine(FadeOut());

        StartInvisServerRpc();
    }

    private IEnumerator FadeOut()
    {
        float fadeDuration = 1f; // The time it takes to fade
        float startAlpha = 1f;   // Start fully visible
        float endAlpha = IsOwner ? 0.5f : 0f; // Player sees themselves at 50%, others see fully invisible
        Color spriteColor = spriteRenderer.color;

        // Gradually change alpha over time
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            spriteColor.a = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            spriteRenderer.color = spriteColor;
            yield return null;
        }

        // Ensure it ends at the target alpha
        spriteColor.a = endAlpha;
        spriteRenderer.color = spriteColor;

        // Keep player invisible for the duration
        yield return new WaitForSeconds(invisDuration);

        // Fade back in after invisibility duration
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        isInvis = false; // End invisibility

        float fadeDuration = 1f; // Time to fade back in
        float startAlpha = IsOwner ? 0.5f : 0f; // Start at 50% for player, 0% for enemies
        float endAlpha = 1f; // Fully visible
        Color spriteColor = spriteRenderer.color;

        // Gradually change alpha back to fully visible
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            spriteColor.a = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            spriteRenderer.color = spriteColor;
            yield return null;
        }

        // Ensure it's fully visible at the end
        spriteColor.a = endAlpha;
        spriteRenderer.color = spriteColor;
    }



    //Fixed update function - normalizes movement speed
    void FixedUpdate()
    {
        if (!IsOwner) return;
        if (!isDashing)
        {
            // Only runs when player isnt dashing
            rb.velocity = movement.normalized * moveSpeed;
        }
        
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

    [ServerRpc] // Sync dash ability
    private void StartDashServerRpc()
    {
        isDashing = true;
        dashTime = Time.time + dashDuration;
        lastDashTime = Time.time;
    }

    [ServerRpc]
    private void StartInvisServerRpc()
    {
        // Sync invisibility to all clients
        UpdateInvisibilityClientRpc();
    }

    [ClientRpc]
    private void UpdateInvisibilityClientRpc()
    {
        // Start the fade-out coroutine to make the player invisible
        if (!IsOwner) // Only run the fade-out for other players' view
        {
            StartCoroutine(FadeOut());
        }
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
