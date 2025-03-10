using NUnit;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    //COMPONENTS
    [Header("===COMPONENTS===")]
    public Rigidbody rb;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public Transform groundCheck;
    public Transform wallCheck;
    public CustomGravityScript gravityScript;

    //PLAYER STATS
    [Header("===PLAYER STATS===")]
    public float speed = 10;

    public float defaultGravity = 1.5f;
    public float gravityMultiplier = 2;

    public float jumpForce = 10;
    public float jumpHoldTime;
    public float maxJumpHoldTime = 0.4f;

    [Header("===WALL JUMPING & SLIDING===")]
    public float wallSlidingSpeed = 2;
    public float wallJumpingDirection;
    public float wallJumpingTime = 0.2f;
    public float wallJumpingCounter; 
    public float wallJumpingDuration = 0.4f;
    public Vector3 wallJumpingPower = new Vector3(8, 16);

   
    
 

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    public float horizontal;

    //PLAYER BOOLS
    [Header("===PLAYER CONDITIONS===")]
    [SerializeField] private bool isWallSliding;
    [SerializeField] private bool isHoldingJump;
    [SerializeField] public bool isWallJumping;
    [SerializeField] private bool isFacingRight;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityScript = GetComponent<CustomGravityScript>();
       
        
    }

    // Update is called once per frame
    void Update()
    { 
        horizontal = Input.GetAxisRaw("Horizontal");
        

        // Allows the player to jump slightly after leaving the ground 
        if (isGrounded())
        {
            coyoteTimeCounter = coyoteTime;
            gravityScript.gravityScale = defaultGravity;
        }
        else
        {coyoteTimeCounter -= Time.deltaTime;}

        // Allows the player to jump again slightly before hitting the ground 
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
            isHoldingJump = true;
        }
        else
        {jumpBufferCounter -= Time.deltaTime;}

        // Allows the player to jump
        if(jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            jumpBufferCounter = 0f;
        }

        // Stops jump if player lets go of Space
        if (Input.GetButtonUp("Jump"))
        {
            if(rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
                coyoteTimeCounter = 0f;
            }

            isHoldingJump = false;
        }

        WallSlide();
        WallJump();

       
        

        if (!isWallJumping)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }

        

        // Stops upward force after a certain amount of time
        if (isHoldingJump)
        {
            jumpHoldTime += Time.fixedDeltaTime;
            gravityScript.gravityScale -= Time.deltaTime * 0.5f;

            if(jumpHoldTime >= maxJumpHoldTime)
            {
                isHoldingJump = false;
                jumpHoldTime = 0f;
            }
        }

        if (!isHoldingJump && !isGrounded())
        {
            gravityScript.gravityScale += Time.deltaTime * gravityMultiplier;
            if(gravityScript.gravityScale >= 5f)
            {
                gravityScript.gravityScale = 5f;
            }
        }
    }

    // Checks to see if player is grounded
    private bool isGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);
    }

    //Checks to see if player is touching a wall
    private bool isWalled()
    {
        return Physics.CheckSphere(wallCheck.position, 0.3f, wallLayer);
        
    }

    

    private void WallSlide()
    {
        if(isWalled() && !isGrounded() && horizontal != 0f)
        {
            gravityScript.gravityScale = defaultGravity;
            isWallSliding = true;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue), rb.linearVelocity.z);
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if(Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;
            isWallSliding = false;

            if(transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void Flip()
    {
        if(isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }
}
