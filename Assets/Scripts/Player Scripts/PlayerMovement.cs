
using SpriteShadersUltimate.Demo;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

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

    private Animator animator;
    private Rigidbody rb;

    //Movement Variables
    public float horizontalVelocity { get; private set; } 
    public bool isFacingRight { get; private set; }

    //Collision Check Variables 
    private RaycastHit groundHit;
    private RaycastHit headHit;
    private RaycastHit lastWallHit;
    private RaycastHit wallHit;
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
    private float wallJumpTime;
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
    private float rateOfFire = 0.3f;
    private float bulletSpeed = 30f;

    //Melee Variable
    private float lastAttackTime;

    //Bullet Time Variable
    private bool bulletTimeActive;
    private bool bulletTimeReady = true;
    public float bulletTimeTimer;

    //public Image bulletTimeMeter;

    //Health Variable
    public int maxHealth = 5;
    public int health;
    public bool isDead = false;
    private DeathEffect crtEffect;

    private void Awake()
    {
        isFacingRight = true;
        rb = GetComponent<Rigidbody>();

        
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        gunPivot.transform.position = transform.position;
        health = maxHealth;
        bulletTimeTimer = moveStats.bulletTimeMax;
        animator = GetComponent<Animator>();
        crtEffect = FindObjectOfType<DeathEffect>();
    }

    private void Update()
    {
        if(!isDead)
        {
            CountTimers();
            JumpChecks();
            LandCheck();

            WallSlideCheck();
            WallJumpCheck();
            Aiming();
            Shoot();
            BulletTime();
            Animations(InputManager.Movement);
            PerformMeleeAttack();

        }
        else
        {
            return;
        }
        

        
    }

    private void FixedUpdate()
    {

        if (!isDead)
        {
            CollisionChecks();
            Jump();
            Fall();
            WallSlide();
            WallJump();
            
        }
        else { return; }
        

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
        wallJumpTime = 0f;
    }

    private void WallJumpCheck()
    {
        //if (lastWallHit.collider == null)
        //{
        //    Debug.LogError("WallJumpCheck: lastWallHit is NULL!");
        //}
        //else
        //{
        //    Debug.Log($"WallJumpCheck: lastWallHit.collider = {lastWallHit.collider.name}");
        //}

        if (ShouldApplyPostWallJumpBuffer())
        {
            wallJumpPostBufferTimer = moveStats.wallJumpPostBufferTime;
        }

        //Wall Jump Fast Falling
        if (InputManager.JumpWasReleased && !isWallSliding && !isTouchingWall && isWallJumping)
        {
            if (verticalVelocity > 0f)
            {
                if (isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isWallJumpFastFalling = true;
                    wallJumpFastFallTime = moveStats.timeForUpwardsCancel;

                    verticalVelocity = 0f;
                }
                else
                {
                    isWallJumpFastFalling = true;
                    wallJumpFastFallReleaseSpeed = verticalVelocity;
                }
            }
        }

        //Actual Jump With Post Wall Jump Buffer Time
        if (InputManager.JumpWasPressed && wallJumpPostBufferTimer > 0f)
        {
            InitiateWallJump();
        }
    }

    private void InitiateWallJump()
    {
        

        if (!isWallJumping)
        {
            isWallJumping = true;
            useWallJumpMoveStats = true;
        }

        StopWallSlide();
        ResetJumpValues();
        wallJumpTime = 0f;

        verticalVelocity = moveStats.initialWallJumpVelocity;

        Vector2 hitPoint = lastWallHit.normal;
        int dirMultiplier = (hitPoint.x > 0) ? 1 : -1;

        

        horizontalVelocity = moveStats.wallJumpDirection.x * dirMultiplier;
        Debug.Log($"Wall Jump! Horizontal Velocity: {horizontalVelocity}, Direction Multiplier: {dirMultiplier}");


        if (dirMultiplier == 0)
        {
            Debug.LogWarning("dirMultiplier is 0, horizontal velocity will not be set correctly");
        }
        if (horizontalVelocity == 0)
        {
            Debug.LogError("Horizontal velocity is 0. Check dirMultiplier or wall detection.");
        }
        Debug.Log($"Wall Jump! Horizontal Velocity: {horizontalVelocity}, Vertical Velocity: {verticalVelocity}");
    }

    private void WallJump()
    {
        //APPLY WALL JUMP GRAVITY
        if (isWallJumping)
        {
            //TIME TO TAKE OVER MOVEMENT CONTROLS WHILE WALL JUMPING 
            wallJumpTime += Time.fixedDeltaTime;
            if (wallJumpTime >= moveStats.timeTillJumpApex)
            {
                useWallJumpMoveStats = false;
            }

            //HIT HEAD
            if (bumpedHead)
            {
                isWallJumpFastFalling = true;
                useWallJumpMoveStats = false;
            }

            //GRAVITY IN ASCENDING
            if (verticalVelocity >= 0)
            {
                //APEX CONTROLS
                wallJumpApexPoint = Mathf.InverseLerp(moveStats.wallJumpDirection.y, 0f, verticalVelocity);

                if (wallJumpApexPoint > moveStats.apexThreshold)
                {
                    if (!isPastWallJumpApexThreshold)
                    {
                        isPastWallJumpApexThreshold = true;
                        timePastWallJumpApexThreshold = 0f;                        
                    }

                    if (isPastWallJumpApexThreshold)
                    {
                        timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                        if (timePastWallJumpApexThreshold < moveStats.apexHangTime)
                        {
                            verticalVelocity = 0f;
                        }
                        else
                        {
                            verticalVelocity = -0.01f;
                        }
                    }
                }

                //GRAVITY IN ASCENDING BUT NOT PAST APEX THRESHOLD
                else if (!isWallJumpFastFalling)
                {
                    verticalVelocity += moveStats.wallJumpGravity * Time.fixedDeltaTime;
                    if (isPastWallJumpApexThreshold)
                    {
                        isPastWallJumpApexThreshold = false;
                    }
                }
            }

            //GRAVITY ON DESCENDING 
            else if (!isWallJumpFastFalling)
            {
                verticalVelocity += moveStats.wallJumpGravity * Time.fixedDeltaTime;
            }
            else if (verticalVelocity < 0f)
            {
                if (!isWallJumpFalling)
                {
                    isWallJumpFalling = true;
                }
            }
        }

        //HANDLE WALLJUMP CUT
        if (isWallJumpFastFalling)
        {
            if (wallJumpFastFallTime >= moveStats.timeForUpwardsCancel)
            {
                verticalVelocity += moveStats.wallJumpGravity * moveStats.wallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (wallJumpFastFallTime < moveStats.timeForUpwardsCancel)
            {
                verticalVelocity = Mathf.Lerp(wallJumpFastFallReleaseSpeed, 0f, (wallJumpFastFallTime / moveStats.timeForUpwardsCancel));
            }

            wallJumpFastFallTime += Time.fixedDeltaTime;
        }


    }

    private bool ShouldApplyPostWallJumpBuffer()
    {
        if (!isGrounded && (isTouchingWall || isWallSliding))
        {
            return true;
        }
        else { return false; }
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

    #region Melee Attack

    private void PerformMeleeAttack()
    {
        if(InputManager.MeleeAttackWasPressed && Time.time >= lastAttackTime + moveStats.attackCooldown)
        {
            lastAttackTime = Time.time;

            Vector3 attackDirection = rb.linearVelocity.normalized;
            if (attackDirection == Vector3.zero)
            {
                attackDirection = transform.right;
            }

            Vector3 attackEnd = transform.position + attackDirection * moveStats.attackRange;
            DrawAttackCone(attackDirection);

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, moveStats.attackRange, moveStats.enemyLayer);
            foreach (Collider enemy in hitEnemies)
            {
                Debug.Log("Hit " + enemy.name);
            }
        }
        

    }

    private void DrawAttackCone(Vector3 direction)
    {
        float angleInRadians = moveStats.attackAngle * Mathf.Deg2Rad;

        Vector3 leftEdge = Quaternion.Euler(0 ,- moveStats.attackAngle, 0) * direction;
        Vector3 rightEdge = Quaternion.Euler(0, moveStats.attackAngle, 0) * direction;

        Debug.DrawLine(transform.position, transform.position + leftEdge * moveStats.attackRange, Color.red, 1f);
        Debug.DrawLine(transform.position, transform.position + rightEdge * moveStats.attackRange, Color.red, 1f);
        Debug.DrawLine(transform.position, transform.position + direction * moveStats.attackRange, Color.red, 1f);
    }

    #endregion

    #region Bullet Time

    public void BulletTime()
    {
        if (InputManager.BulletTimeIsHeld && (bulletTimeTimer <= moveStats.bulletTimeMax || bulletTimeTimer > 0) && bulletTimeReady)
        {
            bulletTimeActive = true;
            bulletTimeTimer -= Time.fixedDeltaTime * 0.25f;
            Time.timeScale = 0.50f;

           if(bulletTimeTimer <= 0)
           {
                bulletTimeTimer = 0;
                bulletTimeReady = false;
           }
        }
        else if (InputManager.BulletTimeWasReleased || bulletTimeTimer <= 0)
        {
            bulletTimeReady = false;
            bulletTimeActive = false;
            Time.timeScale = 1f; 
        }

        if (bulletTimeTimer < moveStats.bulletTimeMax && !bulletTimeActive && !bulletTimeReady)
        {
            bulletTimeTimer += Time.fixedDeltaTime * 0.05f;

            if(bulletTimeTimer >= moveStats.bulletTimeMax)
            {
                bulletTimeReady = true;
                bulletTimeTimer = moveStats.bulletTimeMax;
            }
        }

        //bulletTimeMeter.fillAmount = bulletTimeTimer / moveStats.bulletTimeMax;
    }

    #endregion

    #region Health System 

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision Detected");

        if (collision.gameObject.tag == "Enemy")
        {
            if(health >= 1)
            {
                health--;
                Debug.Log("Taken Damage");
            }

            if (health == 0)
            {
                isDead = true;
                Debug.Log("Dead!");

                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true; // Optional: if you want them to stop moving completely


                if (crtEffect != null)
                {
                    crtEffect.TriggerGameOver();
                }
                else
                {
                    Debug.LogWarning("CRTGameOver not found in scene.");
                }

            }
        }
    }
    

    

    #endregion

    #region Collision Checks

    private void BumpedHead()
    {
        Vector3 boxCastOrigins = new Vector3(feetCollider.bounds.center.x, bodyCollider.bounds.max.y);  // Adjusted to use max.y for the head position
        Vector3 boxCastSize = new Vector3(feetCollider.bounds.size.x * moveStats.headWidth, moveStats.headDetectionRayLength);

        // Use OverlapBox for detecting collisions in the head area
        Collider[] colliders = Physics.OverlapBox(boxCastOrigins, boxCastSize / 2, Quaternion.identity, moveStats.groundLayer);

        if (colliders.Length > 0)
        {
            bumpedHead = true;
            Debug.Log("Head bumped!");
        }
        else
        {
            bumpedHead = false;
        }

        #region Debug Visualization
        if (moveStats.DebugShowIsHeadBumpedBox)
        {
            Color rayColor = bumpedHead ? Color.green : Color.red;

            // Visualize the box for head bump detection
            Debug.DrawRay(boxCastOrigins, Vector3.up * moveStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector3(boxCastOrigins.x - boxCastSize.x / 2, boxCastOrigins.y), Vector3.up * moveStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector3(boxCastOrigins.x + boxCastSize.x / 2, boxCastOrigins.y), Vector3.up * moveStats.headDetectionRayLength, rayColor);
        }
        #endregion
    }
        private void IsGrounded()
    {
        Vector3 boxCastOrigins = new Vector3(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
        Vector3 boxCastSize = new Vector3(feetCollider.bounds.size.x, moveStats.groundDetectionRayLength);

        Collider[] colliders = Physics.OverlapBox(boxCastOrigins, boxCastSize / 2, Quaternion.identity, moveStats.groundLayer);
        if (colliders.Length > 0)
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
        float originOffset = 0.01f;  // Small offset to avoid colliders clipping the wall
        float originEndPoint = isFacingRight ? bodyCollider.bounds.max.x - originOffset : bodyCollider.bounds.min.x + originOffset;

        float adjustedHeight = bodyCollider.bounds.size.y * moveStats.wallDetectionRayHeightMultiplier;

        // Create the origin point for the box cast
        Vector2 boxCastOrigin = new Vector2(originEndPoint, bodyCollider.bounds.center.y);
        Vector2 boxCastSize = new Vector2(moveStats.wallDetectionRayLength, adjustedHeight);

        // Use OverlapBox for detecting collisions in the area of the box
        Collider[] colliders = Physics.OverlapBox(boxCastOrigin, boxCastSize / 2, Quaternion.identity, moveStats.groundLayer);

        if (colliders.Length > 0)
        {
            isTouchingWall = true;

            RaycastHit hit;
            if (Physics.Raycast(boxCastOrigin, transform.right, out hit, moveStats.wallDetectionRayLength, moveStats.groundLayer))
            {
                lastWallHit = hit;  // Set the RaycastHit directly from the raycast
                //Debug.Log($"Wall detected! Collider: {hit.collider.name}");
            }
        }
        else
        {
            isTouchingWall = false;
            //Debug.LogWarning("No wall detected.");
        }

        #region Debug Visualization 
        if (moveStats.DebugShowWallHitBox)
        {
            Color rayColor = isTouchingWall ? Color.green : Color.red;

            // Visualize the box using Debug.DrawLine
            Vector2 boxBottomLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxBottomRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxTopLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);
            Vector2 boxTopRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);

            Debug.DrawLine(boxBottomLeft, boxBottomRight, rayColor);
            Debug.DrawLine(boxBottomRight, boxTopRight, rayColor);
            Debug.DrawLine(boxTopRight, boxTopLeft, rayColor);
            Debug.DrawLine(boxTopLeft, boxBottomLeft, rayColor);
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

        if (!ShouldApplyPostWallJumpBuffer())
        {
            wallJumpPostBufferTimer -= Time.deltaTime;
        }
    }

    #endregion

    #region Animations
    private void Animations(Vector3 moveInput)
    {
        // Run
        if (Mathf.Abs(moveInput.x) >= moveStats.moveThreshold)
        {
            animator.SetFloat("Speed", 0.5f);
            // Sprint
            if (InputManager.RunIsHeld)
            {
                // do sprint animation
                animator.SetFloat("Speed", 1f);
            }
            else 
            {
             
            }

            

        }
        // If moveinput below threshold do idle 
        if (Mathf.Abs(moveInput.x) < moveStats.moveThreshold)
        {
            // idle
            animator.SetFloat("Speed", 0f);
        }
    }
    

    #endregion
}