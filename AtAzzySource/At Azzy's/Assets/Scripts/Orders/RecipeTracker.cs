using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Orders;

public class RecipeTracker : MonoBehaviour
{
    public static RecipeTracker Instance;
    
    [Header("All Recipes")]
    [SerializeField] private List<Recipe> allRecipes = new List<Recipe>();

    [Header("Unlocked Recipes")]
    [SerializeField] private List<bool> unlockedRecipes = new List<bool>();

    public event System.Action<CollectableType> OnRecipeUnlocked;
    
    /*
     * Initializes the unlockedRecipes list and unlocks default recipes.
     */
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Ensure the unlockedRecipes list matches the size of allRecipes
        if (unlockedRecipes.Count != allRecipes.Count)
        {
            unlockedRecipes = new List<bool>(new bool[allRecipes.Count]);
        }

        // Unlock the default recipes
        UnlockDefaultRecipes();
    }

    /*
     * Unlocks a predefined set of default recipes at the start of the game.
     */
    private void UnlockDefaultRecipes()
    {
        CollectableType[] defaultRecipes = 
        {
            CollectableType.BAKED_POTATO, 
            CollectableType.SALAD, 
            CollectableType.COOKED_STEAK, 
            CollectableType.WINE, 
            CollectableType.VEGETABLE_SOUP
        };

        for (int i = 0; i < allRecipes.Count; i++)
        {
            if (defaultRecipes.Contains(allRecipes[i].foodType))
            {
                unlockedRecipes[i] = true;
            }
        }
    }
    
    /*
     * Returns a list of all recipes.
     */
    public List<Recipe> GetAllRecipes()
    {
        return allRecipes;
    }
    
    /*
     * Unlock all recipes
     */
    public void UnlockAllRecipes()
    {
        for (int i = 0; i < unlockedRecipes.Count; i++)
        {
            unlockedRecipes[i] = true;
        }
    }

    /*
     * Returns a list of all currently unlocked recipes.
     */
    public List<Recipe> GetUnlockedRecipes()
    {
        return allRecipes.Where((recipe, index) => unlockedRecipes[index]).ToList();
    }

    /*
     * Unlocks a recipe based on the provided CollectableType.
     * @param recipeType The CollectableType of the recipe to unlock. The result of the recipe. e.g. BAKED_POTATO
     */
    public void UnlockRecipe(CollectableType recipeType)
    {
        for (int i = 0; i < allRecipes.Count; i++)
        {
            if (allRecipes[i].foodType == recipeType)
            {
                unlockedRecipes[i] = true;
                allRecipes[i].ingredientsNeeded.ForEach(UnlockRecipe);
                OnRecipeUnlocked?.Invoke(recipeType);
                return;
            }
        }

        Debug.LogWarning($"Recipe with type '{recipeType}' not found in the list.");
    }

    /*
     * Checks if a given CollectableType corresponds to a recipe.
     * @param type The CollectableType to check.
     * @return True if the type is a recipe, false otherwise.
     */
    public bool IsFoodRecipe(CollectableType type)
    {
        foreach (Recipe r in allRecipes)
        {
            if (r.foodType == type)
            {
                return true;
            }
        }
        return false;
    }
    
    /*
     * Checks if a given CollectableType recipe is unlocked.
     * @param type The CollectableType to check.
     * @return True if the recipe is unlocked, false otherwise.
     */
    public bool isUnlockedRecipe(CollectableType type)
    {
        for (int i = 0; i < allRecipes.Count; i++)
        {
            if (allRecipes[i].foodType == type)
            {
                return unlockedRecipes[i];
            }
        }
        return false;
    }
    
    /// <summary>
    /// Gets an unlocked recipe by its output type.
    /// </summary>
    /// <param name="type">The food type produced by the recipe</param>
    /// <returns>The recipe if unlocked, null otherwise</returns>
    public Recipe GetUnlockedRecipe(CollectableType type)
    {
        for (int i = 0; i < allRecipes.Count; i++)
        {
            if (allRecipes[i].foodType == type && unlockedRecipes[i])
            {
                return allRecipes[i];
            }
        }
        return null;
    }

    /*
     * Checks if a given recipe is unlocked.
     * @param recipe The Recipe to check.
     * @return True if the recipe is unlocked, false otherwise.
     */
    public bool IsUnlocked(Recipe recipe)
    {
        int index = allRecipes.IndexOf(recipe);
        if (index >= 0 && unlockedRecipes[index])
        {
            return true;
        }
        return false;
    }
}