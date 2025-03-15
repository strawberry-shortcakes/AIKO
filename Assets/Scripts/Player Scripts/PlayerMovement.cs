
using SpriteShadersUltimate.Demo;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats moveStats;
    [SerializeField] private Collider feetCollider;
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private Transform gunPivot;
    [SerializeField] private Camera mainCam;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    

    private Rigidbody rb;

    //Movement Variables
    private Vector3 moveVelocity;
    public bool isFacingRight { get; private set; }

    //Collision Check Variables 
    private RaycastHit groundHit;
    private RaycastHit headHit;
    private bool isGrounded;
    private bool bumpedHead;

    //Jump Variables
    public float verticalVelocity { get; private set; }
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numberOfJumpsUsed;

    //Apex Variables
    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    //Jump Buffer Variables
    private float jumpBufferTimer;
    private bool jumpReleasedDuringBuffer;

    //Coyote Time Variables
    private float coyoteTimer;

    //Aiming Variable
   [SerializeField] private Vector3 mousePos;
   [SerializeField] private Vector3 mouseWorldPosition;

    //Gun Variable
    private float lastTimeShot;
    private float rateOfFire = 0.2f;
    private float bulletSpeed = 15f;

    private void Awake()
    {
        isFacingRight = true;
        rb = GetComponent<Rigidbody>();

        
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        gunPivot.transform.position = transform.position;
    }

    private void Update()
    {
        JumpChecks();
        CountTimers();
        Aiming();
        Shoot();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (isGrounded)
        {
            Move(moveStats.groundAcceleration, moveStats.groundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(moveStats.airAcceleration, moveStats.airDeceleration, InputManager.Movement);
        }
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector3 moveInput)
    {
        if (moveInput != Vector3.zero)
        {
            TurnCheck(moveInput);

            Vector3 targetVelocity = Vector3.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector3(moveInput.x, 0f) * moveStats.maxRunSpeed;
            }
            else { targetVelocity = new Vector3(moveInput.x, 0f) * moveStats.maxWalkSpeed; }

            moveVelocity = Vector3.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y);
        }

        else if (moveInput == Vector3.zero)
        {
            moveVelocity = Vector3.Lerp(moveVelocity, Vector3.zero, deceleration);
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y);
        }
    }

    private void TurnCheck(Vector3 moveInput)
    {
        if (isFacingRight && moveInput.x < 0f)
        {
            Turn(false);
        }
        else if (!isFacingRight && moveInput.x > 0f)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            isFacingRight = false;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    #endregion

    #region Jump

    private void JumpChecks()
    {
        //WHEN JUMP KEY IS PRESSED
        if (InputManager.JumpWasPressed)
        {
            jumpBufferTimer = moveStats.jumpBufferTime;
            jumpReleasedDuringBuffer = false;
        }

        //WHEN JUMP IS RELEASED
        if (InputManager.JumpWasReleased)
        {
            if (jumpBufferTimer > 0f)
            {
                jumpReleasedDuringBuffer = true;
            }
        }

        //INITIATE JUMP WITH JUMP BUFFERING AND COYOTE TIME 
        if (isJumping && verticalVelocity > 0f)
        {
            if (isPastApexThreshold)
            {
                isPastApexThreshold = false;
                isFastFalling = true;
                fastFallTime = moveStats.timeForUpwardsCancel;
                verticalVelocity = 0f;
            }
            else
            {
                isFastFalling = false;
                fastFallReleaseSpeed = verticalVelocity;
            }
        }

        if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (jumpReleasedDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = verticalVelocity;
            }
        }

        //DOUBLE JUMP (IF ADDED)
        else if (jumpBufferTimer > 0f && isJumping && numberOfJumpsUsed < moveStats.numberOfJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);
        }

        //AIR JUMP AFTER COYTOE TIME LAPSE
        else if (jumpBufferTimer > 0f && isFalling && numberOfJumpsUsed < moveStats.numberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            isFastFalling = false;
        }

        //LANDED
        if ((isJumping || isFalling) && isGrounded && verticalVelocity <= 0f)
        {
            isJumping = false;
            isFalling = false;
            isFastFalling = false;
            fastFallTime = 0f;
            isPastApexThreshold = false;
            numberOfJumpsUsed = 0;

            verticalVelocity = Physics.gravity.y;
        }
    }

    private void InitiateJump(int _numberOfJumpsUsed)
    {
        if (!isJumping)
        {
            isJumping = true;
        }

        jumpBufferTimer = 0f;
        numberOfJumpsUsed += _numberOfJumpsUsed;
        verticalVelocity = moveStats.initialJumpVelocity;
    }

    private void Jump()
    {
        //APPLY GRAVITY WHILE JUMPING 
        if (isJumping)
        {
            //CHECK FOR HEAD BUMP
            if (bumpedHead)
            {
                isFastFalling = true;
            }

            //GRAVITY ON ASCENDING
            if (verticalVelocity >= 0f)
            {
                //APEX CONTROLS
                apexPoint = Mathf.InverseLerp(moveStats.initialJumpVelocity, 0f, verticalVelocity);

                if (apexPoint > moveStats.apexThreshold)
                {
                    if (!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }

                    if (isPastApexThreshold)
                    {
                        timePastApexThreshold += Time.fixedDeltaTime;
                        if (timePastApexThreshold < moveStats.apexHangTime)
                        {
                            verticalVelocity = 0f;
                        }
                        else
                        {
                            verticalVelocity = -0.01f;
                        }
                    }
                }

                //GRAVITY ON ASCENDING BUT NOT PAST APEX THRESHOLD
                else
                {
                    verticalVelocity += moveStats.gravity * Time.fixedDeltaTime;
                    if (isPastApexThreshold)
                    {
                        isPastApexThreshold = false;
                    }
                }
            }

            //GRAVITY ON DESCENDING 
            else if (!isFastFalling)
            {
                verticalVelocity += moveStats.gravity * moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (verticalVelocity < 0f)
            {
                if (!isFalling)
                {
                    isFalling = true;
                }
            }

        }

        //JUMP CUT 
        if (isFastFalling)
        {
            if (fastFallTime >= moveStats.timeForUpwardsCancel)
            {
                verticalVelocity += moveStats.gravity * moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (fastFallTime < moveStats.timeForUpwardsCancel)
            {
                verticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, fastFallTime / moveStats.timeForUpwardsCancel);
            }

            fastFallTime += Time.fixedDeltaTime;
        }

        // NORMAL GRAVITY WHILE FALLING 
        if (!isGrounded && !isJumping)
        {
            if (!isFalling)
            {
                isFalling = true;
            }

            verticalVelocity += moveStats.gravity * Time.fixedDeltaTime;
        }

        // CLAMP FALL SPEED
        verticalVelocity = Mathf.Clamp(verticalVelocity, -moveStats.maxFallSpeed, 50f);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, verticalVelocity);
    }

    #endregion

    #region Aiming/Gun

    

    private void Aiming()
    {
        Vector3 mousePos = InputManager.MousePos;

        // Clamp mouse to prevent out-of-bounds errors
        mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width);
        mousePos.y = Mathf.Clamp(mousePos.y, 0, Screen.height);

        // Use the gun pivot’s Z-depth to avoid perspective distortion
        mousePos.z = mainCam.WorldToScreenPoint(gunPivot.transform.position).z;

        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(mousePos);
        Vector3 lookDir = (mouseWorldPosition - gunPivot.transform.position).normalized;

        // Calculate rotation
        float rotZ = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        gunPivot.transform.rotation = Quaternion.Euler(0, 0, rotZ);
    }

    private void Shoot()
    {
        if(Time.time < lastTimeShot + rateOfFire)
        {
            return;
        }

        lastTimeShot = Time.time;

        if (InputManager.ShootWasPressed)
        {
            GameObject newBullet = Instantiate(bulletPrefab);
            newBullet.transform.position = bulletSpawnPoint.transform.position;
            newBullet.transform.rotation = gunPivot.transform.rotation;
            Rigidbody bulletRB = newBullet.GetComponent<Rigidbody>();
            bulletRB.linearVelocity = bulletSpawnPoint.transform.up * bulletSpeed;
        }
    }

    #endregion

    #region Collision Checks

    private void BumpedHead()
    {
        Vector3 boxCastOrigins = new Vector3(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y);
        Vector3 boxCastSize = new Vector3(bodyCollider.bounds.size.x, moveStats.groundDetectionRayLength);

        if (Physics.BoxCast(boxCastOrigins, boxCastSize, Vector3.up, Quaternion.identity, moveStats.headDetectionRayLength, moveStats.groundLayer))
        {
            bumpedHead = true;
        }
        else
        {
            bumpedHead = false;
        }


        #region Debug Visualization
        if (moveStats.DebugShowIsHeadBumpedBox)
        {
            float headWidth = moveStats.headWidth;

            Color rayColor;
            if (bumpedHead)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector3(boxCastOrigins.x - boxCastSize.x / 2, boxCastOrigins.y), Vector3.up * moveStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector3(boxCastOrigins.x + boxCastSize.x / 2, boxCastOrigins.y), Vector3.up * moveStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector3(boxCastOrigins.x - boxCastSize.x / 2, boxCastOrigins.y - moveStats.headDetectionRayLength), Vector3.right * boxCastSize.x * headWidth, rayColor);
        }
        #endregion
    }
    private void IsGrounded()
    {
        Vector3 boxCastOrigins = new Vector3(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
        Vector3 boxCastSize = new Vector3(feetCollider.bounds.size.x, moveStats.groundDetectionRayLength);

        if (Physics.BoxCast(boxCastOrigins, boxCastSize, Vector3.down, Quaternion.identity, moveStats.groundDetectionRayLength, moveStats.groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }


        #region Debug Visualization
        if (moveStats.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if (isGrounded)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector3(boxCastOrigins.x - boxCastSize.x / 2, boxCastOrigins.y), Vector3.down * moveStats.groundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector3(boxCastOrigins.x + boxCastSize.x / 2, boxCastOrigins.y), Vector3.down * moveStats.groundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector3(boxCastOrigins.x - boxCastSize.x / 2, boxCastOrigins.y - moveStats.groundDetectionRayLength), Vector3.right * boxCastSize.x, rayColor);
        }
        #endregion


    }

    private void CollisionChecks()
    {
        IsGrounded();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        jumpBufferTimer -= Time.deltaTime;

        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else
        {
            coyoteTimer = moveStats.jumpCoyoteTime;
        }
    }

    #endregion
}