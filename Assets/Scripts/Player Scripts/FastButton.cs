using UnityEngine;

public class FastButton : MonoBehaviour
{

    private float fixedDeltaTime;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.P))
        {
            if (Time.timeScale == 1.0f)
                Time.timeScale = 2f;
            else
                Time.timeScale = 1.0f;
            // Adjust fixed delta time according to timescale
            // The fixed delta time will now be 0.02 real-time seconds per frame
            Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
        }
    }
}
