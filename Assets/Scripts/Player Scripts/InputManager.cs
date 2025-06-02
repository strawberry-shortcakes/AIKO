using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;
    public static Vector3 MousePos;
    
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    
    public static bool RunIsHeld;
    
    public static bool ShootWasPressed;

    public static bool BulletTimeWasPressed;
    public static bool BulletTimeIsHeld;
    public static bool BulletTimeWasReleased;

    public static bool MeleeAttackWasPressed;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction aimAction;
    private InputAction shootAction;
    private InputAction bulletTimeAction;
    private InputAction meleeAttackAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        moveAction = PlayerInput.actions["Move"];
        jumpAction = PlayerInput.actions["Jump"];
        runAction = PlayerInput.actions["Run"];
        aimAction = PlayerInput.actions["Aiming"];
        shootAction = PlayerInput.actions["Shoot"];
        bulletTimeAction = PlayerInput.actions["Bullet Time"];
        meleeAttackAction = PlayerInput.actions["Melee Attack"];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateControls();
    }

    void UpdateControls()
    {
        Movement = moveAction.ReadValue<Vector2>();
        MousePos = aimAction.ReadValue<Vector2>();

        JumpWasPressed = jumpAction.WasPressedThisFrame();
        JumpIsHeld = jumpAction.IsPressed();
        JumpWasReleased = jumpAction.WasReleasedThisFrame();

        RunIsHeld = runAction.IsPressed();

        ShootWasPressed = shootAction.IsPressed();

        BulletTimeWasPressed = bulletTimeAction.WasPressedThisFrame();
        BulletTimeIsHeld = bulletTimeAction.IsPressed();
        BulletTimeWasReleased = bulletTimeAction.WasReleasedThisFrame();

        MeleeAttackWasPressed = meleeAttackAction.WasPressedThisFrame();
    }
}
