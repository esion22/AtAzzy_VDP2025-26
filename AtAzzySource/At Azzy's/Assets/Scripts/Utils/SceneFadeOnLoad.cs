using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeOnLoad : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    public bool isFading = false;

    
    public void SetFadeImage(Image image)
    {
        fadeImage = image;
    }

    public IEnumerator FadeIn(float duration)
    {
        isFading = true;
        if (fadeImage == null)
        {
            yield break;
        }
        
        fadeImage.raycastTarget = true; // Ensure the image blocks input during fade-in
        fadeImage.color = new Color(0f, 0f, 0f, 1f); // Set to fully opaque black

        float elapsed = 0f;
        Color color = fadeImage.color;
        float startAlpha = color.a;
        

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            fadeImage.color = color;
            
            Canvas.ForceUpdateCanvases();
            
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.raycastTarget = false;
        isFading = false;
    }
}