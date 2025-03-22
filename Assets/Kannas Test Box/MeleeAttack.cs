using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    public float attackRange = 2.0f;
    public float attackAngle = 45.0f;
    public LayerMask enemyLayer;
    public float attackCooldown = 0.5f;

    private float lastAttackTime;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody is missing from the GameObject!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformMeleeAttack();
            lastAttackTime = Time.time;
        }
    }

    void PerformMeleeAttack()
    {
        // If the player is moving, use the velocity direction for the attack
        Vector3 attackDirection = rb.linearVelocity.normalized;

        if (attackDirection == Vector3.zero)
        {
            attackDirection = transform.right; // Default to the "forward" direction if not moving
        }

        // Log start and end positions of the attack testing and stuff
        Vector3 attackEnd = transform.position + attackDirection * attackRange;
        Debug.Log("Attack start: " + transform.position + ", Attack end: " + attackEnd);

        //Cone to show attack range using gizmos. keep it as a cone as the line option is too thin to see sometimes
        DrawAttackCone(attackDirection);

        // Check for collisions within the attack range
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            // Log the hit enemies
            Debug.Log("Hit " + enemy.name);
            // Example: enemy.GetComponent<Enemy>().TakeDamage(10);
        }
    }

    // Function to draw a cone in the Scene view
    void DrawAttackCone(Vector3 direction)
    {
        // Angle in radians
        float angleInRadians = attackAngle * Mathf.Deg2Rad;

        // Calculate direction vectors at the edges of the cone (left and right)
        Vector3 leftEdge = Quaternion.Euler(0, -attackAngle, 0) * direction;  // Rotate left
        Vector3 rightEdge = Quaternion.Euler(0, attackAngle, 0) * direction;  // Rotate right

        // Draw lines from the character to the edge of the cone
        Debug.DrawLine(transform.position, transform.position + leftEdge * attackRange, Color.red, 1f);
        Debug.DrawLine(transform.position, transform.position + rightEdge * attackRange, Color.red, 1f);

        // Draw the center of the cone (forward direction)
        Debug.DrawLine(transform.position, transform.position + direction * attackRange, Color.red, 1f);
    }
}