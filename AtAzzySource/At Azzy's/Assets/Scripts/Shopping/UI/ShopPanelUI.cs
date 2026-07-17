using System;
using System.Collections.Generic;
using System.Globalization;
using Orders;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the shop UI panel
/// </summary>
public class ShopPanelUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform itemContainer;

    private ShopManager currentShopManager;
    private int numberOfFinalRecipes = 2;

    private void Awake()
    {
        gameObject.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShopUI);
        }
    }

    /// <summary>
    /// Setups the shop UI by populating items for sale and toggling visibility
    /// </summary>
    public void OpenShopUI(ShopManager shopManager)
    {
        currentShopManager = shopManager;
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ShopItem item in currentShopManager.itemsForSale)
        {
            if (item.itemToGive.category == CollectableCategory.RECIPE)
            {
                if (RecipeTracker.Instance.GetUnlockedRecipe(item.itemToGive.type) != null)
                {
                    continue;
                }
            }
            GameObject slot = Instantiate(currentShopManager.itemSlotPrefab, itemContainer);
            Collectable itemToGive = item.itemToGive;

            String parsedName = ParseName(itemToGive.getType().ToString());
            slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = parsedName;
            slot.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = item.price.ToString();
            slot.transform.Find("Icon").GetComponent<Image>().sprite = itemToGive.getIcon();

            Button buyButton = slot.transform.Find("BuyButton").GetComponent<Button>();
            buyButton.interactable = EnableInteractable(itemToGive.type);
            buyButton.onClick.AddListener(() => currentShopManager.BuyItem(item));

            Transform lockOverlay = slot.transform.Find("LockOverlay");
            if (lockOverlay == null)
            {
                Debug.LogError("LockOverlay not found in shop item slot prefab");
            }
            
            if (!buyButton.interactable)
            {
                lockOverlay.gameObject.SetActive(true);    
            }
            else
            {
                lockOverlay.gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    /// <summary>
    /// Parses a collectable name into Title Case format and replaces underscores with spaces
    /// </summary>
    /// <param name="collectableName">The original collectable name</param>
    /// <returns>The parsed collectable name</returns>
    private String ParseName(String collectableName)
    {
        string parsedName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
            collectableName.Replace("_", " ").ToLower()
        );
        return parsedName;
    }

    /// <summary>
    /// Determines if a shop item should be interactable based on its type and the player's progress
    /// </summary>
    /// <param name="type">The type of the collectable item</param>
    /// <returns>True if the item should be interactable, false otherwise</returns>
    private bool EnableInteractable(CollectableType type)
    {
        if (type != CollectableType.RAINBOW_BURRITO && type != CollectableType.RAINBOW_ESSENCE)
        {
            return true;
        }
        
        // Check if all other recipes are unlocked before allowing purchase of final recipes
        int allRecipesCount = RecipeTracker.Instance.GetAllRecipes().Count;
        int unlockedRecipesCount = RecipeTracker.Instance.GetUnlockedRecipes().Count;

        if (type == CollectableType.RAINBOW_ESSENCE)
        {
            return unlockedRecipesCount >= allRecipesCount - numberOfFinalRecipes;
        }

        if (type == CollectableType.RAINBOW_BURRITO)
        {
            return unlockedRecipesCount >= allRecipesCount - 1 &&
                   RecipeTracker.Instance.GetUnlockedRecipe(CollectableType.RAINBOW_ESSENCE) != null;
        }

        return false;
    }


    /// <summary>
    /// Closes the shop
    /// </summary>
    private void CloseShopUI()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;

        // check if the finale needs to be triggered
        CheckForFinaleTrigger();
    }
    
    
    /// <summary>
    /// Checks if the conditions to trigger the game finale are met
    /// </summary>
    private void CheckForFinaleTrigger()
    {
        // retrieve the key for the farm finale, if already completed, do nothing
        if (PlayerPrefs.GetInt("FarmFinaleCompleted", 0) == 1)
        {
            return;
        }

        // retrieve the recipe tracker
        RecipeTracker recipeTracker = FindObjectOfType<RecipeTracker>();
        if (recipeTracker != null)
        {
            // get the unlocked recipes
            var unlockedRecipes = recipeTracker.GetUnlockedRecipes();
            bool hasRainbowEssence = false;
            bool hasRainbowBurrito = false;
            foreach (var recipe in unlockedRecipes)
            {
                if (recipe.foodType == CollectableType.RAINBOW_ESSENCE)
                {
                    hasRainbowEssence = true;
                }
                else if (recipe.foodType == CollectableType.RAINBOW_BURRITO)
                {
                    hasRainbowBurrito = true;
                }
            }
            
            if (hasRainbowEssence && hasRainbowBurrito)
            {
                GameFinaleFarmManager finaleManager = GameFinaleFarmManager.Instance;
                if (finaleManager != null && !finaleManager.IsActive)
                {
                    finaleManager.TriggerFinale();
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Exit"))
        {
            CloseShopUI();
        }
    }
}