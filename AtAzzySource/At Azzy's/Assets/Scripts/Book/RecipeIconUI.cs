using Orders;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for displaying a recipe icon (works for both BookPanelUI and MenuManager).
/// </summary>
public class RecipeIconUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    private Recipe recipe;
    private object parentUI; // Can be BookPanelUI or MenuManager
    private Sprite normalBackground;
    private Sprite clickedBackground;
    private bool isSelected = false;
    private bool isInteractable = true;

    /// <summary>
    /// Initializes the recipe icon UI for BookPanelUI
    /// </summary>
    public void Init(Recipe recipeData, bool unlocked, BookPanelUI ui)
    {
        recipe = recipeData;
        parentUI = ui;

        iconImage.sprite = recipe.image;
        button.onClick.RemoveAllListeners();

        SetUnlocked(unlocked);

        if (unlocked)
        {
            button.onClick.AddListener(() => ui.ShowRecipe(recipe));
        }
    }
    
    /// <summary>
    /// Initializes the recipe icon UI for MenuManager
    /// </summary>
    public void Init(Recipe recipeData, bool canCook, MenuManager manager)
    {
        recipe = recipeData;
        parentUI = manager;

        iconImage.sprite = recipe.image;
        button.onClick.RemoveAllListeners();

        SetUnlocked(canCook);

        if (canCook)
        {
            button.onClick.AddListener(() => manager.OnRecipeIconClicked(recipe, this));
        }
    }
    
    /// <summary>
    /// Sets the background sprites for normal and clicked states.
    /// </summary>
    public void SetBackgrounds(Sprite normal, Sprite clicked)
    {
        normalBackground = normal;
        clickedBackground = clicked;
        
        backgroundImage.sprite = isSelected ? clickedBackground : normalBackground;
    }
    
    /// <summary>
    /// Sets whether the recipe is unlocked or locked.
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        isInteractable = unlocked;
        iconImage.color = unlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f);
        button.interactable = unlocked;
    }

    /// <summary>
    /// Sets whether this icon is selected or not.
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        backgroundImage.sprite = isSelected ? clickedBackground : normalBackground;
    }

    /// <summary>
    /// Checks if this icon is interactable
    /// </summary>
    public bool IsInteractable()
    {
        return isInteractable && button.interactable;
    }

    /// <summary>
    /// Gets the recipe associated with this icon
    /// </summary>
    public Recipe GetRecipe()
    {
        return recipe;
    }
}