using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Main Game Manager class that serves as the central hub for game systems.
 * Implements singleton pattern to provide global access to core game components.
 * Responsibilities:
 * - Managing game session persistence across scenes
 * - Providing access to subsystem managers (ItemManager, etc.)
 * - Coordinating between different game systems
 */
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public HashSet<string> pickedItems = new HashSet<string>();
    private int _currentDay = 1;
    private ItemManager itemManager;
    [System.NonSerialized]
    public Inventory playerInventory;

    /*
     * Unity Awake callback.
     * Initializes the GameManager singleton and core systems.
     */
    private void Awake()
    {
        // Debug.Log($"[GameManager] Awake started - Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

        if (instance != null && instance != this)
        {
            // Debug.Log($"<color=orange>[GameManager] Destroying duplicate instance. Existing: {instance.gameObject.name}, New: {gameObject.name}</color>");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        gameObject.name = "GameManager (Persistent)";

        // Debug.Log($"<color=green>[GameManager] Singleton instance set. GameObject: {gameObject.name}</color>");

        InitializeCoreSystems();
    }


    /*
     * Callback invoked when a new scene finishes loading.
     * @param scene The loaded scene.
     * @param mode The loading mode.
     */
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Debug.Log($"[GameManager] OnSceneLoaded - Scene: {scene.name}, GameManager exists: {this != null}");

        if (this == null)
        {
            // Debug.LogError("[GameManager] GameManager instance is NULL in OnSceneLoaded!");
            return;
        }

        // Debug.Log($"[GameManager] Still alive in scene: {scene.name}");
        
        StartCoroutine(DelayedReinitialization());
    }

    /*
     * Initializes core game systems such as Inventory and ItemManager.
     */
    private void InitializeCoreSystems()
    {
        if (playerInventory == null)
        {
            playerInventory = new Inventory(36);
            // Debug.Log("<color=green>[GameManager] Created new Inventory(36)</color>");
        }
        else
        {
            // Debug.Log("<color=cyan>[GameManager] Using existing Inventory</color>");
        }

        itemManager = GetComponent<ItemManager>();
        if (itemManager == null)
        {
            itemManager = gameObject.AddComponent<ItemManager>();
            // Debug.Log("<color=yellow>[GameManager] Added ItemManager component</color>");
        }

        DebugInventoryState();
    }

    /*
     * Prints the inventory state for debugging.
     */
    private void DebugInventoryState()
    {
        if (playerInventory != null && playerInventory.getSlots() != null)
        {
            int emptySlots = 0;
            int filledSlots = 0;

            foreach (var slot in playerInventory.getSlots())
            {
                if (slot.getType() == CollectableType.NONE)
                    emptySlots++;
                else
                    filledSlots++;
            }

            // Debug.Log($"[GameManager] Inventory state: {filledSlots} filled, {emptySlots} empty slots");
        }
    }

    /*
     * Registers the OnSceneLoaded callback.
     */
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /*
     * Unregisters the OnSceneLoaded callback.
     */
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /*
     * Delays reinitialization by one frame to ensure all scene objects are awake.
     * @returns IEnumerator for coroutine.
     */
    private IEnumerator DelayedReinitialization()
    {
        yield return null;
        ReconnectSystems();
    }

    /*
     * Reconnects systems after a scene load.
     */
    private void ReconnectSystems()
    {
        // Debug.Log("<color=green>[GameManager] Systems reconnected after scene load</color>");
        
        //Fetch the money counter if it is present and initialize it
        this.GetComponent<PlayerStats>().Initialize();
    }

    /*
     * Returns the ItemManager instance.
     * @returns The ItemManager used by the GameManager.
     */
    public ItemManager getItemManager()
    {
        return itemManager;
    }
    
    public int GetCurrentDay()
    {
        return _currentDay;
    }

    public void IncrementDay()
    {
        _currentDay++;
    }

    public void ResetDay()
    {
        _currentDay = 1;
    }
}