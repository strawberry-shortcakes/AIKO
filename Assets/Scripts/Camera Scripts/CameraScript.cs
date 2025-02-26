using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public PlayerMovement pm;
    public float cameraRotation = 8;
    public float rotationSpeed = 20;

    public float cameraPosition = 3;
    public float positionSpeed = 20;

    public Vector3 currentEulerAngles;
    public Quaternion currentRotation;
    public Vector3 currentPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pm = GameObject.Find("Player").GetComponent<PlayerMovement>();
        
    }

    // Update is called once per frame
    void Update()
    {
        RotateCamera();

    }

    // Rotates the camera depending on which way the player is going
    void RotateCamera()
    {
        if (pm.horizontal == 1)
        {
            currentEulerAngles += new Vector3(0, cameraRotation * pm.horizontal, 0) * Time.deltaTime * rotationSpeed;  // Modifying the Vector3 based on input, multiplied by speed and time 
            currentRotation.eulerAngles = currentEulerAngles;                                                          // Sets the Vector3 into Quaternion format  
            transform.rotation = currentRotation;                                                                      // Applies the Quaternion to the Camera and rotates it

            // Stops the Camera from continuing to spin after desired rotation is set
            if (currentEulerAngles.y >= 8)
            {
                currentEulerAngles.y = 8;
                transform.rotation = Quaternion.Euler(0, 8, 0);
            }
        }

        if (pm.horizontal == -1)
        {
            currentEulerAngles += new Vector3(0, cameraRotation * pm.horizontal, 0) * Time.deltaTime * rotationSpeed;
            currentRotation.eulerAngles = currentEulerAngles;
            transform.rotation = currentRotation;

            if (currentEulerAngles.y <= -8)
            {
                currentEulerAngles.y = -8;
                transform.rotation = Quaternion.Euler(0, -8, 0);

            }
        }
    }

    
}
