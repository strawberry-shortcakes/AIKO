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

    //PLAYER STATS
    [Header("===PLAYER STATS===")]
    public float speed = 10;
    public float jumpForce = 10;
    public float jumpHoldTime;
    public float maxJumpHoldTime = 0.4f;


    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    public float horizontal;

    //PLAYER BOOLS
    [Header("===PLAYER CONDITIONS===")]
    public bool isGrounded;
    public bool isWallSliding;
    public bool isOnWall;
    public bool isHoldingJump;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();

        // Allows the player to jump slightly after leaving the ground 
        if (isGrounded)
        {coyoteTimeCounter = coyoteTime;}
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
    }

    private void FixedUpdate()
    {
        // Lets the player move left and right
        horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector3(horizontal * speed, rb.linearVelocity.y, rb.linearVelocity.z);

        // Stops upward force after a certain amount of time
        if (isHoldingJump)
        {
            jumpHoldTime += Time.fixedDeltaTime;
            if(jumpHoldTime >= maxJumpHoldTime)
            {
                isHoldingJump = false;
                jumpHoldTime = 0f;
            }
        }
    }

    void GroundCheck()
    {
        if(Physics.Raycast(groundCheck.transform.position, transform.TransformDirection(Vector3.down), 0.2f, groundLayer))
        {
            isGrounded = true;
            Debug.DrawRay(groundCheck.transform.position, transform.TransformDirection(Vector3.down) * 10f, Color.green);
        }
        else
        {
            isGrounded= false;
            Debug.DrawRay(groundCheck.transform.position, transform.TransformDirection(Vector3.down) * 10f, Color.red);

        }
    }
}
