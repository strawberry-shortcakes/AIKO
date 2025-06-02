using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeAndTeleport : MonoBehaviour
{
    public Image fadeImage; // Assign a UI Image with a black color in the Inspector
    public float fadeDuration = 1f; // Duration of the fade effect

    private void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0); // Ensure the image starts fully transparent
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure the player has the correct tag
        {
            StartCoroutine(FadeToBlackAndLoadScene());
        }
    }

    private IEnumerator FadeToBlackAndLoadScene()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            fadeImage.color = new Color(0, 0, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1); // Ensure it's fully black
        yield return new WaitForSeconds(0.5f); // Short pause before scene change

        SceneManager.LoadScene("Sliding"); // Load the target scene
    }
}

