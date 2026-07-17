using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controls the splash screen sequence, showing logos with fade in/out effects before transitioning to the next scene.
/// </summary>
public class SplashController : MonoBehaviour
{
    public GameObject polimiLogo;
    public GameObject studioLogo;

    public float fadeTime = 1f;
    public float showTime = 2f;
    public string nextScene = "BootScene";
    
    void Start()
    {
        StartCoroutine(PlaySplash());
    }

    /// <summary>
    /// Plays the splash screen sequence by showing each logo with fade in/out effects, then loads the next scene.
    /// </summary>
    /// <returns>IEnumerator for the splash screen sequence.</returns>
    IEnumerator PlaySplash()
    {
        yield return StartCoroutine(ShowLogo(polimiLogo));
        yield return StartCoroutine(ShowLogo(studioLogo));

        SceneManager.LoadScene(nextScene);
    }
    
    /// <summary>
    /// Shows a logo with fade in, waits for a specified time, then fades it out.
    /// </summary>
    /// <param name="logo">The sprite to be displayed as the logo.</param>
    /// <returns>IEnumerator for showing the logo with fade effects.</returns>
    IEnumerator ShowLogo(GameObject logo)
    {
        CanvasGroup canvasGroup = logo.GetComponent<CanvasGroup>();

        logo.SetActive(true);
        canvasGroup.alpha = 0;

        // Fade In
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            canvasGroup.alpha = t / fadeTime;
            yield return null;
        }

        canvasGroup.alpha = 1;

        yield return new WaitForSeconds(showTime);

        // Fade Out
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            canvasGroup.alpha = 1 - (t / fadeTime);
            yield return null;
        }

        canvasGroup.alpha = 0;
        logo.SetActive(false);
    }
}