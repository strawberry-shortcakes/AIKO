using UnityEngine;

public class AikoDash : MonoBehaviour
{
    public float dashCooldown;
    public bool canIDash = true;
    public Vector3 savedVelocity;

    void Update()
    {
        if (dashCooldown > 0)
        {
            canIDash = false;
            dashCooldown -= Time.deltaTime;
        }
        if (dashCooldown <= 0)
        {
            canIDash = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && canIDash == true)
        {
            savedVelocity = GetComponent<Rigidbody>().linearVelocity;
            GetComponent<Rigidbody>().linearVelocity = new Vector2(GetComponent<Rigidbody>().linearVelocity.x * 3f, GetComponent<Rigidbody>().linearVelocity.y);
            dashCooldown = 2;
        }
    }
}