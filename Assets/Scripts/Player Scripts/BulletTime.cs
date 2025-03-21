using UnityEngine;

public class BulletTime : MonoBehaviour
{
    private float fixedDeltaTime;
    public AudioSource bulletTimeSound; // Reference to an AudioSource that plays your Bullet Time sound
    public AudioClip bulletTimeClip; // Assign your bullet time sound effect here

    void Start()
    {
        // Set the sound to loop if you want it to loop during Bullet Time
        bulletTimeSound.loop = true;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            if (Time.timeScale == 1.0f)
            {
                // Bullet Time: Slow down time and play sound at normal speed
                Time.timeScale = 0.25f;
                bulletTimeSound.clip = bulletTimeClip;
                bulletTimeSound.Play();
                bulletTimeSound.pitch = 1.0f;  // Maintain normal pitch
            }
            else
            {
                // Normal Time: Reset back to normal speed and stop sound
                Time.timeScale = 1.0f;
                bulletTimeSound.Stop();
            }

            // Adjust fixed delta time according to timescale
            Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
        }
        else if (Time.timeScale == 0.25f && !bulletTimeSound.isPlaying)
        {
            // Optional: Keep the sound playing if bullet time is still active but not repeating the key press
            bulletTimeSound.Play();
        }
    }
}