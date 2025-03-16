
using SpriteShadersUltimate.Demo;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
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
    public float horizontalVelocity { get; private set; } 
    public bool isFacingRight { get; private set; }

    //Collision Check Variables 
    private RaycastHit groundHit;
    private RaycastHit headHit;
    private bool isGrounded;
    private bool bumpedHead;
    private bool isTouchingWall;

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

    //Wall Slide
    private bool isWallSliding;
    private bool isWallSlideFalling;

    //Wall Jump 
    private bool useWallJumpMoveStats;
    private bool isWallJumping;
    private float wallJumpTIme;
    private bool isWallJumpFastFalling;
    private bool isWallJumpFalling;
    private float wallJumpFastFallTime;
    private float wallJumpFastFallReleaseSpeed;

    private float wallJumpPostBufferTimer;

    private float wallJumpApexPoint;
    private float timePastWallJumpApexThreshold;
    private bool isPastWallJumpApexThreshold;

    //Aiming Variable
    private Vector3 mousePos;
    private Vector3 mouseWorldPosition;

    //Gun Variable
    private float lastTimeShot;
    private float rateOfFire = 0.5f;
    private float bulletSpeed = 30f;

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
        CountTimers();
        JumpChecks();
        LandCheck();

        WallSlideCheck();

        Aiming();
        Shoot();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        Fall();
        WallSlide();

        if (isGrounded)
        {
            Move(moveStats.groundAcceleration, moveStats.groundDeceleration, InputManager.Movement);
        }
        else
        {
            //Wall Jumping
            if(useWallJumpMoveStats)
            {
                Move(moveStats.wallJumpMoveAcceleration, moveStats.wallJumpMoveDeceleration, InputManager.Movement);
            }

            //Airborne
            else
            {
                Move(moveStats.airAcceleration, moveStats.airDeceleration, InputManager.Movement);
            }
            
        }

        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        // CLAMP FALL SPEED
        verticalVelocity = Mathf.Clamp(verticalVelocity, -moveStats.maxFallSpeed, 50f);

        rb.linearVelocity = new Vector3(horizontalVelocity, verticalVelocity);
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector3 moveInput)
    {
        if (Mathf.Abs(moveInput.x) >= moveStats.moveThreshold)
        {
            TurnCheck(moveInput);

            float targetVelocity = 0f;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = moveInput.x * moveStats.maxRunSpeed;
            }
            else { targetVelocity = moveInput.x * moveStats.maxWalkSpeed; }

            horizontalVelocity = Mathf.Lerp(horizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            
        }

        else if (Mathf.Abs(moveInput.x) < moveStats.moveThreshold)
        {
            horizontalVelocity = Mathf.Lerp(horizontalVelocity, 0f, deceleration);
            
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

    #region Land/Fall

    private void LandCheck()
    {
        //LANDED
        if ((isJumping || isFalling || isWallJumpFalling || isWallJumping || isWallSlideFalling || isWallSliding) && isGrounded && verticalVelocity <= 0f)
        {
            ResetJumpValues();
            StopWallSlide();
            ResetWallJumpValues();

            numberOfJumpsUsed = 0;

            verticalVelocity = Physics.gravity.y;
        }
    }

    private void Fall()
    {
        // NORMAL GRAVITY WHILE FALLING 
        if (!isGrounded && !isJumping && !isWallSliding && !isWallJumping)
        {
            if (!isFalling)
            {
                isFalling = true;
            }

            verticalVelocity += moveStats.gravity * Time.fixedDeltaTime;
        }
    }

    #endregion

    #region Jump

    private void ResetJumpValues()
    {
        isJumping = false;
        isFalling = false;
        isFastFalling = false;
        fastFallTime = 0f;
        isPastApexThreshold = false;
    }

    private void JumpChecks()
    {
        //WHEN JUMP KEY IS PRESSED
        if (InputManager.JumpWasPressed)
        {
            if(isWallSlideFalling && wallJumpPostBufferTimer >= 0) 
            { return; }
            
            else if(isWallSliding || (isTouchingWall & !isGrounded))
            { return; }

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
        else if (jumpBufferTimer > 0f && (isJumping || isWallJumping || isWallSlideFalling) && !isTouchingWall && numberOfJumpsUsed < moveStats.numberOfJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);
        }

        //AIR JUMP AFTER COYTOE TIME LAPSE
        else if (jumpBufferTimer > 0f && isFalling && !isWallSlideFalling && numberOfJumpsUsed < moveStats.numberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            isFastFalling = false;
        }

        
    }

    private void InitiateJump(int _numberOfJumpsUsed)
    {
        if (!isJumping)
        {
            isJumping = true;
        }

        ResetWallJumpValues();

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
                else if(!isFastFalling)
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

        

        
    }

    #endregion

    #region Wall Slide

    private void WallSlideCheck()
    {
        if(isTouchingWall && !isGrounded)
        {
            if(verticalVelocity < 0f && !isWallSliding)
            {
                ResetJumpValues();
                ResetWallJumpValues();

                isWallSlideFalling = false;
                isWallSliding = true;
            }

            if (moveStats.ResetJumpsOnWallSlide)
            {
                numberOfJumpsUsed = 0;
            }
        }
        else if(isWallSliding && !isTouchingWall && !isGrounded && !isWallSlideFalling)
        {
            isWallSlideFalling = true;
            StopWallSlide();
        }
        else
        {
            StopWallSlide();
        }
    }

    private void StopWallSlide()
    {
        if (isWallSliding)
        {
            numberOfJumpsUsed++;
            isWallSliding = false;
        }
    }

    private void WallSlide()
    {
        if (isWallSliding)
        {
            verticalVelocity = Mathf.Lerp(verticalVelocity, -moveStats.wallSlideSpeed, moveStats.wallSlideDecelerationSpeed * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region WallJump

    private void ResetWallJumpValues()
    {
        isWallSlideFalling = false;
        useWallJumpMoveStats = false;
        isWallJumping = false;
        isWallJumpFastFalling = false;
        isWallJumpFalling = false;
        isPastWallJumpApexThreshold = false;

        wallJumpFastFallTime = 0f;
        wallJumpTIme = 0f;
    }

    #endregion

    #region Aiming/Gun


    private void Aiming()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        gunPivot.transform.LookAt(mousePos);


        // Clamp mouse to prevent out-of-bounds errors
        mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width);
        mousePos.y = Mathf.Clamp(mousePos.y, 0, Screen.height);

        // Use the gun pivot�s Z-depth to avoid perspective distortion
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

    private void IsTouchingWall()
    {
        float originEndPoint = 0f;
        if (isFacingRight)
        {
            originEndPoint = bodyCollider.bounds.max.x;
        }
        else
        {
            originEndPoint = bodyCollider.bounds.min.x;
        }

        float adjustedHeight = bodyCollider.bounds.size.y * moveStats.wallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originEndPoint, bodyCollider.bounds.center.y);
        Vector2 boxCastSize = new Vector2(moveStats.wallDetectionRayLength,adjustedHeight);

        if(Physics.Raycast(boxCastOrigin,transform.right, moveStats.wallDetectionRayLength, moveStats.groundLayer))
        {
            isTouchingWall = true;
        }
        else
        {
            isTouchingWall= false;
        }

        #region Debug Visualization 

        if (moveStats.DebugShowWallHitBox)
        {
            Color rayColor;
            if (isTouchingWall)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(boxCastOrigin, transform.right, rayColor);
        }

        #endregion

    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
        IsTouchingWall();
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