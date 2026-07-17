using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using CookingStations;
using DS;
using Finale;
using Orders;

public class GameFinaleCookingManager : MonoBehaviour
{
    [Header("UI")] [SerializeField] private DSDialogueUI dialogueUI;
    [SerializeField] private DSDialogueRunner dialogueRunner;
    [SerializeField] private Sprite humanPortrait;
    [SerializeField] private Sprite dragonPortrait;

    [Header("Ingredients")] 
    [SerializeField] private List<CollectableType> neededIngredients;
    [SerializeField] private List<CollectableCategory> neededCategories;
    [SerializeField] private List<int> neededQuantities;
    
    [Header("References")] 
    [SerializeField] private Vector3 azzyTargetPosition;
    [SerializeField] private GameObject azzyHumanPrefab;
    [SerializeField] private GameObject circlePrefab;
    
    [Header("Finale Screens")]
    [SerializeField] private GameObject successScreenPrefab;
    [SerializeField] private GameObject failureScreenPrefab;

    private int _currentStep = 0;
    private Inventory _playerInventory;
    private bool _dialogueVisible = false;
    private int _pendingNextStep = -1;
    private OrderManager _orderManager;
    private MenuManager _menuManager;
    private bool _isProcessingInventoryChange = false;
    private KegStation _kegStation;
    private CuttingBoard _cuttingBoard;
    private float _fadeDuration = 0.5f;
    private GameObject _highlight;
    private bool _playerInRange = false;
    private bool _azzyWaitingForBurrito = false;
    private GameObject _azzyGameObject;
    private AudioManager audioManager;
    private bool _azzyIsHuman = false;

    private const string _FINALE_COMPLETED_KEY = "CookingFinaleCompleted";
    private const string _FARM_FINALE_COMPLETED_KEY = "FarmFinaleCompleted";

    public static GameFinaleCookingManager Instance { get; private set; }
    public bool IsActive { get; private set; }

    private void Awake()
    {
        Instance = this;

        GameManager gameManager = GameManager.instance;
        if (gameManager != null)
        {
            _playerInventory = gameManager.playerInventory;
        }

        // If finale already completed, just enable dialogue runner and deactivate
        if (PlayerPrefs.GetInt(_FINALE_COMPLETED_KEY, 0) == 1 || PlayerPrefs.GetInt(_FARM_FINALE_COMPLETED_KEY, 0) == 0)
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.enabled = true;
            }

            gameObject.SetActive(false);
            return;
        }

        // Disable dialogue runner
        if (dialogueRunner != null)
        {
            dialogueRunner.enabled = false;
        }
    }

    private bool CookingFinaleShouldStart()
    {
        // the cooking finale should start if the farm finale has been completed and if the player has all the required ingredients in the inventory
        if (PlayerPrefs.GetInt(_FARM_FINALE_COMPLETED_KEY, 0) == 0)
        {
            return false;
        }
        
        // check if the player has all the possible recipes in the game
        RecipeTracker recipeTracker = FindFirstObjectByType<RecipeTracker>();
        List<Recipe> possibleRecipes = recipeTracker.GetAllRecipes();
        List<Recipe> unlockedRecipes = recipeTracker.GetUnlockedRecipes();
        if (possibleRecipes.Count != unlockedRecipes.Count)
        {
            Debug.Log("Player does not have all recipes unlocked for cooking finale. missing: " +
                      string.Join(", ", possibleRecipes.Except(unlockedRecipes).Select(r => r.foodType.ToString())));
            return false;
        }


        // Retrieve all slots from the player's inventory
        List<Slot> inventorySlots = _playerInventory.getSlots();
        
        // Get bare ingredients from inventory
        Dictionary<CollectableType, int> bareIngredients = GetBareIngredientsFromInventory(inventorySlots);
        // check that all needed ingredients are in bareIngredients with at least the needed quantity
        for (int i = 0; i < neededIngredients.Count; i++)
        { 
            CollectableType ingredient = neededIngredients[i];
            int quantity = neededQuantities[i];

            if (!bareIngredients.ContainsKey(ingredient) || bareIngredients[ingredient] < quantity)
            {
                // Debug log missing ingredient
                Debug.Log($"Missing ingredient {ingredient} x{quantity}. Inventory has: " +
                          string.Join(", ", bareIngredients.Select(kvp => $"{kvp.Key} x{kvp.Value}")));
                return false; // Missing required ingredient or quantity
            }
        }

        return true;
    }

    private Dictionary<CollectableType, int> GetBareIngredientsFromInventory(List<Slot> inventorySlots)
    {
        RecipeTracker recipeTracker = FindFirstObjectByType<RecipeTracker>();
        Dictionary<CollectableType, int> bareIngredients = new Dictionary<CollectableType, int>();

        foreach (Slot slot in inventorySlots)
        { 
            CollectableType slotType = slot.getType();
            
            // check if there is a recipe which has slotType as foodType
            Recipe recipe = recipeTracker.GetUnlockedRecipes().FirstOrDefault(r => r.foodType == slotType);
            if (recipe == null)
            {
                // no recipe found, so it's a bare ingredient
                if (bareIngredients.ContainsKey(slotType))
                {
                    bareIngredients[slotType] += slot.getCount();
                }
                else
                {
                    bareIngredients[slotType] = slot.getCount();
                }
            }
            else
            {
                // it is a recipe, so we need to get down to its ingredients
                foreach (CollectableType ingredient in recipe.ingredientsNeeded)
                {
                    // check if there is a recipe which has ingredient as foodType
                    Recipe ingredientRecipe = recipeTracker.GetUnlockedRecipes().FirstOrDefault(r => r.foodType == ingredient);
                    if (ingredientRecipe == null)
                    {
                        // no recipe found, so it's a bare ingredient
                        // get how many of this ingredient are needed in the recipe
                        int ingredientIndexInRecipe = recipe.ingredientsNeeded.IndexOf(ingredient);
                        int ingredientQuantityInRecipe = recipe.amountNeeded[ingredientIndexInRecipe];
                        if (bareIngredients.ContainsKey(ingredient))
                        {
                            bareIngredients[ingredient] += slot.getCount() * ingredientQuantityInRecipe;
                        }
                        else
                        {
                            bareIngredients[ingredient] = slot.getCount() * ingredientQuantityInRecipe;
                        }
                    }
                    else
                    {
                        // it is a recipe, so we need to go deeper
                        foreach (CollectableType subIngredient in ingredientRecipe.ingredientsNeeded)
                        { 
                            // check if there is a recipe which has subIngredient as foodType
                            Recipe subIngredientRecipe = recipeTracker.GetUnlockedRecipes().FirstOrDefault(r => r.foodType == subIngredient);
                            if (subIngredientRecipe == null)
                            {
                                // no recipe found, so it's a bare ingredient
                                // get how many of this sub-ingredient are needed in the ingredient recipe
                                int subIngredientIndexInRecipe = ingredientRecipe.ingredientsNeeded.IndexOf(subIngredient);
                                int subIngredientQuantityInRecipe = ingredientRecipe.amountNeeded[subIngredientIndexInRecipe];
                                // get how many of the ingredient are needed in the main recipe
                                int ingredientIndexInRecipe = recipe.ingredientsNeeded.IndexOf(ingredient);
                                int ingredientQuantityInRecipe = recipe.amountNeeded[ingredientIndexInRecipe];
                                // total quantity is slot count * ingredient quantity * sub-ingredient quantity
                                int totalQuantity = slot.getCount() * ingredientQuantityInRecipe * subIngredientQuantityInRecipe;
                                if (bareIngredients.ContainsKey(subIngredient))
                                {
                                    bareIngredients[subIngredient] += totalQuantity;
                                }
                                else
                                {
                                    bareIngredients[subIngredient] = totalQuantity;
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"GetBareIngredientsFromInventory: Ingredient {subIngredient} has a recipe, deeper levels not supported.");
                            }
                        }
                    }
                }
            }
        }
        return bareIngredients;
    }

    private void Start()
    {
        // Find the AudioManager in the scene
        audioManager = FindObjectOfType<AudioManager>();
        
        // skip if the finale has already been completed
        if (PlayerPrefs.GetInt(_FINALE_COMPLETED_KEY, 0) == 1)
        {
            gameObject.SetActive(false);
            
            // enable dialogue runner
            if (dialogueRunner != null)
            {
                dialogueRunner.enabled = true;
            }
            return;
        }

        // Check if the cooking finale should start
        if (CookingFinaleShouldStart())
        {
            // Disable menu and order managers
            _menuManager = FindFirstObjectByType<MenuManager>();
            if (_menuManager != null)
            {
                _menuManager.gameObject.SetActive(false);
            }

            _orderManager = FindFirstObjectByType<OrderManager>();
            if (_orderManager != null)
            {
                _orderManager.gameObject.SetActive(false);
            }
            
            // Play the finale background music
            if (audioManager != null)
            {
                audioManager.PlayBackgroundFinaleMusic();
            }
            
            // Find AzzyDragon object
            _azzyGameObject = GameObject.Find("AzzyDragon");
            _highlight = _azzyGameObject?.transform.Find("Highlight")?.gameObject;
            if (_highlight != null)
            {
                _highlight.SetActive(false); // Ensure it's deactivated initially
            }

            // Retain existing logic
            SubscribeToCookingEvents();
            TriggerCookingFinale();
        }
        else
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.enabled = true;
            }

            gameObject.SetActive(false);
        }
    }

    private void SubscribeToCookingEvents()
    {
        
        _kegStation = FindFirstObjectByType<KegStation>();
        if (_kegStation != null)
        {
            _kegStation.OnRecipeCompleted += HandleRecipeCompleted;
            _kegStation.OnCookingFailed += HandleCookingFailed;
        }

        _cuttingBoard = FindFirstObjectByType<CuttingBoard>();
        if (_cuttingBoard != null)
        {
            _cuttingBoard.OnRecipeCompleted += HandleRecipeCompleted;
            _cuttingBoard.OnCookingFailed += HandleCookingFailed;
        }
        
        OvenStation ovenStation = FindFirstObjectByType<OvenStation>();
        if (ovenStation != null)
        {
            ovenStation.OnRecipeCompleted += HandleRecipeCompleted;
            ovenStation.OnCookingFailed += HandleCookingFailed;
        }
        
        StoveStation stoveStation = FindFirstObjectByType<StoveStation>();
        if (stoveStation != null)
        {
            stoveStation.OnRecipeCompleted += HandleRecipeCompleted;
            stoveStation.OnCookingFailed += HandleCookingFailed;
        }
        
        BoilingStation boilingStation = FindFirstObjectByType<BoilingStation>();
        if (boilingStation != null)
        {
            boilingStation.OnRecipeCompleted += HandleRecipeCompleted;
            boilingStation.OnCookingFailed += HandleCookingFailed;
        }
    }
    
    private void Update()
    {
        if (_playerInRange && _azzyWaitingForBurrito && Input.GetKeyDown(KeyCode.F))
        {
            if (GiveRainbowBurritoToDragon())
            {
                _azzyWaitingForBurrito = false; // Reset the flag
                ShowStep(12); // Trigger case 12
                
            }
        }
        
        if (!IsActive || !_dialogueVisible) return;

        // Proceed on Space or Enter key press
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            HideDialogue();

            // -2 signals tutorial completion
            if (_pendingNextStep == -2)
            {
                _pendingNextStep = -1;
                CompleteFinale();
                return;
            }
            
            // -2 signals cooking finale failure
            if (_pendingNextStep == -3)
            {
                _pendingNextStep = -1;
                // retrieve the end of day manager to end the day
                FailedFinale();
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
    
    public void SetPlayerInRange(bool inRange)
    {
        _playerInRange = inRange;
    }
    
    /*
     * Checks if the player has the Rainbow Burrito and removes it from the inventory.
     * @return True if the Rainbow Burrito was successfully given, false otherwise.
     */
    private bool GiveRainbowBurritoToDragon()
    {
        List<Slot> inventorySlots = _playerInventory.getSlots();

        foreach (Slot slot in inventorySlots)
        {
            if (slot.getType() == CollectableType.RAINBOW_BURRITO)
            {
                slot.RemoveItem(); // Remove one Rainbow Burrito from the inventory
                _playerInventory.OnInventoryChanged?.Invoke();
                // Destroy the highlight object
                if (_highlight != null)
                {
                    Destroy(_highlight);
                }
                return true;
            }
        }
        return false; // Rainbow Burrito not found
    }

    private void AddRequiredIngredientsToStations()
    {
        if (_kegStation != null)
        {
            // Retrieve the keg station's accepted ingredients list
            List<CollectableType> acceptedIngredients = _kegStation.GetAcceptedIngredients();
            // Add required ingredients for rainbow essence
            acceptedIngredients.Add(CollectableType.CARROT);
            acceptedIngredients.Add(CollectableType.PEPPER);
            acceptedIngredients.Add(CollectableType.ONION);
            acceptedIngredients.Add(CollectableType.LETTUCE);
            acceptedIngredients.Add(CollectableType.GRAPES);
            acceptedIngredients.Add(CollectableType.STRAWBERRY);
            
            // set the keg station to accept only these ingredients
            _kegStation.SetAcceptedIngredients(acceptedIngredients);
        }

        if (_cuttingBoard != null)
        {
            // Retrieve the cutting board's accepted ingredients list
            List<CollectableType> acceptedIngredients = _cuttingBoard.GetAcceptedIngredients();
            // Add required ingredients for rainbow burrito
            acceptedIngredients.Add(CollectableType.BREAD);
            acceptedIngredients.Add(CollectableType.TOMATO_SAUCE);
            acceptedIngredients.Add(CollectableType.FISH_AND_SALAD);
            acceptedIngredients.Add(CollectableType.COOKED_STEAK);
            acceptedIngredients.Add(CollectableType.RAINBOW_ESSENCE);
            acceptedIngredients.Add(CollectableType.MILK);
            acceptedIngredients.Add(CollectableType.CHEESE);
            
            // set the cutting board to accept only these ingredients
            _cuttingBoard.SetAcceptedIngredients(acceptedIngredients);
        }
    }
    
    public void TriggerCookingFinale()
    {
        IsActive = true;

        // disable dialogue runner to prevent other dialogues
        if (dialogueRunner != null)
        {
            dialogueRunner.StopAllCoroutines();
            dialogueRunner.enabled = false;
        }
        // Disable AzzyInteraction script
        AzzyInteraction azzyInteraction = _azzyGameObject.GetComponent<AzzyInteraction>();
        if (azzyInteraction != null)
        {
            azzyInteraction.enabled = false;
        }
        // Disable AzzyDialogueTrigger script
        AzzyDialogueTrigger azzyDialogueTrigger = _azzyGameObject.GetComponent<AzzyDialogueTrigger>();
        if (azzyDialogueTrigger != null)
        {
            azzyDialogueTrigger.enabled = false;
        }
        
        
        // Add all the ingredients to the cooking stations AcceptedIngredients
        AddRequiredIngredientsToStations();

        ShowStep(0);
    }
    
    private bool CheckIfFoodAlreadyInInventory(CollectableType foodType)
    {
        List<Slot> inventorySlots = _playerInventory.getSlots();

        foreach (Slot slot in inventorySlots)
        {
            if (slot.getType() == foodType && slot.getCount() > 0)
            {
                return true; // Food item found in inventory
            }
        }

        return false; // Food item not found
    }

    private void ShowStep(int step)
    {
        _currentStep = step;

        switch (step)
        {
            case 0:
                ShowDialogue("You finally gathered all the ingredients for the Rainbow Burrito! Let's prepare it together.", 1);
                break;
            
            case 1:
                ShowDialogue("First, we will need to prepare the rainbow essence using the keg. This one contains the magic of the ground and the seeds.", 2);
                break;
            
            // The player should prepare the rainbow essence
            case 2:
                ShowDialogue("You'll need:\n1 carrot, 1 pepper, 1 onion, 1 lettuce, 1 tomato, 1 grape and 1 strawberry.\nYou must not fail!");
                break;
            
            // The player has collected the rainbow essence
            case 3:
                ShowDialogue("You really are a great cook, I can feel the magic in this essence! Now, let's prepare the Rainbow Burrito.", 4);
                break;
            
            // The player prepares bread
            case 4:
                bool hasBread = CheckIfFoodAlreadyInInventory(CollectableType.BREAD);
                if (hasBread)
                {
                    ShowDialogue("You already have the bread prepared! Great job! Now, let's move on to the next ingredient.", 5);
                    break;
                }
                ShowDialogue("It really is a complex recipe. First we will need some bread, prepare it using the wheat. If you ever forget the recipe, just look at the book by pressing 'B'.");
                break;
            
            // The player prepares tomato sauce
            case 5:
                bool hasTomatoSauce = CheckIfFoodAlreadyInInventory(CollectableType.TOMATO_SAUCE);
                if (hasTomatoSauce)
                { 
                    ShowDialogue("I see, you already have the tomato sauce ready! Excellent!", 6);
                    break;
                }
                ShowDialogue("Great! Now, let's prepare the tomato sauce using the tomatoes. This will add a nice flavor to our burrito.");
                break;
            
            // The player prepares the fish with salad
            case 6:
                bool hasFishAndSalad = CheckIfFoodAlreadyInInventory(CollectableType.FISH_AND_SALAD);
                if (hasFishAndSalad)
                {
                    ShowDialogue("You already have the fish with salad prepared! The burrito will be refreshing!", 7);
                    break;
                }
                ShowDialogue("Now, let's prepare the fish with some fresh salad. This will give our burrito a refreshing taste.");
                break;
            
            // The player prepares the steak
            case 7:
                bool hasSteak = CheckIfFoodAlreadyInInventory(CollectableType.COOKED_STEAK);
                if (hasSteak)
                {
                    ShowDialogue("You already have the steak cooked! Perfect! We have all the ingredients we need.", 8);
                    break;
                }
                ShowDialogue("Finally, let's cook the steak to perfection. This will be the main protein of our Rainbow Burrito.");
                break;
            
            // The player now has all the ingredients
            case 8:
                bool hasRainbowBurrito = CheckIfFoodAlreadyInInventory(CollectableType.RAINBOW_BURRITO);
                if (hasRainbowBurrito)
                {
                    ShowDialogue("You already have the Rainbow Burrito prepared! You really are incredible my friend!", 10);
                    break;
                }
                ShowDialogue("Now that we have all the flavors ready, let's assemble the Rainbow Burrito together! Go to the cutting board, I can't wait...", 9);
                break;
            
            // preparing the burrito
            case 9:
                ShowDialogue("You must not fail!\n1 loaf of bread, 1 tomato sauce, 1 fish with salad, 1 steak, 1 bottle of milk, 1 block of cheese and 1 rainbow essence.");
                break;
            
            // the player has collected the burrito
            case 10:
                ShowDialogue("The Rainbow Burrito is ready! Give it to me, I really hope it works...", 11);
                break;
            
            case 11:
                StartCoroutine(FadeAndMoveAzzyCoroutine(_azzyGameObject, azzyTargetPosition, _fadeDuration));
                _azzyWaitingForBurrito = true;
                break;
            
            case 12:
                StartCoroutine(TransformDragonToHuman());
                break;
                
            // the dragon has transformed
            case 13:
                ShowDialogue("OH MY GOODNESS! I AM NOT A DRAGON ANYMORE! THANK YOU SO MUCH!", 14);
                break;
            
            case 14:
                ShowDialogue("I can finally live my life as a human again, all thanks to you, I really owe you my life!", 15);
                break;
            
            case 15:
                ShowDialogue("You really are a fantastic cook and an even better friend.", 16);
                break;
            
            case 16:
                ShowDialogue("I've thought about it, and I would love to thank you by gifting you my tavern. It's the least I can do after all you've done for me.", 17);
                break;
            
            case 17:
                ShowDialogue("Thank you my friend.", -2);
                break;
            
            // the player has failed cooking something and the finale will be postponed to another time
            case 99:
                ShowDialogue("Nooooo, you failed! The Rainbow Burrito is ruined... I'm so sorry, but I guess the curse remains for now. We'll have to try again another time.", -3);
                break;
                
        }
    }
    private IEnumerator FadeAndMoveAzzyCoroutine(GameObject azzy, Vector3 azzyTargetPosition, float fadeDuration)
    {
        // Create a temporary canvas for the fade effect
        GameObject canvasObj = new GameObject("TemporaryFadeCanvas");
        Canvas fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 30000;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create an image for the fade effect
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);
        Image fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        // Stretch the image to fill the screen
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Fade in
        yield return FadeImage(fadeImage, 0f, 1f, fadeDuration);

        // Move Azzy
        azzy.transform.position = azzyTargetPosition;

        // Fade out
        yield return FadeImage(fadeImage, 1f, 0f, fadeDuration);

        // Destroy the canvas
        Destroy(canvasObj);
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = image.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            image.color = color;
            yield return null;
        }

        color.a = endAlpha;
        image.color = color;
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
            
            // Set the appropriate portrait based on Azzy's current form
            Sprite currentPortrait = _azzyIsHuman ? humanPortrait : dragonPortrait;
            dialogueUI.SetPortrait(currentPortrait);

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
        if (resultType == CollectableType.BURNT)
        {
            FailedFinale();
        }
        else if (resultType == CollectableType.RAINBOW_ESSENCE)
        {
            OnCookingComplete(3);
        }
        else if (resultType == CollectableType.BREAD)
        {
            OnCookingComplete(5);
        }
        else if (resultType == CollectableType.TOMATO_SAUCE)
        {
            OnCookingComplete(6);
        }
        else if (resultType == CollectableType.FISH_AND_SALAD)
        {
            OnCookingComplete(7);
        }
        else if (resultType == CollectableType.COOKED_STEAK)
        {
            OnCookingComplete(8);
        }
        else if (resultType == CollectableType.RAINBOW_BURRITO)
        {
            OnCookingComplete(10);
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

        ShowStep(99);
    }

    private IEnumerator TransformDragonToHuman()
    {
        // Stop the background music
        if (audioManager != null)
        {
            audioManager.StopBackgroundMusic();
        }
        
        // Instantiate the prefab at Azzy's position
        GameObject circleObj = Instantiate(circlePrefab, _azzyGameObject.transform.position, Quaternion.identity);

        // Get the Transform for scaling
        Transform circleTransform = circleObj.transform;

        // Set initial scale to (0, 0)
        circleTransform.localScale = Vector3.zero;

        // Scale up to (2, 2)
        yield return ScaleOverTime(circleTransform, Vector3.zero, new Vector3(2f, 2f, 1f), 0.5f);

        // Replace Azzy with the new prefab
        GameObject newAzzy = Instantiate(azzyHumanPrefab, _azzyGameObject.transform.position + new Vector3(0, -0.5f, 0), Quaternion.identity);
        Destroy(_azzyGameObject);
        _azzyGameObject = newAzzy;
        
        // delete the highlight object if it exists
        Transform highlightTransform = _azzyGameObject.transform.Find("Highlight");
        if (highlightTransform != null)
        {
            Destroy(highlightTransform.gameObject);
        }
        
        // Wait for 1 second
        yield return new WaitForSeconds(2f);

        // Scale back down to (0, 0)
        yield return ScaleOverTime(circleTransform, new Vector3(2f, 2f, 1f), Vector3.zero, 0.5f);

        AzzyScript azzy = FindFirstObjectByType<AzzyScript>();

            if (azzy != null)
            {
                azzy.ConverToHuman();
            }

        // Mark Azzy as human so portrait switches
        _azzyIsHuman = true;

        // Destroy the circle object
        Destroy(circleObj);
        
        // Play the happy ending music
        if (audioManager != null)
        {
            audioManager.PlayFinaleHappyEndingMusic();
        }

        // Proceed to the next step
        ShowStep(13);
    }
    
    private IEnumerator ScaleOverTime(Transform transform, Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = endScale;
    }
    
    private void ShowSuccessScreen()
    {
        if (successScreenPrefab != null)
        {
            InstantiateAndShowScreen(successScreenPrefab);
        }
        else
        {
            Debug.LogWarning("Success screen prefab is not assigned.");
        }
    }

    private void ShowFailureScreen()
    {
        if (failureScreenPrefab != null)
        {
            InstantiateAndShowScreen(failureScreenPrefab);
        }
        else
        {
            Debug.LogWarning("Failure screen prefab is not assigned.");
        }
    }
/*
    private void InstantiateAndShowScreen(GameObject screenPrefab)
    {
        GameObject screenInstance = Instantiate(screenPrefab);
        EndOfDayUI endOfDayUI = screenInstance.GetComponent<EndOfDayUI>();

        if (endOfDayUI != null)
        {
            endOfDayUI.ShowEndOfDayScreen(null, 1.5f); // Adjust duration as needed
        }
        else
        {
            Debug.LogError("The instantiated screen does not have an EndOfDayUI component.");
        }
    }*/
    
    private void InstantiateAndShowScreen(GameObject screenPrefab)
    {
        GameObject uiCanvas = GameObject.Find("UI");
        if (uiCanvas == null)
        {
            Debug.LogError("No Canvas named 'UI' found in the scene. Please ensure a Canvas named 'UI' exists.");
            return;
        }

        GameObject screenInstance = Instantiate(screenPrefab, uiCanvas.transform);
        if (!screenInstance.activeSelf)
        {
            screenInstance.SetActive(true);
        }

        foreach (Transform child in screenInstance.transform)
        {
            child.gameObject.SetActive(true);
        }

        EndOfDayUI endOfDayUI = screenInstance.GetComponent<EndOfDayUI>();
        if (endOfDayUI != null)
        {
            endOfDayUI.ShowEndOfDayScreen(null, 1.5f);
        }
        else
        {
            Debug.LogError($"[GameFinaleCookingManager] The instantiated screen {screenInstance.name} does not have an EndOfDayUI component.");
        }
    }


    
    private void CompleteFinale()
    {
        ShowSuccessScreen();
        
        // Mark finale as completed
        PlayerPrefs.SetInt(_FINALE_COMPLETED_KEY, 1);
        PlayerPrefs.Save();
    }
    
    private void FailedFinale()
    {
        ShowFailureScreen();
    }
    
    #if UNITY_EDITOR
        /*
         * Editor menu item to display current tutorial key value.
         */
        [UnityEditor.MenuItem("Tools/Finale/Show Cooking Finale Key")]
        public static void ShowTutorialKey()
        {
            int value = PlayerPrefs.GetInt(_FINALE_COMPLETED_KEY, 0);
            Debug.Log($"{_FINALE_COMPLETED_KEY} = {value} (0=not done, 1=completed)");
        }

        /*
         * Editor menu item to reset only the tutorial PlayerPrefs key.
         */
        [UnityEditor.MenuItem("Tools/Finale/Reset Cooking Finale Key Only")]
        public static void ResetTutorialKeyOnly()
        {
            PlayerPrefs.DeleteKey(_FINALE_COMPLETED_KEY);
            PlayerPrefs.Save();
            Debug.Log("Cooking Finale key deleted. Reload scene to replay cooking finale.");
        }
    #endif
}
