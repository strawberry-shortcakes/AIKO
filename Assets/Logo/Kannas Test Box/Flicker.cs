using UnityEngine;
using System.Collections;

public class Flicker : MonoBehaviour
{
    public Light spotlight;
    public float normalIntensity = 66f;
    public float flickerIntensity = 40f;
    public float flickerDuration = 0.1f;
    public float flickerInterval = 2f;
    public int flickersPerInterval = 5;

    private float flickerTimer;
    private int flickerCount;

    void Start()
    {
        if (spotlight == null)
        {
            spotlight = GetComponent<Light>();
        }

        spotlight.intensity = normalIntensity;  
        flickerTimer = flickerInterval;
    }

    void Update()
    {
        flickerTimer -= Time.deltaTime;

        if (flickerTimer <= 0) 
        {
            StartCoroutine(FlickerRoutine());
            flickerTimer = flickerInterval;
        }
    }

    IEnumerator FlickerRoutine()
    {
        // Run multiple flickers in one cycle
        for (int i = 0; i < flickersPerInterval; i++)
        {
            // Flicker on (dim intensity)
            spotlight.intensity = flickerIntensity;

            // Wait for the flicker duration
            yield return new WaitForSeconds(flickerDuration);

            // Flicker off (return to normal intensity)
            spotlight.intensity = normalIntensity;

            // Wait for a small delay before next flicker
            yield return new WaitForSeconds(flickerDuration / 2);  // Half of the flicker duration for delay
        }
    }
}