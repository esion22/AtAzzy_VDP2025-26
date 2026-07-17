using Orders;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI elements on the right page of the book, displaying the recipe's formula and icon.
/// </summary>
public class RightPageUI : MonoBehaviour
{
    [Header("Parent & Prefab")]
    public Transform formulaParent;         
    public CookingFormulaUI formulaPrefab; 
    
    [Header("Recipe Icon")]
    public Image recipeIconImage;
    public GameObject recipePrice;
    public TextMeshProUGUI recipePriceText;

    /// <summary>
    /// Displays the formula for the given recipe on the right page.
    /// </summary>
    public void DisplayRecipe(Recipe recipe)
    {
        recipeIconImage.enabled = false;
        recipePrice.SetActive(false);
        
        // Set recipe icon
        if (recipe.image != null)
        {
            recipeIconImage.sprite = recipe.image;
            recipeIconImage.enabled = true;
        }

        if (recipe.money != null)
        {
            recipePriceText.text = recipe.money.ToString();
            recipePriceText.enabled = true;
            recipePrice.SetActive(true);
        }
        
        // Clear previous formula
        foreach (Transform child in formulaParent)
            Destroy(child.gameObject);

        // Instantiate and setup the single formula
        var formulaUI = Instantiate(formulaPrefab, formulaParent, false);
        formulaUI.Setup(recipe);
    }
}