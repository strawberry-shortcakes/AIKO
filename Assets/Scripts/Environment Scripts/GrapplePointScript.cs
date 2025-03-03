using Unity.VisualScripting;
using UnityEngine;

public class GrapplePointScript : MonoBehaviour
{
    public Transform grapplePosition;
    public PlayerMovement ps;
    public LayerMask playerLayer;
         

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grapplePosition = this.gameObject.transform;
        ps = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerCheck();
    }

    public void PlayerCheck()
    {
       if(Physics.CheckSphere(new Vector3(grapplePosition.transform.position.x, grapplePosition.transform.position.y), 10f, playerLayer))
        {
            ps.gps = this;
            ps.isNearGrapple = true;
        }
        else
        {
            ps.isNearGrapple = false;
            ps.gps = null;
        }
    }
}
