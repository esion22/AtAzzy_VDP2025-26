using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Global pause menu manager (lives in BootScene)
/// Controls pause UI and HUD visibility safely across scenes
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    [Header("Pause UI")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject creditsMenuUI;


    private CanvasGroup hudCanvasGroup;
    public static bool isPaused = false;
    private const string SALES_DIALOGUE = "SalesManDialogue";
    private float previousTimeScale = 1f;
    
    private GameFinaleFarmManager _gameFinaleFarmManager;
    private string _currentScene;
    [SerializeField] private GameObject unlockFinaleButton;




    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Listen for scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // BootScene does not allowed pause
        if (IsBootScene())
            return;

        if (_gameFinaleFarmManager == null)
        {
            _gameFinaleFarmManager = FindObjectOfType<GameFinaleFarmManager>();
        }
        if (Input.GetButtonDown("OpenMenu"))
        {
            if (settingsMenuUI != null && settingsMenuUI.activeSelf)
            {
                CloseSettings();
            }
            else if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    
    private void UpdateUnlockFinaleButtonState()
    {
        // Only update when pause menu is active
        if (!pauseMenuUI.activeSelf || unlockFinaleButton == null)
        {
            return;
        }

        string activeScene = SceneManager.GetActiveScene().name;
        _currentScene = activeScene;

        if (_currentScene == "FarmScene")
        {
            unlockFinaleButton.SetActive(true);
        }
        else
        {
            unlockFinaleButton.SetActive(false);
        }
        // if the player has completed the finale, hide the button regardless of scene
        if (PlayerPrefs.GetInt("FarmFinaleCompleted", 0) == 1)
        {
            unlockFinaleButton.SetActive(false);
        }
    }

    #endregion

    #region Scene / HUD Binding

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindHUD();
    }

    /// <summary>
    /// Finds the HUD CanvasGroup in the current scene
    /// </summary>
    private void BindHUD()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string hudName = null;

        if (sceneName == "FarmScene")
        {
            hudName = "FarmHUD";
        }
        else if (sceneName == "TavernScene")
        {
            hudName = "TavernHUD";
        }

        if (hudName == null)
        {
            hudCanvasGroup = null;
            return;
        }

        GameObject hud = GameObject.Find(hudName);

        if (hud == null)
        {
            Debug.LogWarning($"HUD '{hudName}' not found in scene {sceneName}");
            hudCanvasGroup = null;
            return;
        }

        hudCanvasGroup = hud.GetComponent<CanvasGroup>();

        if (hudCanvasGroup == null)
        {
            Debug.LogError($"HUD '{hudName}' is missing CanvasGroup!");
        }
    }


    #endregion

    #region Pause / Resume

    private void Pause()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isPaused = true;
        UpdateUnlockFinaleButtonState();
        SetHUDVisible(false);
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);

    
        isPaused = false;

        SetHUDVisible(true);
        
        Time.timeScale = previousTimeScale;
    }

    public void PauseButton()
    {
        if (!isPaused)
            Pause();
    }

    #endregion

    #region Settings Menu

    public void OpenSettings()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }
    
    public void OpenCredits()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (creditsMenuUI != null)
            creditsMenuUI.SetActive(true);
    }

    public void CloseCredits()
    {
        if (creditsMenuUI != null)
            creditsMenuUI.SetActive(false);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    #endregion

    #region Helpers

    private void SetHUDVisible(bool visible)
    {
        if (hudCanvasGroup == null)
            return;

        hudCanvasGroup.alpha = visible ? 1f : 0f;
        hudCanvasGroup.interactable = visible;
        hudCanvasGroup.blocksRaycasts = visible;
    }

    private bool IsBootScene()
    {
        return SceneManager.GetActiveScene().name == "BootScene";
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
    
    /// <summary>
    /// Unlocks the finale content if the player is in the FarmScene and the GameFinaleFarmManager is available
    /// </summary>
    public void UnlockFinaleContent()
    {
        if (_gameFinaleFarmManager != null && SceneManager.GetActiveScene().name == "FarmScene")
        {
            Resume();
            _gameFinaleFarmManager.UnlockFinaleCheat();
        }
    }

    #endregion
}
