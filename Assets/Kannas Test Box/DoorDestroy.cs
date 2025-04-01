using UnityEngine;

public class DoorDestroy : MonoBehaviour
{
    // Define the layer index for Bullet
    private int bulletLayer = 9; // Layer 9 is for the Bullet

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with an object on the Bullet layer (Layer 9)
        if (collision.gameObject.layer == bulletLayer)
        {
            // Destroy this object (target, enemy, etc.)
            Destroy(gameObject);
        }
    }

    // Alternatively, for triggers:
    void OnTriggerEnter(Collider other)
    {
        // Check if the other object is in the Bullet layer (Layer 9)
        if (other.gameObject.layer == bulletLayer)
        {
            // Destroy this object
            Destroy(gameObject);
        }
    }
}
