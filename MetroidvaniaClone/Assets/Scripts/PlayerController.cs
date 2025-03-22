using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Singleton
    public static PlayerController Instance;

    [Header("Health Settings")]
    [SerializeField] public int maxHealth;
    public int health;   

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

    [Header("Attack Settings")]
    [SerializeField] private Transform sideAttackTransform;
    [SerializeField] private Transform upAttackTransform;
    [SerializeField] private Transform downAttackTransform;
    [SerializeField] private Vector2 sideAttackArea;
    [SerializeField] private Vector2 upAttackArea;
    [SerializeField] private Vector2 downAttackArea;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private float damage;
    [SerializeField] private GameObject slashEffect;
    private bool attack = false;
    private float timeBetweenAttack;
    private float timeSinceAttack;

    [Header("Recoil Settings")]
    [SerializeField] private int recoilXStep = 5;
    [SerializeField] private int recoilYStep = 5;
    [SerializeField] private float recoilXSpeed= 100;
    [SerializeField] private float recoilYSpeed = 100;
    private int stepsXRecoiled;
    private int stepsYRecoiled;


    // Variables   

    // References
    [HideInInspector] public PlayerStateList playerState;

    private Rigidbody2D rb;
    private Animator animator;   
    private float gravity;
    private float xAxis;
    private float yAxis;
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
        health = maxHealth;
    }

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerState = GetComponent<PlayerStateList>();
        gravity = rb.gravityScale;
    }

    // Show gizmos on Scene
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(sideAttackTransform.position, sideAttackArea);
        Gizmos.DrawWireCube(upAttackTransform.position, upAttackArea);
        Gizmos.DrawWireCube(downAttackTransform.position, downAttackArea);
    }

    // Update is called once per frame
    private void Update()
    {
        GetInputs();
        if (playerState.dashing) return;
        Move();
        Jump();
        StartDash();
        Attack();
        Recoil();
    }

    private void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetMouseButtonDown(0); // Mouse left click
    }

    private void Flip()
    {
        // Move left
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            // Part 6: transform.eulerAngles = new Vector2(0, 180);
            
            // Set global direction
            playerState.lookingRight = false;
        }
        else if (xAxis > 0) // Move right
        {
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            // Part 6: transform.eulerAngles = new Vector2(0, 0);

            // Set global direction
            playerState.lookingRight = true;
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

    #region Attack

    private void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if(attack && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            // Set animation
            animator.SetTrigger("Attacking");

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(sideAttackTransform, sideAttackArea, ref playerState.recoilingX, recoilXSpeed);
                // Add effect
                Instantiate(slashEffect, sideAttackTransform);
            }
            else if (yAxis > 0)
            {
                Hit(upAttackTransform, upAttackArea, ref playerState.recoilingY, recoilYSpeed);
                // Add effect
                SlashEffectAtAngle(slashEffect, 80, upAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(downAttackTransform, downAttackArea, ref playerState.recoilingY, recoilYSpeed);
                // Add effect
                SlashEffectAtAngle(slashEffect, -90, downAttackTransform);
            }
        }
    }

    private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        if(objectsToHit.Length > 0)
        {
            _recoilDir = true;
        }

        for(int i =0; i< objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
            }
        }
    }

    private void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        // Create a new effect
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        // Rotate the effect
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        // Strach it to fit the attack area
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }

    private void Recoil()
    {
        if(playerState.recoilingX)
        {
            if (playerState.lookingRight) // looking right
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0); // pushed back to left
            }
            else // looking left
            {
                rb.velocity = new Vector2(recoilXSpeed, 0); // pushed back to right
            }
        }

        if (playerState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {            
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        // Stop recoil X
        if (playerState.recoilingX && stepsXRecoiled < recoilXStep)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        // Stop recoil Y
        if (playerState.recoilingY && stepsYRecoiled < recoilYStep)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if(Grounded())
        {
            StopRecoilY();
        }
    }

    private void StopRecoilX()
    {
        stepsXRecoiled = 0;
        playerState.recoilingX = false;
    }

    private void StopRecoilY()
    {
        stepsYRecoiled = 0;
        playerState.recoilingY = false;
    }

    #endregion


    public void TakeDamege(float _damage)
    {
        health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
    }

    private IEnumerator StopTakingDamage()
    {
        playerState.invincible = true;
        animator.SetTrigger("TakeDamage");
        ClampHealth();
        yield return new WaitForSeconds(1f);
        playerState.invincible = false;
    }

    private void ClampHealth()
    {
        // The health can go beyond the minimum or maximum 
        health = Mathf.Clamp(health, 0, maxHealth);
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
