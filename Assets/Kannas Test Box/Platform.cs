using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField]
    private Transform position1, position2;
    public float _speed = 3.0f;
    private bool _switch = false;

    private void FixedUpdate()
    {
        // Move the platform between position1 and position2
        if (_switch == false)
        {
            transform.position = Vector3.MoveTowards(transform.position, position1.position,
                _speed * Time.deltaTime);
        }
        else if (_switch == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, position2.position,
                _speed * Time.deltaTime);
        }

        // Switch the direction when the platform reaches one of the positions
        if (transform.position == position1.position)
        {
            _switch = true;
        }
        else if (transform.position == position2.position)
        {
            _switch = false;
        }
    }

    // When the player enters the platform, it will move with it
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Make sure the player has the "Player" tag
        {
            // Move the player along with the platform
            collision.transform.SetParent(transform);
        }
    }

    // When the player leaves the platform, it will stop moving with it
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Remove the player from the platform's parent
            collision.transform.SetParent(null);
        }
    }
}