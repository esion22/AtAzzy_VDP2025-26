using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/// <summary>
/// DataManager is a singleton class that manages player data across different scenes.
/// It tracks various statistics such as seeds planted, recipes completed, and time spent in different scenes.
/// </summary>
public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    private string currentScene;

    [Header("Player Info")] public int playerId;

    [Header("Data")] 
    public int numberOfSeedsPlanted;
    public int numberOfSeedsFailed;
    public int numberOfRecipesDone;
    public int numberOfRecipesFailed;
    public float timeSpentInFarm;
    public float timeSpentInTavern;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// It initializes the singleton instance and sets up player data.
    /// </summary>
    void Awake()
    {
        Debug.Log("Awake called, Instance: " + Instance);
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // generate player ID if not exists
            if (PlayerPrefs.HasKey("PlayerID"))
                playerId = PlayerPrefs.GetInt("PlayerID");
            else
            {
                playerId = Random.Range(1000, 9999);
                PlayerPrefs.SetInt("PlayerID", playerId);
            }

            Debug.Log("Player ID: " + playerId);
            
            SceneManager.activeSceneChanged += OnSceneChanged;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Methods to increment various data counters.
    /// </summary>
    public void AddSeedPlanted() => numberOfSeedsPlanted++;
    public void AddSeedFailed() => numberOfSeedsFailed++;
    public void AddRecipeDone() => numberOfRecipesDone++;
    public void AddRecipeFailed() => numberOfRecipesFailed++;

    /// <summary>
    /// Update is called once per frame, tracking time spent in each scene.
    /// </summary>
    private void Update()
    {
        if (currentScene == "FarmScene")
        {
            timeSpentInFarm += Time.deltaTime;
        }
        else if (currentScene == "TavernScene")
        {
            timeSpentInTavern += Time.deltaTime;
        }
    }

    /// <summary>
    /// Called when the active scene changes, updating the current scene name.
    /// </summary>
    /// <param name="oldScene">Scene that was active before the change.</param>
    /// <param name="newScene">Scene that is now active.</param>
    void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log("Changed scene from " + oldScene.name + " to " + newScene.name);
        currentScene = newScene.name;
    }
}