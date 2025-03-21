using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Singleton
    public static PlayerController Instance;

    [Header("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed = 20;

    [Header("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 45;
    private float jumpBufferCounter = 0f;
    [SerializeField] private float jumpBufferFrames;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;

    [Header("Ground Check Settings")]   
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    [SerializeField] private GameObject dashEffect;

    // Variables   

    // References
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerStateList playerState;
    private float gravity;
    private float xAxis;   
    private bool canDash = true;
    private bool dashed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerState = GetComponent<PlayerStateList>();
        gravity = rb.gravityScale;
    }

    // Update is called once per frame
    private void Update()
    {
        GetInputs();
        if (playerState.dashing) return;
        Move();
        Jump();
        StartDash();
    }

    private void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }

    private void Flip()
    {
        // Move left
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            // Part 6: transform.eulerAngles = new Vector2(0, 180);
        }
        else if (xAxis > 0) // Move right
        {
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            // Part 6: transform.eulerAngles = new Vector2(0, 0);
        }
    }

    private void Move()
    {
        // Set moving vector
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        // Set updates, direction to player
        Flip();
        // Set animation 
        animator.SetBool("Walking", rb.velocity.x != 0 && Grounded());
    }

    private void StartDash()
    {
        if(Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        if (Grounded())
        {
            dashed = false;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        playerState.dashing = true;
        animator.SetTrigger("Dashing");
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        if (Grounded()) Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        playerState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) 
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX,0,0), Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Jump()
    {
        if(Input.GetButtonDown("Jump") && rb.velocity.y > 0) // is in the air?
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            // Set PlayerStateList script's variable
            playerState.jumping = false;
        }

        if (!playerState.jumping) // When not jumping
        {
            // Simple Jump mechanic
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                // Set PlayerStateList script's variable
                playerState.jumping = false;
            }
            else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump")) // In the air & have more jump & buttton down
            {
                // Set PlayerStateList script's variable
                playerState.jumping = false;

                airJumpCounter++;

                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
            }
        }

        // Set animation
        animator.SetBool("Jumping", !Grounded());

        // Set updates
        UpdateJumpingVariables();
    }

    private void UpdateJumpingVariables()
    {
        if (Grounded()) // When player in ground reset variables
        {
            // Set PlayerStateList script's variable
            playerState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            // Reset double jump
            airJumpCounter = 0;
        }
        else // When player is int the air
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter = jumpBufferCounter - Time.deltaTime * 10;
        }

    }
}
