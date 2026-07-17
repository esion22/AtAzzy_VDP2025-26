using System.Collections.Generic;
using System.Linq;
using Orders;
using UnityEngine;
using System.Reflection;

public class MenuManager : MonoBehaviour
{
    [Header("Pages")] 
    [SerializeField] private Transform leftPageGrid;
    [SerializeField] private RightPageUI rightPageUI;

    [SerializeField] private RecipeIconUI recipeIconPrefab;
    
    [Header("Background Sprites - Normal")]
    [SerializeField] private Sprite leftEdgeBackground;
    [SerializeField] private Sprite middleBackgroundVariantA;
    [SerializeField] private Sprite middleBackgroundVariantB;
    [SerializeField] private Sprite rightEdgeBackground;
    
    [Header("Background Sprites - Clicked")]
    [SerializeField] private Sprite leftEdgeBackgroundClicked;
    [SerializeField] private Sprite middleBackgroundVariantAClicked;
    [SerializeField] private Sprite middleBackgroundVariantBClicked;
    [SerializeField] private Sprite rightEdgeBackgroundClicked;
    
    [SerializeField] private int columnsPerRow = 6;

    private OrderManager orderManager;
    private MenuPanelUI menuPanelUI;

    private Dictionary<CollectableType, RecipeIconUI> iconLookup = new();
    private readonly HashSet<RecipeIconUI> selectedIcons = new();
    private RecipeIconUI lastClickedIcon;
    private bool isPrototype;

    private void Awake()
    {
        orderManager = FindFirstObjectByType<OrderManager>();
        menuPanelUI = FindFirstObjectByType<MenuPanelUI>();

        if (leftPageGrid == null)
        {
            Debug.LogError("leftPageGrid is not assigned in MenuManager");
        }

        if (rightPageUI == null)
        {
            Debug.LogError("rightPageUI is not assigned in MenuManager");
        }

        if (recipeIconPrefab == null)
        {
            Debug.LogError("recipeIconPrefab not assigned in MenuManager");
        }

        isPrototype = IsPrototype();
        
        ClearRightPage();
        PopulateLeftPage();
    }

    /// <summary>
    /// Populates the left page with recipe icons
    /// </summary>
    private void PopulateLeftPage()
    {
        foreach (Transform child in leftPageGrid)
        {
            Destroy(child.gameObject);
        }

        iconLookup.Clear();
        selectedIcons.Clear();

        List<Recipe> recipes = RecipeTracker.Instance.GetUnlockedRecipes()
            .Where(r => ItemManager.Instance.collectableDictionary.ContainsKey("FOOD - " + r.foodType))
            .ToList();

        if (recipes == null || recipes.Count == 0)
        {
            Debug.LogWarning("No recipes found");
            return;
        }

        int i = 0;
        foreach (var recipe in recipes)
        {
            var icon = Instantiate(recipeIconPrefab, leftPageGrid);
            
            bool canCook = isPrototype || CanCookRecipe(recipe);
            
            icon.Init(recipe, canCook, this);
            
            var backgrounds = GetBackgroundsForIndex(i);
            icon.SetBackgrounds(backgrounds.normal, backgrounds.clicked);
            
            iconLookup.Add(recipe.foodType, icon);
            i++;
        }
    }
    
    /// <summary>
    /// Determines the appropriate background sprites for a given index
    /// </summary>
    private (Sprite normal, Sprite clicked) GetBackgroundsForIndex(int index)
    {
        int column = index % columnsPerRow;

        if (column == 0)
        {
            return (leftEdgeBackground, leftEdgeBackgroundClicked);
        }

        if (column == columnsPerRow - 1)
        {
            return (rightEdgeBackground, rightEdgeBackgroundClicked);
        }
        
        bool useA = column % 2 == 1;
        return useA 
            ? (middleBackgroundVariantA, middleBackgroundVariantAClicked) 
            : (middleBackgroundVariantB, middleBackgroundVariantBClicked);
    }

    /// <summary>
    /// Called when a recipe icon is clicked (called from RecipeIconUI)
    /// </summary>
    public void OnRecipeIconClicked(Recipe recipe, RecipeIconUI icon)
    {
        if (!icon.IsInteractable())
        {
            return;
        }

        if (selectedIcons.Contains(icon))
        {
            // Deselect
            selectedIcons.Remove(icon);
            icon.SetSelected(false);
            
            if (lastClickedIcon == icon)
            {
                lastClickedIcon = null;
                ClearRightPage();
            }
        }
        else
        {
            // Select
            selectedIcons.Add(icon);
            icon.SetSelected(true);
            lastClickedIcon = icon;
            
            // Show recipe details on right page
            rightPageUI.DisplayRecipe(recipe);
        }
    }

    /// <summary>
    /// Clears the right page display
    /// </summary>
    private void ClearRightPage()
    {
        if (rightPageUI != null && rightPageUI.recipeIconImage != null)
        {
            rightPageUI.recipeIconImage.enabled = false;
            rightPageUI.recipePrice.SetActive(false);
        }
        
        if (rightPageUI != null && rightPageUI.formulaParent != null)
        {
            foreach (Transform child in rightPageUI.formulaParent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Clears all selected recipe icons
    /// </summary>
    public void DeselectAll()
    {
        foreach (var icon in selectedIcons)
        {
            icon.SetSelected(false);
        }

        selectedIcons.Clear();
        lastClickedIcon = null;
        ClearRightPage();
    }

    /// <summary>
    /// Selects all available recipe icons
    /// </summary>
    public void SelectAll()
    {
        selectedIcons.Clear();
        
        foreach (var kvp in iconLookup)
        {
            var icon = kvp.Value;
            if (!icon.IsInteractable())
                continue;

            selectedIcons.Add(icon);
            icon.SetSelected(true);
        }

        lastClickedIcon = null;
        ClearRightPage();
    }

    /// <summary>
    /// Confirms selected recipes and updates OrderManager
    /// </summary>
    public void ConfirmSelection()
    {
        if (selectedIcons.Count == 0)
        {
            menuPanelUI?.CloseMenu();
            EndOfDayManager.Instance.TriggerEndOfDay();
            return;
        }

        List<Recipe> selected = new();
        foreach (var icon in selectedIcons)
        {
            selected.Add(icon.GetRecipe());
        }

        string fieldName = isPrototype ? "prototypeRecipes" : "recipes";
        FieldInfo field = typeof(OrderManager)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        if (field == null)
            return;

        var recipeList = field.GetValue(orderManager) as List<Recipe>;
        recipeList.Clear();
        recipeList.AddRange(selected);

        orderManager.StartOrderGeneration();

        DeselectAll();

        Inventory playerInventory = FindFirstObjectByType<PlayerItemController>().GetInventory();
        playerInventory.FilterInventoryForRecipes(selected, FindObjectOfType<RecipeTracker>());

        menuPanelUI?.CloseMenu();
    }

    private bool IsPrototype()
    {
        FieldInfo field = typeof(OrderManager)
            .GetField("isPrototype", BindingFlags.NonPublic | BindingFlags.Instance);

        return field != null && (bool)field.GetValue(orderManager);
    }

    private bool CanCookRecipe(Recipe recipe)
    {
        if (!RecipeTracker.Instance.IsFoodRecipe(recipe.foodType))
        {
            return false;
        }

        Inventory inv = FindFirstObjectByType<PlayerItemController>().GetInventory();
        Dictionary<CollectableType, int> owned = new();

        foreach (Slot s in inv.getSlots())
        {
            if (s.getCategory() == CollectableCategory.INGREDIENT)
                owned[s.getType()] = s.getCount();
        }

        Dictionary<CollectableType, int> needed =
            Utils.GetIngredientList(recipe.ingredientsNeeded, recipe.amountNeeded);

        return CanCookWithIngredients(owned, needed);
    }

    private bool CanCookWithIngredients(
        Dictionary<CollectableType, int> available, 
        Dictionary<CollectableType, int> needed)
    {
        Dictionary<CollectableType, int> workingInventory = new(available);

        foreach (var (ingredientType, amountNeeded) in needed)
        {
            int amountOwned = workingInventory.GetValueOrDefault(ingredientType, 0);

            if (amountOwned >= amountNeeded)
            {
                workingInventory[ingredientType] -= amountNeeded;
                continue;
            }

            Recipe subRecipe = RecipeTracker.Instance.GetUnlockedRecipe(ingredientType);
            
            if (subRecipe != null)
            {
                Dictionary<CollectableType, int> subRecipeNeeds =
                    Utils.GetIngredientList(subRecipe.ingredientsNeeded, subRecipe.amountNeeded);

                int craftCount = amountNeeded - amountOwned;

                Dictionary<CollectableType, int> totalSubNeeds = new();
                foreach (var (subType, subAmount) in subRecipeNeeds)
                {
                    totalSubNeeds[subType] = subAmount * craftCount;
                }

                if (CanCookWithIngredients(workingInventory, totalSubNeeds))
                {
                    foreach (var (subType, subAmount) in totalSubNeeds)
                    {
                        workingInventory[subType] = 
                            workingInventory.GetValueOrDefault(subType, 0) - subAmount;
                    }
                    
                    workingInventory[ingredientType] = 
                        workingInventory.GetValueOrDefault(ingredientType, 0) + craftCount;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        
        return true;
    }
}