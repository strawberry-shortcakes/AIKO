using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections;

public class DeathEffect : MonoBehaviour
{
    public RectTransform crtOverlay;       
    public Image crtBackground;           
    public float collapseDuration = 1f;
    public float delayBeforeRespawn = 1.5f;

    private Vector2 originalSize;
    private bool isGameOver = false;
    private bool canClickToRestart = false;

    void Start()
    {
        originalSize = crtOverlay.sizeDelta;

        if (crtOverlay != null)
            crtOverlay.gameObject.SetActive(false);

        if (crtBackground != null)
        {
            crtBackground.color = new Color(0f, 0f, 0f, 1f); 
            crtBackground.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (canClickToRestart && Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (crtBackground != null)
            crtBackground.gameObject.SetActive(true);

        if (crtOverlay != null)
        {
            crtOverlay.gameObject.SetActive(true);
            StartCoroutine(CRTCollapse());
        }
    }

    IEnumerator CRTCollapse()
    {
        float time = 0f;
        Vector2 startSize = originalSize;
        Vector2 endSize = new Vector2(originalSize.x, 2f);

        Image image = crtOverlay.GetComponent<Image>();
        Color originalColor = image.color;

        while (time < collapseDuration)
        {
            float t = time / collapseDuration;
            crtOverlay.sizeDelta = Vector2.Lerp(startSize, endSize, t);
            image.color = Color.Lerp(originalColor, Color.white, t);
            time += Time.deltaTime;
            yield return null;
        }

        crtOverlay.sizeDelta = endSize;
        image.color = Color.white;

        
        yield return new WaitForSeconds(0.2f);
        float fadeTime = 0f;
        while (fadeTime < 0.5f)
        {
            image.color = Color.Lerp(Color.white, Color.black, fadeTime / 0.5f);
            fadeTime += Time.deltaTime;
            yield return null;
        }

        image.color = Color.black;

        
        yield return new WaitForSeconds(delayBeforeRespawn);
        canClickToRestart = true;
    }
}