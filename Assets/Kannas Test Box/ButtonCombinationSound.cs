using UnityEngine;

public class ButtonCombinationSound : MonoBehaviour
{
    public AudioSource audioSource;  // Assign an AudioSource in the Inspector
    private int currentStep = 0;
    private KeyCode[] sequence = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };

    void Update()
    {
        if (Input.GetKeyDown(sequence[currentStep]))
        {
            currentStep++;

            // If all keys have been pressed in order, play sound
            if (currentStep >= sequence.Length)
            {
                audioSource.Play(); // Plays the assigned sound
                currentStep = 0; // Reset sequence for future attempts
            }
        }
        else if (Input.anyKeyDown) // Reset if the wrong key is pressed
        {
            currentStep = 0;
        }
    }
}