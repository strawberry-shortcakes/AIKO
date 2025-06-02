using UnityEngine;

public class PlayAudioOnTrigger : MonoBehaviour
{
    public AudioClip audioClip;  // The audio clip to play
    private AudioSource audioSource;  // The AudioSource component to play the sound
    private bool hasPlayed = false;  // Flag to check if the audio has been played

    void Start()
    {
        // Get the AudioSource component on the GameObject
        audioSource = GetComponent<AudioSource>();

        // Ensure the AudioSource component is assigned
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource component found on this GameObject.");
        }

        // Log if the AudioClip is assigned or not
        if (audioClip == null)
        {
            Debug.LogError("No AudioClip assigned!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the player (or any specific object) enters the trigger
        if (other.CompareTag("Player") && !hasPlayed)  // Ensure the trigger is activated by the player and not yet played
        {
            Debug.Log("Player entered trigger zone");

            // Play the audio clip when the player enters the trigger zone
            if (audioSource != null && audioClip != null)
            {
                audioSource.PlayOneShot(audioClip);  // Play the sound
                hasPlayed = true;  // Set the flag to prevent further playback
                Debug.Log("Audio played once");
            }
            else
            {
                Debug.LogError("AudioSource or AudioClip is missing.");
            }
        }
    }

    // Optionally, reset the flag if you want the sound to be playable again after some condition (e.g., re-entering the area).
    public void ResetAudioFlag()
    {
        hasPlayed = false;
    }
}