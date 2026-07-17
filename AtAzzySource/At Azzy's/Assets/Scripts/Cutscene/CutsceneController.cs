using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private int secondsDelay = 55;
    [SerializeField] private string targetScene = "FarmScene";

    private Coroutine _timerCoroutine;
    
    void Start()
    {
        _timerCoroutine = StartCoroutine(TimerAndLoad(secondsDelay, targetScene));
    }
    
    private IEnumerator TimerAndLoad(int seconds, string sceneName)
    {
        yield return new WaitForSeconds(seconds);

        // When timer completes, attempt to fade via SceneFadeManager if available
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeToScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
