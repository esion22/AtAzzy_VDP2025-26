using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a slot on the counter that can hold a collectable item
/// </summary>
public class CounterSlot : MonoBehaviour
{
    private Collectable currentItem;
    
    /// <summary>
    /// Gets the current item on the counter slot
    /// </summary>
    /// <returns>The collectable item, or null if empty</returns>
    public Collectable getCurrentItem()
    {
        return currentItem;
    }
    
    /// <summary>
    /// Checks if the counter slot has an item
    /// </summary>
    /// <returns>True if there is an item, false otherwise</returns>
    public bool HasItem()
    {
        return currentItem != null;
    }

    /// <summary>
    /// Places an item of the specified category type onto the counter slot
    /// </summary>
    /// <param name="categoryType">The category type of the item to place</param>
    /// <returns>True if the item was placed successfully, false otherwise</returns>
    public bool PlaceItem(string categoryType)
    {
        if (HasItem())
        {
            Debug.LogWarning("Counter already has an item!");
            return false;
        }
    
        Collectable itemToPlace = GameManager.instance.getItemManager()
            .GetItemByCategoryType(categoryType);
        
        if (itemToPlace == null)
        {
            Debug.LogError($"Item not found: {categoryType}");
            return false;
        }

        Collectable instance = Instantiate(itemToPlace, transform.position, Quaternion.identity);
        instance.isOnCounter = true;
        currentItem = instance;
    
        Debug.Log($"Item placed: {instance.name}, Category: {instance.category}, Type: {instance.type}");
    
        return true;
    }

    /// <summary>
    /// Picks up and removes the item from the counter slot
    /// </summary>
    /// <returns>The collectable item that was picked up, or null if none</returns>
    public Collectable PickUpItem()
    {
        if (!HasItem())
        {
            return null;
        }

        Collectable item = currentItem;
        currentItem = null;
        return item;
    }
}