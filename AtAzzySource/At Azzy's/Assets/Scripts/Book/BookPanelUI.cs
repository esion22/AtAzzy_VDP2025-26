using System.Collections.Generic;
using Orders;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the book UI panel
/// </summary>
public class BookPanelUI : MonoBehaviour
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
    
    private Dictionary<CollectableType, RecipeIconUI> iconLookup = new();
    private RecipeIconUI currentlySelected;

    private void Awake()
    {
        gameObject.SetActive(false);

        if (leftPageGrid == null)
        {
            Debug.LogError("leftPageGrid is not assigned in BookPanelUI");
        }

        if (RecipeTracker.Instance == null)
        {
            Debug.LogError("RecipeTracker instance not found");
            return;
        }
        
        if (recipeIconPrefab == null)
            Debug.LogError("recipeIconPrefab not assigned");

        PopulateLeftPage();
    }

    /// <summary>
    /// Subscribes to recipe unlock events
    /// </summary>
    void OnEnable()
    {
        RecipeTracker.Instance.OnRecipeUnlocked += HandleRecipeUnlocked;
    }

    /// <summary>
    /// Unsubscribes from recipe unlock events
    /// </summary>
    void OnDisable()
    {
        RecipeTracker.Instance.OnRecipeUnlocked -= HandleRecipeUnlocked;
    }

    /// <summary>
    /// Handles a recipe being unlocked
    /// </summary>
    /// <param name="recipeType">The type of the recipe that was unlocked</param>
    void HandleRecipeUnlocked(CollectableType recipeType)
    {
        if (iconLookup.TryGetValue(recipeType, out var icon))
        {
            icon.SetUnlocked(true);
        }
    }

    /// <summary>
    /// Populates the left page with recipe icons
    /// </summary>
    void PopulateLeftPage()
    {
        foreach (Transform child in leftPageGrid)
        {
            Destroy(child.gameObject);
        }

        iconLookup.Clear();

        int i = 0;
        foreach (var recipe in RecipeTracker.Instance.GetAllRecipes())
        {
            var icon = Instantiate(recipeIconPrefab, leftPageGrid);
            bool unlocked = RecipeTracker.Instance.IsUnlocked(recipe);

            icon.Init(recipe, unlocked, this);
            
            var backgrounds = GetBackgroundsForIndex(i);
            icon.SetBackgrounds(backgrounds.normal, backgrounds.clicked);
            
            iconLookup.Add(recipe.foodType, icon);
            i++;
        }
    }
    
    /// <summary>
    /// Determines the appropriate background sprites for a given index
    /// </summary>
    /// <param name="index">The index of the icon</param>
    /// <returns>A tuple containing the normal and clicked background sprites</returns>
    private (Sprite normal, Sprite clicked) GetBackgroundsForIndex(int index)
    {
        int column = index % columnsPerRow;

        // Left edge
        if (column == 0)
        {
            return (leftEdgeBackground, leftEdgeBackgroundClicked);
        }

        // Right edge
        if (column == columnsPerRow - 1)
        {
            return (rightEdgeBackground, rightEdgeBackgroundClicked);
        }
        
        // Middle tiles: alternate
        bool useA = column % 2 == 1;
        return useA 
            ? (middleBackgroundVariantA, middleBackgroundVariantAClicked) 
            : (middleBackgroundVariantB, middleBackgroundVariantBClicked);
    }

    /// <summary>
    /// Shows the recipe on the right page
    /// </summary>
    /// <param name="recipe">The recipe to show</param>
    public void ShowRecipe(Recipe recipe)
    {
        if (recipe == null)
            return;

        // Deselect previous
        if (currentlySelected != null)
        {
            currentlySelected.SetSelected(false);
        }

        // Select new
        if (iconLookup.TryGetValue(recipe.foodType, out var icon))
        {
            icon.SetSelected(true);
            currentlySelected = icon;
        }

        rightPageUI.DisplayRecipe(recipe);
    }

    /// <summary>
    /// Opens the book
    /// </summary>
    public void OpenBook()
    {
        if (Time.timeScale != 0)
        {
            PopulateLeftPage();
            gameObject.SetActive(true);
            Time.timeScale = 0;
        }
    }

    /// <summary>
    /// Closes the book
    /// </summary>
    public void CloseBook()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }
}