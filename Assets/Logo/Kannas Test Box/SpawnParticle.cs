using UnityEngine;
using System.Collections;
public class ParticleOnceOnStart : MonoBehaviour
{
    public GameObject particleSystemPrefab;  // Reference to the Particle System prefab in the scene
    private ParticleSystem particleSystem;   // Reference to the actual Particle System component

    void Start()
    {
        if (particleSystemPrefab != null)
        {
            // Instantiate the particle system prefab in the scene
            GameObject particleInstance = Instantiate(particleSystemPrefab, transform.position, Quaternion.identity);

            // Get the ParticleSystem component from the instantiated object
            particleSystem = particleInstance.GetComponent<ParticleSystem>();

            if (particleSystem != null)
            {
                // Make sure the particle system is not set to loop
                var mainModule = particleSystem.main;
                mainModule.loop = false;  // Disable looping

                // Play the particle system once on startup
                particleSystem.Play();

                // Stop the particle system after its duration and then deactivate the object
                StartCoroutine(StopAndDeactivate(particleInstance));
            }
            else
            {
                Debug.LogWarning("No ParticleSystem component found on the prefab.");
            }
        }
        else
        {
            Debug.LogWarning("No Particle System prefab assigned.");
        }
    }

    private IEnumerator StopAndDeactivate(GameObject particleInstance)
    {
        // Wait for the particle system's duration to finish
        yield return new WaitForSeconds(particleSystem.main.duration);

        // Stop the particle system after it finishes
        particleSystem.Stop();

        // Deactivate the particle system GameObject to prevent it from doing anything else
        particleInstance.SetActive(false);
    }
}