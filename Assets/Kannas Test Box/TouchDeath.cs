using UnityEngine;

public class TouchDeath : MonoBehaviour
{
    public float fallSpeed = 20f; // Speed at which the object moves down

    private bool shouldMoveDown = false;

    void Update()
    {
        if (shouldMoveDown)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Trigger the downward movement when a collision occurs
        shouldMoveDown = true;
    }
}
