using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CookingStations;
using Orders;
using PixelAdventures.Scripts.Completed_Scripts;

namespace Tutorial
{
    /*
     * Manages the cooking tutorial flow in the tavern scene.
     * Guides the player through cooking three recipes: Baked Potato, Steak, and Salad.
     * Handles dialogue display, ingredient management, and tutorial progression.
     */
    public class CookingTutorial : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image dragonPortrait;
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private DSDialogueUI dialogueUI;
        [SerializeField] private DSDialogueRunner dialogueRunner;



        [Header("References")]
        [SerializeField] private Transform cookingStationHighlight;

        [Header("Debug")]
        [SerializeField] private bool skipTutorial = false;

        [Header("Tutorial Ingredients")]
        [SerializeField] private List<GameObject> ingredientPrefabs;

        [Header("Highlight References")]
        [SerializeField] private HotbarController hotbarController;
        [SerializeField] private Vector3 arrowOffset = new Vector3(-1.5f, -1.5f, 0);
        [SerializeField] private Vector3 cuttingBoardArrowOffset = new Vector3(-1.0f, -0.5f, 0);
        
        // Current tutorial step index
        private int _currentStep = 0;
        // Reference to the current arrow highlight object
        private GameObject _currentArrow;
        // Reference to the player's inventory
        private Inventory _playerInventory;
        // List of all cooking stations in the scene
        private List<AbstractCookingStation> _cookingStations = new List<AbstractCookingStation>();
        // Flag indicating if a dialogue is currently visible
        private bool _dialogueVisible = false;
        // Next step to show after dialogue dismissal (-1 if none)
        private int _pendingNextStep = -1;

        // Reference to the order manager
        private OrderManager _orderManager;
        // Reference to the menu selector
        private MenuManager _menuManager;
        // Cooking station transforms (found at runtime)
        private Transform _ovenTransform;
        private Transform _stoveTransform;
        private Transform _cuttingBoardTransform;
        // tutorial order
        private Order _currentTutorialOrder;
        private bool _isProcessingInventoryChange = false;
        
        // Spell tutorial colors (bright versions)
        private static readonly Color FireColor = new Color(1f, 0.4f, 0.2f, 1f);    
        private static readonly Color WaterColor = new Color(0.2f, 0.7f, 1f, 1f);  
        private static readonly Color GrowthColor = new Color(0.3f, 1f, 0.4f, 1f);  
        private static readonly Color CutColor = new Color(0.6f, 0.6f, 0.7f, 1f);      
        // Index of currently highlighted slot for tutorial (-1 if none)
        private int _highlightedSlotIndex = -1;
        
        // PlayerPrefs key for tutorial completion status
        private const string _TUTORIAL_COMPLETE_KEY = "CookingTutorialDone";

        // Singleton instance
        public static CookingTutorial Instance { get; private set; }
        // Flag indicating if the tutorial is currently active
        public bool IsActive { get; private set; }

        // List of saved inventory slots for restoration
        private List<SlotData> _savedSlots = new List<SlotData>();

        /*
         * Struct to store slot data for inventory backup and restoration.
         */
        [System.Serializable]
        private struct SlotData
        {
            public CollectableCategory category;
            public CollectableType type;
            public int count;

            /*
             * Creates a SlotData from an existing Slot.
             *
             * @param slot The slot to copy data from
             */
            public SlotData(Slot slot)
            {
                category = slot.getCategory();
                type = slot.getType();
                count = slot.getCount();
            }
        }

        /*
         * Resets the tutorial progress via context menu.
         * Deletes the PlayerPrefs key so the tutorial will play on next scene load.
         */
        [ContextMenu("Reset Tutorial")]
        public void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(_TUTORIAL_COMPLETE_KEY);
            PlayerPrefs.Save();
            Debug.Log("CookingTutorial: Tutorial reset - will play on next scene load");
        }

        /*
         * Initializes the singleton instance and gets references.
         */
        private void Awake()
        {
            Instance = this;

            GameManager gameManager = GameManager.instance;
            if (gameManager != null)
            {
                _playerInventory = gameManager.playerInventory;
            }

            // If tutorial already completed, just enable dialogue runner and deactivate
            if (PlayerPrefs.GetInt(_TUTORIAL_COMPLETE_KEY, 0) == 1)
            {
                if (dialogueRunner != null)
                {
                    dialogueRunner.enabled = true;
                }
                gameObject.SetActive(false);
                return;
            }

            // Tutorial not done yet - disable systems
            if (!skipTutorial)
            {
                _menuManager = FindFirstObjectByType<MenuManager>();
                if (_menuManager != null)
                {
                    _menuManager.gameObject.SetActive(false);
                }

                // Disable dialogue runner during tutorial
                if (dialogueRunner != null)
                {
                    dialogueRunner.enabled = false;
                }
            }
        }

        /*
         * Initializes the tutorial if not skipped or already completed.
         * Subscribes to cooking station events and starts the tutorial.
         */
        private void Start()
        {
            // Skip tutorial if flag is set or already completed
            if (skipTutorial || PlayerPrefs.GetInt(_TUTORIAL_COMPLETE_KEY, 0) == 1)
            {
                gameObject.SetActive(false);
                return;
            }
            
            _orderManager = FindFirstObjectByType<OrderManager>();

            // Find and subscribe to all cooking stations
            _cookingStations.AddRange(FindObjectsOfType<AbstractCookingStation>());

            foreach (var station in _cookingStations)
            {
                if (station is OvenStation)
                    _ovenTransform = station.transform;
                else if (station is StoveStation)
                    _stoveTransform = station.transform;
                else if (station is CuttingBoard)
                    _cuttingBoardTransform = station.transform;
                station.OnRecipeCompleted += HandleRecipeCompleted;
                station.OnCookingFailed += HandleCookingFailed;
                station.OnIngredientPlaced += (ingredientType) =>
                {
                    HandleIngredientPlaced(ingredientType);
                };
                station.OnSpellReceived += HandleSpellReceived;
            }

            // Subscribe to inventory changes
            if (_playerInventory != null)
            {
                _playerInventory.OnInventoryChanged += HandleInventoryChanged;
            }

            SavePlayerIngredients();
            StartTutorial();
        }

        /*
         * Handles input for dialogue dismissal.
         * Processes Space/Enter keys to advance the tutorial.
         */
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
                    CompleteTutorial();
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

        /*
         * Starts the tutorial sequence.
         * Disables the dialogue runner, order manager, and shows the first step.
         */
        private void StartTutorial()
        {
            IsActive = true;

            // Disable dialogue runner to prevent other dialogues
            if (dialogueRunner != null)
            {
                dialogueRunner.StopAllCoroutines();
                dialogueRunner.enabled = false;
            }

         
            GiveTutorialIngredients();
            SavePlayerIngredients();
            ShowStep(0);
        }
        
        

        
        /*
         * Gives the player all ingredients needed for the tutorial.
         * Adds potato, raw steak, lettuce, and tomato to the inventory.
         */
        private void GiveTutorialIngredients()
        {
            PlayerItemController playerItemController = FindFirstObjectByType<PlayerItemController>();
            if (playerItemController == null)
            {
                return;
            }

            Inventory inventory = playerItemController.GetInventory();

            // List of ingredients needed for the tutorial
            CollectableType[] neededIngredients = new CollectableType[]
            {
                CollectableType.POTATO
            };

            foreach (CollectableType ingredientType in neededIngredients)
            {
                GameObject toSpawn = null;
                foreach (GameObject obj in ingredientPrefabs)
                {
                    Collectable collectable = obj.GetComponent<Collectable>();
                    if (collectable != null && collectable.getType() == ingredientType)
                    {
                        toSpawn = obj;
                        inventory.Add(collectable);
                        break;
                    }
                }
            }

            inventory.OnInventoryChanged?.Invoke();
        }

        /*
         * Selects a specific ingredient type in the player's inventory.
         * Updates both the selection index and the active selection.
         *
         * @param targetType The type of ingredient to select
         */
        private void SelectIngredientInInventory(CollectableType targetType)
        {
            if (_playerInventory == null) return;

            List<Slot> slots = _playerInventory.getSlots();
    
            // Prima verifica che l'item esista davvero
            int foundIndex = -1;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].getType() == targetType && slots[i].getCount() > 0)
                {
                    foundIndex = i;
                    break;
                }
            }
    
            if (foundIndex >= 0)
            {
                _playerInventory.setSelectionIndex(foundIndex);
                _playerInventory.setSelection(slots[foundIndex]);
                Debug.Log($"CookingTutorial: Selected {targetType} at slot {foundIndex}");
            }
            else
            {
                Debug.LogWarning($"CookingTutorial: Could not find {targetType} in inventory!");
            }
        }
        
        /*
         * Restores only the specified ingredients that are missing from the inventory.
         * Used for partial restoration when cooking fails with incomplete recipe.
         *
         * @param ingredientsToRestore Variable array of ingredient types to restore
         */
        private void RestoreLostIngredients(params CollectableType[] ingredientsToRestore)
        {
            if (_playerInventory == null) return;

            ItemManager itemManager = GameManager.instance.getItemManager();
            if (itemManager == null) return;

            foreach (CollectableType ingredientType in ingredientsToRestore)
            {
                // Check if ingredient already exists in inventory
                bool alreadyHas = false;
                foreach (Slot slot in _playerInventory.getSlots())
                {
                    if (slot.getType() == ingredientType)
                    {
                        alreadyHas = true;
                        break;
                    }
                }

                // Add ingredient if not present
                if (!alreadyHas)
                {
                    // Try as INGREDIENT first, then as FOOD
                    string keyIngredient = CollectableCategory.INGREDIENT + " - " + ingredientType;
                    string keyFood = CollectableCategory.FOOD + " - " + ingredientType;

                    Collectable prefab = itemManager.GetItemByCategoryType(keyIngredient);
                    if (prefab == null)
                    {
                        prefab = itemManager.GetItemByCategoryType(keyFood);
                    }

                    if (prefab != null)
                    {
                        _playerInventory.Add(prefab);
                    }
                }
            }

            _playerInventory.OnInventoryChanged?.Invoke();
        }
        
        
        /*
         * Saves the current state of all inventory slots.
         * Used for restoration when cooking fails.
         */
        private void SavePlayerIngredients()
        {
            _savedSlots.Clear();

            if (_playerInventory == null) return;

            foreach (Slot slot in _playerInventory.getSlots())
            {
                _savedSlots.Add(new SlotData(slot));
            }
        }

        /*
         * Restores all inventory slots to their previously saved state.
         * Clears all slots first (including BURNT items) then repopulates.
         */
        private void RestorePlayerIngredients()
        {
            if (_playerInventory == null || _savedSlots.Count == 0) return;

            List<Slot> slots = _playerInventory.getSlots();
            ItemManager itemManager = GameManager.instance.getItemManager();

            // Clear ALL slots first (removes BURNT items too)
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].Clear();
            }

            // Restore saved ingredients
            for (int i = 0; i < slots.Count && i < _savedSlots.Count; i++)
            {
                SlotData saved = _savedSlots[i];

                if (saved.type == CollectableType.NONE || saved.count <= 0)
                    continue;

                string key = saved.category.ToString() + " - " + saved.type.ToString();
                Collectable prefab = itemManager.GetItemByCategoryType(key);

                if (prefab != null)
                {
                    for (int j = 0; j < saved.count; j++)
                    {
                        slots[i].AddItem(prefab);
                    }
                }
            }

            // Select first non-empty slot
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].getType() != CollectableType.NONE)
                {
                    _playerInventory.setSelectionIndex(i);
                    break;
                }
            }

            _playerInventory.OnInventoryChanged?.Invoke();
        }
        /*
         * Displays the specified tutorial step.
         * Contains all dialogue text and logic for each step.
         *
         * @param step The step index to display
         */
        private void ShowStep(int step)
        {
            _currentStep = step;
            ClearHighlights();

            switch (step)
            {
                // Introduction
                case 0:
                    ShowDialogue("Welcome to the Tavern! I will teach you how to cook.", 1);
                    break;

                // Baked Potato introduction
                case 1:
                    ShowDialogue("Let's prepare a Baked Potato.", 2);
                    break;

                // Place potato in oven
                case 2:
                    EnsureIngredientAvailable(CollectableType.POTATO);
                    SelectIngredientInInventory(CollectableType.POTATO);
                    HighlightWorldPosition(_ovenTransform);
                    ShowDialogue("Put the potato from your inventory to the oven by pressing 'F'.");
                    break;

                // Cast fire spell on potato
                case 3:
                    HighlightSpellSlot(0); // Fire spell
                    ShowDialogue("Now use a Fire spell to cook it. Select the spell by pressing '1' and then click to cast it.");
                    break;

                // Cast water spell to stop cooking
                case 4:
                    HighlightSpellSlot(1); // Water spell
                    ShowDialogue("Wonderful! Now use a Water spell to cook it perfectly when it reaches the blue zone, otherwise it will burn. Select the spell by pressing '2' and then click to cast it.");
                    break;

                // Potato burnt - retry
                case 5:
                    ShowDialogue("What a pity, it seems the potato got burnt. Let's try again.",2);
                    RestorePlayerIngredients();
                    break;

                // Collect baked potato
                case 6:
                    HighlightWorldPosition(_ovenTransform);
                    CreateTutorialOrderFor(CollectableType.BAKED_POTATO);
                    ShowDialogue("Great job! You've cooked a Baked Potato. Now collect it from the oven.");
                    break;

                // Gold explanation
                case 7:
                    ShowDialogue("Look! A Customer! He is asking for a Baked Potato. Go to the counter and press 'F' to give it to him!");
                    break;

                // Steak introduction
                case 8:
                    ShowDialogue("Cooking allows you to earn gold by preparing dishes for your tavern. Now let's try cooking with something else. Let's make a steak!", 9);
                    break;

                // Place steak in stove
                case 9:
                    EnsureIngredientAvailable(CollectableType.RAW_STEAK);
                    SelectIngredientInInventory(CollectableType.RAW_STEAK);
                    HighlightWorldPosition(_stoveTransform);
                    ShowDialogue("Grab a raw steak from your inventory. Place it in the stove by pressing 'F'.");
                    break;

                // Cast fire spell on steak
                case 10:
                    HighlightSpellSlot(0); // Fire spell
                    ShowDialogue("Now use a Fire spell to cook it. Select the spell by pressing '1' and then click to cast it. You need to cast more than once, keep the fire alive!");
                    break;

                // Collect cooked steak
                case 11:
                    HighlightWorldPosition(_stoveTransform);
                    CreateTutorialOrderFor(CollectableType.COOKED_STEAK);
                    ShowDialogue("Excellent! You're doing great, now take the cooked steak from the stove and serve it to the customer at the counter!");
                    break;

                // Salad introduction
                case 12:
                    ShowDialogue("Remember, different recipes may require different cooking stations and spells. Let's try the last recipe: Salad!", 13);
                    break;

                // Place lettuce on cutting board
                case 13:
                    EnsureIngredientAvailable(CollectableType.LETTUCE);
                    SelectIngredientInInventory(CollectableType.LETTUCE);
                    HighlightWorldPosition(_cuttingBoardTransform, cuttingBoardArrowOffset);
                    ShowDialogue("In order to make a salad, take some lettuce from your inventory and place it in the cutting board by pressing 'F'.");
                    break;

                // Place tomato on cutting board
                case 14:
                    EnsureIngredientAvailable(CollectableType.TOMATO);
                    SelectIngredientInInventory(CollectableType.TOMATO);
                    HighlightWorldPosition(_cuttingBoardTransform, cuttingBoardArrowOffset);
                    ShowDialogue("A green salad is a bit sad, let's add some tomatoes! Take a tomato from your inventory and place it in the cutting board by pressing 'F'.");
                    break;
                
                case 23:
                    ShowDialogue("TIP: if you ever mess up the ingredients of a recipe, you can press 'R' while focused on the cooking station to retrieve all the ingredients back to your inventory!", 15);
                    break;

                // Cast cut spell
                case 15:
                    HighlightSpellSlot(3); // Cut spell
                    ShowDialogue("Now use the cut spell 3 times to chop the ingredients. Select the spell by pressing '4' and then click to cast it.");
                    break;

                // Collect salad
                case 16:
                    HighlightWorldPosition(_cuttingBoardTransform, cuttingBoardArrowOffset);
                    CreateTutorialOrderFor(CollectableType.SALAD);
                    ShowDialogue("Fantastic! Now collect your fresh salad from the cutting board and serve it to the customer at the counter!");
                    break;

                // Additional cooking stations hint
                case 17:
                    ShowDialogue("There are two more ways to cook: boiling and fermenting with the keg. Feel free to explore these cooking stations on your own once you have the recipes for them!", 18);
                    break;

                // Final tips
                case 18:
                    ShowDialogue("Remember to always keep an eye on the cooking process and use the appropriate spells to ensure your dishes are perfectly cooked.", 19);
                    break;

                // Tutorial almost completed
                case 19:
                    ShowDialogue("You can find new recipes as you progress in the game. Now you're ready to start cooking and managing your tavern!", 22);
                    break;
                
                // Tutorial complete
                case 20:
                    ShowDialogue("I am giving you the ingredient I had in stock, use them wisely. Good Luck!", -2);
                    break;
                
                case 22:
                    ShowDialogue("REMEMBER!\n If you forget about anything, you have my special book. Press 'B' to open it!", 20);
                    break;
                
                // Salad error - missing ingredients
                case 21:
                    ShowDialogue("Remember, if you do not add all the ingredients for a recipe, we cannot serve it! You can also press 'R' to retrieve all ingredients in the cooking station. Let's try again.", 13);
                    break;
            }
        }
        
        /*
         * Displays a dialogue message with optional automatic progression.
         *
         * @param text The dialogue text to display
         * @param nextStepOnDismiss The step to show after dismissal (-1 for none)
         */
        private void ShowDialogue(string text, int nextStepOnDismiss = -1)
        {
            _dialogueVisible = true;
            _pendingNextStep = nextStepOnDismiss;

            // Ensure dialogue runner is disabled
            if (dialogueRunner != null)
                dialogueRunner.enabled = false;

            if (dialogueUI != null)
            {
                dialogueUI.ShowLine(text);

                // Keep normal time scale during tutorial
                Time.timeScale = 1f;

                dialogueUI.SetHUDVisible(true);
            }
        }
        
        /*
         * Hides the current dialogue without affecting time scale.
         */
        private void HideDialogue()
        {
            _dialogueVisible = false;

            if (dialogueUI != null)
            {
                // Hide without touching timeScale (already at 1)
                dialogueUI.root.SetActive(false);
                dialogueUI.SetHUDVisible(true);
            }
        }
        
        /*
         * Creates an arrow highlight at a world position (pointing from left to right).
         *
         * @param target The transform to highlight
         */
        private void HighlightWorldPosition(Transform target, Vector3? customOffset = null)
        {
            if (arrowPrefab == null || target == null) return;

            ClearWorldHighlight();

            Vector3 offset = customOffset ?? arrowOffset;
            Vector3 position = target.position + offset;
            _currentArrow = Instantiate(arrowPrefab, position, Quaternion.Euler(0, 0, 90));
        }

        /*
         * Highlights a spell slot in the hotbar with a color based on the spell type.
         *
         * @param slotIndex The index of the spell slot to highlight
         */
        private void HighlightSpellSlot(int slotIndex)
        {
            if (hotbarController == null) return;
            if (slotIndex < 0 || slotIndex >= hotbarController.slots.Length) return;

            ClearHotbarHighlight();

            Color color = slotIndex switch
            {
                0 => FireColor,
                1 => WaterColor,
                2 => GrowthColor,
                3 => CutColor,
                _ => Color.white
            };

            hotbarController.slots[slotIndex].SetTutorialHighlight(color);
            _highlightedSlotIndex = slotIndex;
        }
        
        /**
         * Creates a tutorial order for a specific food type.
         * @param {CollectableType} foodType - The food type to order.
         */
        private void CreateTutorialOrderFor(CollectableType foodType)
        {

            if (_orderManager == null) return;

            // Find the recipe for this food type
            Recipe targetRecipe = FindRecipeForFood(foodType);

            if (targetRecipe == null)
            {
                Debug.LogWarning($"CookingTutorial: No recipe found for {foodType}");
                return;
            }

            _currentTutorialOrder = _orderManager.CreateTutorialOrder(targetRecipe);
        }
        
        /**
         * Finds a recipe that produces the specified food type.
         * @param {CollectableType} foodType - The food type to find.
         * @returns {Recipe} The matching recipe or null.
         */
        private Recipe FindRecipeForFood(CollectableType foodType)
        {
            // Access recipes via reflection (same pattern as MenuManager)
            var field = typeof(OrderManager).GetField("recipes", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    
            if (field == null) return null;
    
            var recipes = field.GetValue(_orderManager) as List<Recipe>;
            if (recipes == null) return null;

            foreach (Recipe recipe in recipes)
            {
                if (recipe.foodType == foodType)
                    return recipe;
            }
    
            // Check prototype recipes too
            var protoField = typeof(OrderManager).GetField("prototypeRecipes",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    
            if (protoField != null)
            {
                var protoRecipes = protoField.GetValue(_orderManager) as List<Recipe>;
                if (protoRecipes != null)
                {
                    foreach (Recipe recipe in protoRecipes)
                    {
                        if (recipe.foodType == foodType)
                            return recipe;
                    }
                }
            }
    
            return null;
        }
        
        /**
         * Called when a tutorial order is fulfilled.
         * @param {CollectableType} foodType - The delivered food type.
         */
        public void OnTutorialOrderFulfilled(CollectableType foodType)
        {
            if (!IsActive || _currentTutorialOrder == null) return;

            _currentTutorialOrder = null;

            // Advance based on delivered food
            if (foodType == CollectableType.BAKED_POTATO)
            {
                ShowStep(8); // go to the money explanation
            }
            else if (foodType == CollectableType.COOKED_STEAK)
            {
                ShowStep(12); // go to the salad introduction
            }
            else if (foodType == CollectableType.SALAD)
            {
                ShowStep(17); // tutorial completion steps
            }
        }

        /*
         * Clears the highlight from the currently highlighted hotbar slot.
         */
        private void ClearHotbarHighlight()
        {
            if (hotbarController == null) return;

            if (_highlightedSlotIndex >= 0 && _highlightedSlotIndex < hotbarController.slots.Length)
            {
                hotbarController.slots[_highlightedSlotIndex].SetTutorialHighlight(null);
            }
            _highlightedSlotIndex = -1;
        }


        /*
         * Removes the world highlight arrow.
         */
        private void ClearWorldHighlight()
        {
            if (_currentArrow != null)
            {
                Destroy(_currentArrow);
                _currentArrow = null;
            }
        }

        /*
        * Removes all highlight objects (world arrows and hotbar arrows).
        */
        private void ClearHighlights()
        {
            ClearWorldHighlight();
            ClearHotbarHighlight();
        }

        /*
         * Handles the completion of a recipe at a cooking station.
         * Advances the tutorial based on the completed recipe type.
         *
         * @param resultType The type of the completed recipe
         */
        private void HandleRecipeCompleted(CollectableType resultType)
        {
            if (!IsActive) return;


            HideDialogue();

            // Advance to appropriate step based on completed recipe
            if (resultType == CollectableType.BAKED_POTATO)
            {
                OnCookingComplete(6);
            }
            else if (resultType == CollectableType.COOKED_STEAK)
            {
                OnCookingComplete(11);
            }
            else if (resultType == CollectableType.SALAD)
            {
                OnCookingComplete(16);
            }
            else if (resultType == CollectableType.BURNT)
            {
                RestorePlayerIngredients();
                OnCookingComplete(5);
            }
        }
        
        /*
         * Advances to the specified step after cooking completion.
         *
         * @param nextStep The step index to advance to
         */
        public void OnCookingComplete(int nextStep)
        {
            if (!IsActive) return;
            ShowStep(nextStep);
        }

        /*
         * Handles cooking failure events.
         * Restores ingredients and shows appropriate error message based on current step.
         */
        private void HandleCookingFailed()
        {
            if (!IsActive) return;

            // Salad steps (13-16): restore only lost ingredients and show error
            if (_currentStep >= 13 && _currentStep <= 16)
            {
                RestoreLostIngredients(CollectableType.LETTUCE, CollectableType.TOMATO);
                ShowStep(21);
            }
            // Potato steps (2-6): full restore and retry
            else if (_currentStep >= 2 && _currentStep <= 6)
            {
                RestorePlayerIngredients();
                ShowStep(2);
            }
            // Steak steps (9-11): full restore and retry
            else if (_currentStep >= 9 && _currentStep <= 11)
            {
                RestorePlayerIngredients();
                ShowStep(9);
            }
        }

        /*
         * Handles ingredient placement events from cooking stations.
         * Advances tutorial when correct ingredient is placed at the right step.
         *
         * @param ingredientType The type of ingredient that was placed
         */
        private void HandleIngredientPlaced(CollectableType ingredientType)
        {
            if (!IsActive) return;

            switch (_currentStep)
            {
                case 2 when ingredientType == CollectableType.POTATO:
                    HideDialogue();
                    ShowStep(3);
                    break;
                case 9 when ingredientType == CollectableType.RAW_STEAK:
                    HideDialogue();
                    ShowStep(10);
                    break;
                case 13 when ingredientType == CollectableType.LETTUCE:
                    HideDialogue();
                    ShowStep(14);
                    break;
                case 14 when ingredientType == CollectableType.TOMATO:
                    HideDialogue();
                    ShowStep(23);
                    break;
            }
        }
        
        /*
         * Handles inventory change events.
         * Checks if player collected the required item for current step.
         */
        private void HandleInventoryChanged()
        {
            if (!IsActive || _isProcessingInventoryChange) return;

            _isProcessingInventoryChange = true;

            try
            {
                // Step 6: wait for BAKED_POTATO collection
                if (_currentStep == 6)
                {
                    if (HasItemInInventory(CollectableType.BAKED_POTATO))
                    {
                        SelectIngredientInInventory(CollectableType.BAKED_POTATO);
                        HideDialogue();
                        ShowStep(7);
                        return;
                    }
                }
                // Step 11: wait for COOKED_STEAK collection
                else if (_currentStep == 11)
                {
                    if (HasItemInInventory(CollectableType.COOKED_STEAK))
                    {
                        SelectIngredientInInventory(CollectableType.COOKED_STEAK);
                        HideDialogue();
                        return;
                    }
                }
                // Step 16: wait for SALAD collection
                else if (_currentStep == 16)
                {
                    if (HasItemInInventory(CollectableType.SALAD))
                    {
                        SelectIngredientInInventory(CollectableType.SALAD);
                        HideDialogue();
                        return;
                    }
                }
            }
            finally
            {
                _isProcessingInventoryChange = false;
            }
        }

        /*
         * Checks if a specific item type exists in the player's inventory.
         *
         * @param type The collectable type to search for
         * @returns True if the item exists in inventory, false otherwise
         */
        private bool HasItemInInventory(CollectableType type)
        {
            if (_playerInventory == null) return false;

            foreach (Slot slot in _playerInventory.getSlots())
            {
                if (slot.getType() == type)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Ensures the player has a specific ingredient, restoring it if missing.
        /// </summary>
        /// <param name="ingredientType">The ingredient type to check/restore</param>
        /// <returns>True if ingredient is available (or was restored)</returns>
        private bool EnsureIngredientAvailable(CollectableType ingredientType)
        {
            if (HasItemInInventory(ingredientType))
                return true;

            // Ingredient missing, restore it
            RestoreLostIngredients(ingredientType);
            return HasItemInInventory(ingredientType);
        }

        /*
         * Handles spell cast events from cooking stations.
         * Used to track progress in cooking steps.
         *
         * @param spellType The type of spell that was cast
         */
        private void HandleSpellReceived(SpellEnum spellType)
        {
            if (!IsActive) return;

            switch (_currentStep)
            {
                case 3 when spellType == SpellEnum.FIRE:
                    HideDialogue();
                    ShowStep(4); // Oven: fire cast, now wait for water
                    break;
                case 10 when spellType == SpellEnum.FIRE:
                    // Stove: fire cast (may need multiple casts)
                    break;
                case 15 when spellType == SpellEnum.CUT:
                    // Cutting board: cut spell cast
                    break;
            }
        }
        
        /*
         * Gives bonus ingredients to the player upon tutorial completion.
         */
        private void GiveCompletionRewards()
        {
            if (_playerInventory == null) return;
            
            ItemManager itemManager = GameManager.instance?.getItemManager();
            if (itemManager == null) return;
        
            // Define rewards: (type, quantity)
            var rewards = new (CollectableType type, int quantity)[]
            {
                (CollectableType.POTATO, 5),
                (CollectableType.RAW_STEAK, 2),
                (CollectableType.LETTUCE, 4),
                (CollectableType.TOMATO, 4)
            };
        
            foreach (var (type, quantity) in rewards)
            {
                // Try INGREDIENT category first, then FOOD
                string keyIngredient = CollectableCategory.INGREDIENT + " - " + type;
                string keyFood = CollectableCategory.FOOD + " - " + type;
        
                Collectable prefab = itemManager.GetItemByCategoryType(keyIngredient);
                if (prefab == null)
                {
                    prefab = itemManager.GetItemByCategoryType(keyFood);
                }
        
                if (prefab != null)
                {
                    for (int i = 0; i < quantity; i++)
                    {
                        _playerInventory.Add(prefab);
                    }
                }
            }
        
            _playerInventory.OnInventoryChanged?.Invoke();
        }
        

        /*
         * Completes the tutorial and restores normal game state.
         * Re-enables dialogue runner and order manager, saves completion flag.
         */
        private void CompleteTutorial()
        {
            
            GiveCompletionRewards();

            // Re-enable order manager
            if (_orderManager != null)
            {
                _orderManager.gameObject.SetActive(true);
                _orderManager.StartOrderGeneration();
            }
            // Re-enable menu manager
            if (_menuManager != null)
            {
                _menuManager.gameObject.SetActive(true);
            }

            // Disable dialogue runner because we do not want to run the welcome dialogue after the tutorial
            if (dialogueRunner != null)
                dialogueRunner.enabled = false;

            IsActive = false;

            // Save completion status
            PlayerPrefs.SetInt(_TUTORIAL_COMPLETE_KEY, 1);
            PlayerPrefs.Save();
            
            ClearHighlights();
        }

        /*
         * Cleans up event subscriptions when the object is destroyed.
         */
        private void OnDestroy()
        {
            // Unsubscribe from cooking station events
            foreach (var station in _cookingStations)
            {
                if (station != null)
                {
                    station.OnRecipeCompleted -= HandleRecipeCompleted;
                    station.OnCookingFailed -= HandleCookingFailed;
                    station.OnIngredientPlaced -= (ingredientType) =>
                    {
                        HandleIngredientPlaced(ingredientType);
                    };
                    station.OnSpellReceived -= HandleSpellReceived;
                }
            }

            // Unsubscribe from inventory events
            if (_playerInventory != null)
            {
                _playerInventory.OnInventoryChanged -= HandleInventoryChanged;
            }
        }

#if UNITY_EDITOR
        /*
         * Editor menu item to display current tutorial key value.
         */
        [UnityEditor.MenuItem("Tools/Tutorial/Show Cooking Tutorial Key")]
        public static void ShowTutorialKey()
        {
            int value = PlayerPrefs.GetInt("CookingTutorialDone", 0);
            Debug.Log($"CookingTutorialDone = {value} (0=not done, 1=completed)");
        }

        /*
         * Editor menu item to reset only the tutorial PlayerPrefs key.
         */
        [UnityEditor.MenuItem("Tools/Tutorial/Reset Cooking Tutorial Key Only")]
        public static void ResetTutorialKeyOnly()
        {
            PlayerPrefs.DeleteKey("CookingTutorialDone");
            PlayerPrefs.Save();
            Debug.Log("CookingTutorialDone key deleted. Reload scene to replay tutorial.");
        }
#endif
    }
}
