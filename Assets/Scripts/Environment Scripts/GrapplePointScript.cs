using Unity.VisualScripting;
using UnityEngine;

public class GrapplePointScript : MonoBehaviour
{
    public Vector3 grapplePosition;
    public PlayerMovement ps;
    public LayerMask playerLayer;
         

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grapplePosition = gameObject.transform.position;
        ps = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerCheck();
    }

    public void PlayerCheck()
    {
       if(IsPlayerNearGrapple())
        {
            ps.grapplePoint = this.gameObject;
            ps.gps = this;
            ps.endMarker = this.grapplePosition;
            ps.isNearGrapple = true;
        }
       else
        {
            ps.isNearGrapple = false;
            ps.endMarker = Vector3.zero;
            ps.gps = null;
            ps.grapplePoint = null;
            
        }
    }

    public bool IsPlayerNearGrapple()
    {
        return Physics.CheckSphere(new Vector3(grapplePosition.x, grapplePosition.y), 10f, playerLayer, QueryTriggerInteraction.Collide);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(grapplePosition, 10);
    }
}
