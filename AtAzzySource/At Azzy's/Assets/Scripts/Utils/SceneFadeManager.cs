using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance { get; private set; }

    [SerializeField] private float fadeDuration = 0.5f;
    
    private Image fadeImage;
    private Canvas fadeCanvas;
    private bool _isTransitioning = false;
    public SceneFadeOnLoad sceneFadeOnLoadPublic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        CreateFadeCanvas();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void CreateFadeCanvas()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.overrideSorting = true;
        fadeCanvas.sortingOrder = 30000;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = true;

        // Stretch to fill screen
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        SceneFadeOnLoad sceneFadeOnLoad = canvasObj.AddComponent<SceneFadeOnLoad>();
        sceneFadeOnLoad.SetFadeImage(fadeImage);
        sceneFadeOnLoadPublic = sceneFadeOnLoad;
        
        DontDestroyOnLoad(canvasObj);
        
        fadeCanvas.enabled = false;
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (fadeCanvas == null || fadeImage == null)
            yield break;

        fadeCanvas.enabled = true;

        // Fade out to black
        yield return StartCoroutine(Fade(0f, 1f));
        
        // Mark that we're waiting to perform fade-in after load
        _isTransitioning = true;

        // Start async load and wait until done
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        // Allow scene activation immediately (default), just wait until it finishes
        while (!op.isDone)
            yield return null;
        
        yield return null;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_isTransitioning)
            return;

        _isTransitioning = false; // Reset transitioning flag
        
        SceneFadeOnLoad sceneFade = FindObjectOfType<SceneFadeOnLoad>();
        if (sceneFade != null)
        {
            StartCoroutine(DoSceneFadeIn(sceneFade)); // Delegate fade-in to scene component
        }
        else
        {
            StartCoroutine(FallbackFadeIn()); // Fallback to persistent canvas fade-in
        }
    }
    
    private IEnumerator DoSceneFadeIn(SceneFadeOnLoad sceneFade)
    {
        yield return StartCoroutine(sceneFade.FadeIn(fadeDuration));
        fadeCanvas.enabled = false;
        _isTransitioning = false;
    }
    
    private IEnumerator FallbackFadeIn()
    {
        yield return StartCoroutine(Fade(1f, 0f));
        fadeCanvas.enabled = false;
        _isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }
}