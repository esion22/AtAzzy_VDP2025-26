using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CookingStations;
using Orders;
using TMPro;
using PixelAdventures.Scripts.Completed_Scripts;
using Tutorial;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class AbstractCookingStation : MonoBehaviour, ISpellInteractable
{
    // Ingredients and food attributes
    [Header("Ingredient settings")]
    [SerializeField] protected List<CollectableType> acceptedIngredients = new List<CollectableType>();
    protected ArrayList CookingSlots = new ArrayList();
    [SerializeField] protected List<Recipe> CookingRecipes = new List<Recipe>();
    // protected Dictionary<ArrayList, CollectableType> CookingRecipes = new Dictionary<ArrayList, CollectableType>();
    protected CollectableType OutputSlot = CollectableType.NONE;
    
    // Cooking bar attributes
    [Header("Cooking bar settings")]
    [SerializeField] protected int doneThreshold;
    [SerializeField] protected bool isDiscrete;
    protected float CookingBarValue;
    protected float CurrentVelocity;
    [SerializeField] protected float cookingVelocity;
    [SerializeField] protected float cookingDeceleration;
    [SerializeField] protected float minNegativeVelocity;
    
    // Cooking station attributes
    [Header("Cooking station settings")] 
    protected CookingStationStateEnum CurrentState;
    [FormerlySerializedAs("SpellsNeededToWork")] [SerializeField] protected List<SpellEnum> spellsNeededToWork;
    protected ArrayList _spellHistory = new ArrayList();
    
    // UI attributes
    [FormerlySerializedAs("CookingBarSlider")]
    [Header("UI settings")] 
    [SerializeField] protected Slider cookingBarSlider;
    [SerializeField] protected GameObject itemCloud;
    private Image _itemCloudImage;
    [SerializeField] protected Image itemSpriteImage;
    [SerializeField] protected TextMeshProUGUI itemQuantityText;
    [SerializeField] protected float spriteRotationSpeed = 1.5f;

    protected ItemManager itemManager;
    private Coroutine spriteRotationCoroutine;
    protected AudioManager audioManager;
    protected RecipeTracker recipeTracker;

    public System.Action<CollectableType> OnRecipeCompleted;
    public System.Action OnCookingFailed;
    public System.Action<CollectableType> OnIngredientPlaced;
    public System.Action<SpellEnum> OnSpellReceived;
    protected CookingTutorial _cookingTutorial;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        itemManager = ItemManager.Instance;
        _cookingTutorial = CookingTutorial.Instance;
     
        // Find the RecipeTracker in the scene
        recipeTracker = FindObjectOfType<RecipeTracker>();
        if (recipeTracker == null)
        {
            Debug.LogError("RecipeTracker not found in the scene. Ensure it is added to a GameObject.");
            return;
        }
        
        if (itemCloud != null)
        {
            itemCloud.SetActive(false);
            itemQuantityText.text = "";
        }
        
        CookingBarValue = 0f;
        CurrentState = CookingStationStateEnum.RESTING;
            
        if (cookingBarSlider != null)
        {
            cookingBarSlider.minValue = 0;
            cookingBarSlider.maxValue = doneThreshold;
            cookingBarSlider.value = 0;
            cookingBarSlider.gameObject.SetActive(false);
        }
            
        if (cookingVelocity == 0 && cookingDeceleration == 0 && minNegativeVelocity == 0)
        {
            cookingVelocity = 1f;
            cookingDeceleration = 1f;
            minNegativeVelocity = -1f;
        }
        
        // get the item cloud sprite image
        if (itemCloud != null)
        {
            _itemCloudImage = itemCloud.GetComponent<Image>();
        }
        
        // Filter the CookingRecipes to include only unlocked recipes
        FilterUnlockedRecipes();
    }
    
    /*
     * Filters the CookingRecipes list to include only recipes that are unlocked in the RecipeTracker
     */
    private void FilterUnlockedRecipes()
    {
        if (recipeTracker != null)
        {
            var unlockedRecipes = recipeTracker.GetUnlockedRecipes();
            CookingRecipes = CookingRecipes.Intersect(unlockedRecipes).ToList();
        }
    }
    
    /*
     * Sets the accepted ingredients for this cooking station
     * @param ingredients The list of accepted ingredient types
     */
    public void SetAcceptedIngredients(List<CollectableType> ingredients)
    {
        acceptedIngredients = ingredients;
    }

    /*
     * Gets the accepted ingredients for this cooking station
     * @return The list of accepted ingredient types
     */
    public List<CollectableType> GetAcceptedIngredients()
    { 
        // give a copy of the accepted ingredients list
        return new List<CollectableType>(acceptedIngredients);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // update the cooking bar if in cooking state
        if (CurrentState == CookingStationStateEnum.COOKING)
        {
            UpdateCookingBar();
            CookingReady();
            if (cookingBarSlider != null)
            {
                cookingBarSlider.value = CookingBarValue;
            }
        }
    }
    
    /*
     * Attempts to add an ingredient from the player's inventory to the cooking station
     * @param player The player item controller
     */
    public virtual void TryToAddIngredient(PlayerItemController player)
    {
        if (CurrentState == CookingStationStateEnum.COOKING || CurrentState == CookingStationStateEnum.DONE)
        {
            return;
        }
        var playerInventory = player.GetInventory();
        if (playerInventory == null) return;
        
        // get the item in the quick inventory slot
        Slot item = playerInventory.getSelection();
        
        if (item != null && acceptedIngredients.Contains(item.getType()) && (item.getCategory() == CollectableCategory.INGREDIENT || item.getCategory() == CollectableCategory.FOOD))
        {
            CollectableType ingredientType = item.getType();
            if (CurrentState == CookingStationStateEnum.RESTING)
            {
                CurrentState = CookingStationStateEnum.READY;
            }
            CookingSlots.Add(item.getType());
            // find the index of the item in the player's inventory and remove it
            int itemIndex = playerInventory.findSlotIndex(item);
            if (itemIndex == -1)
            { 
                Debug.LogWarning("Could not find slot index for item " + item.getType());
            }
            playerInventory.Remove(itemIndex);
            
            UpdateVisualFeedback();
            
            // invoke the ingredient placed event
            OnIngredientPlaced?.Invoke(ingredientType);
        }
    }
    
    /*
     * Called when a spell hits the cooking station
     * @param spellType The type of spell that hit the cooking station
     */
    public virtual void OnSpellHit(SpellEnum spellType) {}
    
    /*
     * Updates the cooking bar based on the current state and velocity
     */
    public virtual void UpdateCookingBar() {}
    
    /*
     * Updates the cooking bar in discrete steps
     */
    public virtual void UpdateCookingBarDiscrete()
    {
        if (isDiscrete)
        {
            // update the cooking bar in discrete steps
            if (CurrentState == CookingStationStateEnum.COOKING)
            {
                // increase the cooking bar value by a fixed amount
                CookingBarValue += 1;

                // if it is greater than or equal to done threshold, clamp it to done threshold
                if (CookingBarValue >= doneThreshold)
                {
                    CookingBarValue = doneThreshold;
                }

                if (cookingBarSlider != null)
                {
                    cookingBarSlider.value = CookingBarValue;
                }
            }
        }
    }

    /*
     * Sets an ingredient in the cooking slot
     * @param ingredient The ingredient to add to the cooking slot
     */
    public void SetCookingSlot(CollectableCategory ingredient)
    {
        // add the ingredient to the cooking slot
        CookingSlots.Add(ingredient);
    }

    /*
     * Gets the output slot of the cooking station
     * @return The collectable type in the output slot
     */
    public CollectableType GetOutputSlot()
    {
        return OutputSlot;
    }
    
    /*
     * Resets the output slot of the cooking station to NONE
     */
    public void ResetOutputSlot()
    {
        OutputSlot = CollectableType.NONE;
        ResetStation();
    }

    /*
     * Resets the cooking station to its initial state
     */
    protected virtual void ResetStation()
    {
        CurrentState = CookingStationStateEnum.RESTING;
        CookingBarValue = 0f;
        CookingSlots.Clear();
        UpdateVisualFeedback();
        // reset the spell history
        _spellHistory.Clear();
    }
    
    /*
     * Called when cooking is ready
     */
    protected virtual void CookingReady() {}


    /*
     * Checks the current ingredients against the cooking recipes
     * @param givenIngredients The list of ingredients to check
     * @return The resulting collectable type if a recipe matches, otherwise NONE
     */
    protected CollectableType CheckRecipes(ArrayList givenIngredients)
    {
        // Convert the given ingredients into a dictionary that counts occurrences.
        var givenCounts = new Dictionary<CollectableType, int>();
        foreach (CollectableType ingredient in givenIngredients)
        {
            if (givenCounts.ContainsKey(ingredient))
                givenCounts[ingredient]++;
            else
                givenCounts[ingredient] = 1;
        }
        
        foreach (var recipe in CookingRecipes)
        {
            // Skip recipes with different ingredient counts
            int sumAmountNeeded = 0;
            foreach (int amount in recipe.amountNeeded)
            {
                sumAmountNeeded += amount;
            }
            if (sumAmountNeeded != givenIngredients.Count) continue;

            // Build recipeCounts from the parallel lists ingredientsNeeded and amountNeeded
            var recipeCounts = new Dictionary<CollectableType, int>();
            int n = Mathf.Min(recipe.ingredientsNeeded.Count, recipe.amountNeeded.Count);
            for (int i = 0; i < n; i++)
            {
                var ing = recipe.ingredientsNeeded[i];
                var amt = recipe.amountNeeded[i];
                if (recipeCounts.ContainsKey(ing))
                    recipeCounts[ing] += amt;
                else
                    recipeCounts[ing] = amt;
            }
            
            // Check if the ingredient counts match exactly
            bool match = recipeCounts.Count == givenCounts.Count &&
                         recipeCounts.All(kv => givenCounts.TryGetValue(kv.Key, out var v) && v == kv.Value);

            if (match)
            {
                // If all counts match, return the resulting collectable type.
                return recipe.foodType;
            }
        }

        // If no recipe matched, return NONE.
        return CollectableType.TRASH;
    }


    /*
     * Updates the visual feedback of the cooking station based on its state
     */
    protected void UpdateVisualFeedback()
    {
        if (itemCloud == null || itemSpriteImage == null || itemManager == null)
            return;

        if (spriteRotationCoroutine != null)
        {
            StopCoroutine(spriteRotationCoroutine);
            spriteRotationCoroutine = null;
        }
        
        var itemsToShow = new Dictionary<CollectableType, int>();

        if (CurrentState == CookingStationStateEnum.DONE && OutputSlot != CollectableType.NONE)
        {
            itemsToShow.Add(OutputSlot, 1);
        }
        else if (CookingSlots.Count > 0)
        {
            foreach (CollectableType type in CookingSlots)
            {
                if (itemsToShow.ContainsKey(type))
                    itemsToShow[type]++;
                else
                    itemsToShow[type] = 1;
            }
        }
        
        if (itemsToShow.Count > 0)
        {
            itemCloud.SetActive(true);
            
            if (CurrentState != CookingStationStateEnum.DONE)
            {
                CollectableType result = CheckRecipes(CookingSlots);
                if (result != CollectableType.TRASH && result != CollectableType.NONE)
                {
                    // change the color to #60FF72
                    _itemCloudImage.color = new Color(0.38f, 1f, 0.45f); // recipe valid
                }
                else
                {
                    // change the color to #FF9E9E
                    _itemCloudImage.color = new Color(1f, 0.62f, 0.62f); // recipe invalid
                }
            }
            else
            {
                _itemCloudImage.color = Color.white; // default color
            }
            
            // Start rotating through the sprites
            spriteRotationCoroutine = StartCoroutine(RotateSprites(itemsToShow));
        }
        else
        {
            itemCloud.SetActive(false);
            _itemCloudImage.color = Color.white;
        }
    }
    
    /*
     * Coroutine to rotate through a list of sprites at a set speed
     * @param sprites The list of sprites to rotate through
     */
    private IEnumerator RotateSprites(Dictionary<CollectableType, int> items)
    {
        var itemList = items.Keys.ToList();
        int currentIndex = 0;
        while (true)
        {
            if (itemList.Count > 0)
            {
                CollectableType currentType = itemList[currentIndex];
                int currentCount = items[currentType];

                string keyFood = CollectableCategory.FOOD.ToString() + " - " + currentType.ToString();
                string keyIngredient = CollectableCategory.INGREDIENT.ToString() + " - " + currentType.ToString();

                // Try to get prefab
                Collectable itemPrefab = itemManager.GetItemByCategoryType(keyFood);
                if (itemPrefab == null)
                {
                    itemPrefab = itemManager.GetItemByCategoryType(keyIngredient);
                }

                if (itemPrefab != null)
                {
                    itemSpriteImage.sprite = itemPrefab.getIcon();
                }
                else
                {
                    // Log useful for debugging
                    Debug.LogWarning(
                        $"RotateSprites: prefab non trovato per '{currentType}'. Keys provate: '{keyFood}', '{keyIngredient}'");

                    // zero out the sprite if prefab not found
                    if (itemSpriteImage != null)
                        itemSpriteImage.sprite = null;
                }

                if (itemQuantityText != null)
                {
                    itemQuantityText.text = currentCount > 1 ? currentCount.ToString() : "";
                }

                currentIndex = (currentIndex + 1) % itemList.Count;
            }

            yield return new WaitForSeconds(spriteRotationSpeed);
        }
    }
    
/*
     * Retrieves all ingredients from the cooking station if not yet cooking
     * @param player The player item controller to return ingredients to
     * @returns True if ingredients were retrieved, false otherwise
     */
    public virtual bool TryRetrieveIngredients(PlayerItemController player)
    {
        // Can only retrieve if not in DONE state
        if (CurrentState == CookingStationStateEnum.DONE)
        {
            return false;
        }
        
        if (CookingSlots.Count == 0)
        {
            return false;
        }
        
        var playerInventory = player.GetInventory();
        if (playerInventory == null) return false;
        
        // Add each ingredient back to the player's inventory
        foreach (CollectableType ingredientType in CookingSlots)
        {
            // Try INGREDIENT category first, then FOOD
            string keyIngredient = CollectableCategory.INGREDIENT.ToString() + " - " + ingredientType.ToString();
            string keyFood = CollectableCategory.FOOD.ToString() + " - " + ingredientType.ToString();
            
            Collectable itemPrefab = itemManager.GetItemByCategoryType(keyIngredient);
            if (itemPrefab == null)
            {
                itemPrefab = itemManager.GetItemByCategoryType(keyFood);
            }
            
            if (itemPrefab != null)
            {
                playerInventory.Add(itemPrefab);
            }
            else
            {
                Debug.LogWarning($"Could not find prefab for ingredient: {ingredientType}");
            }
        }
        
        // Hide and reset the cooking bar slider
        if (cookingBarSlider != null)
        {
            cookingBarSlider.gameObject.SetActive(false);
            cookingBarSlider.value = 0f;
        }
        
        // Reset the station
        ResetStation();
        
        return true;
    }
    
}