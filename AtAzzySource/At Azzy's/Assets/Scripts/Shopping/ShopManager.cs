using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;

/// <summary>
/// Handles the shop purchasing logic
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("Shop Setup")] 
    public GameObject itemSlotPrefab;          
    public List<ShopItem> itemsForSale;        
    
    [Header("Player")]
    public PlayerStats playerStats;
    public PlayerItemController player; 
    private Inventory playerInventory;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip purchaseClip;

    private AudioManager _audioManager;

    // in every reload of the scene -> get unlocked recipes from recipe tracker and remove them from itemsForSale
    private void Start()
    {
        if (playerStats == null)
        {
            playerStats = GameObject.Find("GameManager (Persistent)").GetComponent<PlayerStats>();
        }

        if (player == null)
        {
            player = FindObjectOfType<PlayerItemController>();
        }
        
        playerInventory = GameManager.instance.playerInventory;

        if (playerInventory == null)
        {
            Debug.LogError("ShopManager: Inventory is null");
        }
        _audioManager = FindFirstObjectByType<AudioManager>();
    }
    
    /// <summary>
    /// Handles purchasing an item from the shop
    /// </summary>
    /// <param name="item">Object which wraps a collectable item and contains the price of it.</param>
    public void BuyItem(ShopItem item)
    {
        Collectable beingBought = item.itemToGive;
        
        if (playerInventory == null)
        {
            Debug.LogError("playerInventory is null");
            return;
        }
        
        if (beingBought == null)
        {
            Debug.LogError($"ShopItem {beingBought.getType()} has no itemToGive assigned");
            return;
        }

        if (playerStats.GetMoney() >= item.price)
        {
            _audioManager.PlaySFX(purchaseClip);
            if (beingBought.getCategory() == CollectableCategory.RECIPE)
            {
                RecipeTracker.Instance.UnlockRecipe(beingBought.getType());
                Debug.Log($"Added recipe to recipe book: {beingBought.getType()}");
                itemsForSale.Remove(item);
                FindObjectOfType<ShopPanelUI>().OpenShopUI(this);
            }
            else
            {
                playerInventory.Add(beingBought);
            }

            playerStats.SpendMoney(item.price);
            Debug.Log($"Bought: {beingBought.getType()}");
        }
        else
        {
            Debug.Log("Not enough money");
        }
    }
}