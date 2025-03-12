using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Player Movement")]
public class PlayerMovementStats : MonoBehaviour
{
    [Header("Walk")]
    [Range(1f, 100f)] public float maxWalkSpeed = 12.5f;
    [Range(0.25f, 50f)] public float groundAcceleration = 5f;
    [Range(0.25f, 50f)] public float groundDeceleration = 20f;
    [Range(0.25f, 50f)] public float airAcceleration = 5f;
    [Range(0.25f, 50f)] public float airDeceleration = 5f;

    [Header("Run")]
    [Range(1f, 100f)] public float maxRunSpeed = 20f;

    [Header("Ground/Collision Checks")]
    public LayerMask groundLayer;
    public float groundDetectionRayLength = 0.02f;
    public float headDectectionRayLength = 0.02f;
    [Range(0f, 1f)] public float headWidth = 0.75f;
}
