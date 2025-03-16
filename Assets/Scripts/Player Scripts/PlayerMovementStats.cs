using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Player Movement")]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Walk")]
    [Range(0f, 1f)] public float moveThreshold = 0.25f;
    [Range(1f, 100f)] public float maxWalkSpeed = 12.5f;
    [Range(0.25f, 50f)] public float groundAcceleration = 5f;
    [Range(0.25f, 50f)] public float groundDeceleration = 20f;
    [Range(0.25f, 50f)] public float airAcceleration = 5f;
    [Range(0.25f, 50f)] public float airDeceleration = 5f;
    [Range(0.25f, 50f)] public float wallJumpMoveAcceleration = 5;
    [Range(0.25f, 50f)] public float wallJumpMoveDeceleration = 5;

    [Header("Run")]
    [Range(1f, 100f)] public float maxRunSpeed = 20f;

    [Header("Ground/Collision Checks")]
    public LayerMask groundLayer;
    public float groundDetectionRayLength = 0.02f;
    public float headDetectionRayLength = 0.02f;
    [Range(0f, 1f)] public float headWidth = 0.75f;
    public float wallDetectionRayLength = 0.125f;
    [Range(0.01f, 2f)] public float wallDetectionRayHeightMultiplier = 0.9f;

    [Header("Jump")]
    public float jumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float jumpHeightCompensationFactor = 1.054f;
    public float timeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float gravityOnReleaseMultiplier = 2f;
    public float maxFallSpeed = 26f;
    [Range(1, 5)] public int numberOfJumpsAllowed = 2;

    [Header("Reset Jump Options")]
    public bool ResetJumpsOnWallSlide = true;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float timeForUpwardsCancel;

    [Header("Jump Apex")]
    [Range(0.05f, 1f)] public float apexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float apexHangTime = 0.75f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float jumpBufferTime = 0.125f;

    [Header("Jump Coyote Time")]
    [Range(0, 1f)] public float jumpCoyoteTime = 0.1f;

    [Header("Wall Slide")]
    [Min(0.01f)] public float wallSlideSpeed = 5f;
    [Range(0.25f, 50f)] public float wallSlideDecelerationSpeed = 50f;

    [Header("Wall Jump")]
    public Vector2 wallJumpDirection = new Vector2(-20f, 6.5f);
    [Range(0f, 1f)] public float wallJumpPostBufferTime = 0.125f;
    [Range(0.01f, 5f)] public float wallJumpGravityOnReleaseMultiplier = 1f; 

    [Header("Jump Visualization Tool")]
    public bool showWalkJumpArc = false;
    public bool showRunJumpArc = false;
    public bool stopOnCollision = true;
    public bool drawRight = true;
    [Range(5, 100)] public int arcResolution = 20;
    [Range(0, 500)] public int visualizationSteps = 90;

    [Header("Debug Options")]
    public bool DebugShowIsGroundedBox = true;
    public bool DebugShowIsHeadBumpedBox = true;
    public bool DebugShowWallHitBox = true;

    //Jump
    public float gravity { get; private set; }
    public float initialJumpVelocity { get; private set; }
    public float adjustedJumpHeight { get; private set; }

    //Wall Jump
    public float wallJumpGravity { get; private set; }
    public float initialWallJumpVelocity { get; private set; }
    public float adjustedWallJumpHeight { get; private set; }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnabled()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        //Jump
        adjustedJumpHeight = jumpHeight * jumpHeightCompensationFactor;
        gravity = -(2f * adjustedJumpHeight) / Mathf.Pow(timeTillJumpApex, 2f);
        initialJumpVelocity = Mathf.Abs(gravity) * timeTillJumpApex;

        //Wall Jump
        adjustedWallJumpHeight = wallJumpDirection.y * jumpHeightCompensationFactor;
        wallJumpGravity = -(2f * adjustedWallJumpHeight) / Mathf.Pow(timeTillJumpApex, 2f);
        initialWallJumpVelocity = Mathf.Abs(wallJumpGravity) * timeTillJumpApex;
    }
}
