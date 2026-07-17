using UnityEngine;
using System.Collections.Generic;

public class GameFinaleFarmManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private DSDialogueUI dialogueUI;
    [SerializeField] private DSDialogueRunner dialogueRunner;
    [SerializeField] private Sprite dragonSprite;
    
    [Header("Ingredients for cheat")]
    [SerializeField] private List<CollectableType> allIngredients;
    [SerializeField] private List<int> allIngredientQuantities;

    private int _currentStep = 0;
    private int _pendingStep = -1;
    private bool _dialogueVisible = false;
    private int _pendingNextStep = -1;
    
    private const string _FARM_FINALE_COMPLETED_KEY = "FarmFinaleCompleted";
    
    public static GameFinaleFarmManager Instance { get; private set; }
    public bool IsActive { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // skip if the finale has already been completed
        if (PlayerPrefs.GetInt(_FARM_FINALE_COMPLETED_KEY, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void Update()
    {
        if (!IsActive || !_dialogueVisible) return;
        
        // Proceed on Space or Enter key press
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            HideDialogue();

            // -2 signals tutorial completion
            if (_pendingNextStep == -2)
            {
                _pendingNextStep = -1;
                CompleteFarmFinale();
                return;
            }
                
            // Show pending next step if exists
            if (_pendingNextStep >= 0)
            {
                int next = _pendingNextStep;
                _pendingNextStep = -1;
                ShowStep(next);
            }
        } 
        
        // Capture mouse click to prevent dialogue interference
        if (Input.GetMouseButtonDown(0))
        {
            // Do nothing, capturing the dialogue click
        }
    }

    public void TriggerFinale()
    {
        IsActive = true;

        ShowStep(0);
    }
    
    private void ShowStep(int step)
    {
        _currentStep = step;

        switch (step)
        {
            // You bought the essential recipes
            case 0:
                ShowDialogue("Oh, I see you bought the recipes for the Rainbow Essence and the Rainbow Burrito...", 1);
                break;
            
            // 
            case 1:
                ShowDialogue("I know these recipes! They are said to have the power to lift curses!", 2);
                break;
            
            //
            case 2:
                ShowDialogue("If we prepare these dishes, they might just cure my curse! But it seems like we need a lot of ingredients...", 3);
                break;
            
            //
            case 3:
                ShowDialogue("1 carrot, 1 pepper, 1 onion, 2 lettuce, 4 tomatoes, 1 grape, 1 strawberry, 1 wheat, 1 egg, 1 cheese, 1 milk, 1 raw fish and 1 raw steak.", 4);
                break;
            
            //
            case 4:
                ShowDialogue("If you forget about them, just look at the book by pressing 'B'. Once you have them all, we will prepare the rainbow burrito together!", -2);
                break;
        }
    }
    
    private void ShowDialogue(string text, int nextStepOnDismiss = -1)
    {
        _dialogueVisible = true;
        _pendingNextStep = nextStepOnDismiss;

        if (dialogueUI != null)
        {
            dialogueUI.ShowLine(text);
            dialogueUI.SetPortrait(dragonSprite);
            
            Time.timeScale = 1;
            
            dialogueUI.SetHUDVisible(true);
        }
    }

    private void HideDialogue()
    {
        _dialogueVisible = false;

        if (dialogueUI != null)
        {
            dialogueUI.root.SetActive(false);
            dialogueUI.SetHUDVisible(true);
        }
    }

    private void CompleteFarmFinale()
    {
        IsActive = false;
        
        PlayerPrefs.SetInt(_FARM_FINALE_COMPLETED_KEY, 1);
        PlayerPrefs.Save();
    }
    
    
    public void UnlockFinaleCheat()
    {
        // unlock all recipes
        RecipeTracker recipeTracker = FindFirstObjectByType<RecipeTracker>();
        recipeTracker.UnlockAllRecipes();
        
        
        GameManager gameManager = GameManager.instance;
        ItemManager itemManager = gameManager.getItemManager();
        Inventory playerInventory = gameManager.playerInventory;
        
        if (playerInventory == null || itemManager == null)
        {
            Debug.LogError("Player inventory or item manager not found. Cannot unlock finale cheat.");
            return;
        }
        
        
        // give player all needed ingredients needed for the finale
        foreach (CollectableType ingredientType in allIngredients)
        {
            // Try as INGREDIENT first, then as FOOD
            string keyIngredient = CollectableCategory.INGREDIENT + " - " + ingredientType;

            Collectable prefab = itemManager.GetItemByCategoryType(keyIngredient);

            if (prefab != null)
            {
                // give the player the specified quantity of this ingredient
                int quantity = allIngredientQuantities[allIngredients.IndexOf(ingredientType)];
                for (int i = 0; i < quantity; i++)
                {
                    playerInventory.Add(prefab);
                }
            }
        }
        
        // start the finale dialogue
        TriggerFinale();
    }

    #if UNITY_EDITOR
        /*
         * Editor menu item to display current tutorial key value.
         */
        [UnityEditor.MenuItem("Tools/Finale/Show Farm Finale Key")]
        public static void ShowTutorialKey()
        {
            int value = PlayerPrefs.GetInt(_FARM_FINALE_COMPLETED_KEY, 0);
            Debug.Log($"{_FARM_FINALE_COMPLETED_KEY} = {value} (0=not done, 1=completed)");
        }

        /*
         * Editor menu item to reset only the tutorial PlayerPrefs key.
         */
        [UnityEditor.MenuItem("Tools/Finale/Reset Farm Finale Key Only")]
        public static void ResetTutorialKeyOnly()
        {
            PlayerPrefs.DeleteKey(_FARM_FINALE_COMPLETED_KEY);
            PlayerPrefs.Save();
            Debug.Log("Farm Finale key deleted. Reload scene to replay farm finale.");
        }
    #endif
}
