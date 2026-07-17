using UnityEngine;
using System.Collections.Generic;
using Orders;
using System.Linq;

[System.Serializable]
public class Inventory
{
    private List<Slot> slots = new List<Slot>();

    // --- Selection system (maintains two types because different UIs rely on different selection styles) ---

    // Used by QuickInventory: Slot reference based selection
    private Slot selection;

    // Used by Inventory UI: index-based selection (more stable)
    private int selectionIndex = -1;
    
    private Dictionary<(CollectableType, CollectableCategory), int> removedItems = new();


    // --- Event notification ---
    public System.Action OnInventoryChanged;

    /*
     * Initializes the inventory with a fixed number of slots.
     *
     * @param numSlots  The number of slots to create in the inventory.
     */
    public Inventory(int numSlots)
    {
        for (int i = 0; i < numSlots; i++)
        {
            slots.Add(new Slot());
        }
    }


    // --- Basic Accessors ---
    public List<Slot> getSlots() => slots;

    public Slot getSlot(int index) => slots[index];


    // --- Selection Getters ---

    /*
     * Gets the currently selected Slot reference.
     *
     * @return  The currently selected Slot, or null if none.
     */
    public Slot getSelection()
    {
        return selection;
    }

    /*
     * Gets the index of the currently selected inventory slot.
     *
     * @return  The selected index, or -1 if no slot is selected.
     */
    public int getSelectionIndex()
    {
        return selectionIndex;
    }


    // --- Selection Setter (QuickInventory uses Slot reference) ---

    /*
     * Sets the selection using a Slot reference.
     * Also synchronizes the index if the slot exists in the list.
     *
     * @param slot  The slot to select.
     */
    public void setSelection(Slot slot)
    {
        if (slot == null) return;

        // 1. Set Slot reference
        selection = slot;

        // 2. Sync index if the slot exists in the list
        int index = slots.IndexOf(slot);
        if (index >= 0)
        {
            selectionIndex = index;
        }

        // 3. Notify UI
        OnInventoryChanged?.Invoke();
        // Debug.Log("OnInventoryChanged called");

    }


    // --- Selection Setter (Inventory UI selects by index) ---

    /*
     * Sets the selection by index.
     * Also synchronizes the internal slot reference.
     *
     * @param index  The index of the slot to select.
     */
    public void setSelectionIndex(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        // 1. Set index
        selectionIndex = index;

        // 2. Sync Slot reference
        selection = slots[index];

        // 3. Notify UI
        OnInventoryChanged?.Invoke();
        // Debug.Log("OnInventoryChanged called");

    }


    // --- Backward compatibility ---

    /*
     * Selects a slot by index (legacy method).
     *
     * @param index  The index to select.
     */
    public void select(int index)
    {
        setSelectionIndex(index);
    }


    // --- Inventory Utility Functions ---

    /*
     * Counts the number of non-empty slots.
     *
     * @return  How many slots contain items.
     */
    public int getCount()
    {
        int count = 0;
        foreach (var slot in slots)
            if (slot.getType() != CollectableType.NONE && slot.getCount() > 0)
                count++;

        return count;
    }


    /*
     * Finds the index of a given slot.
     *
     * @param slot  The slot to locate.
     * @return      The slot index, or -1 if not found.
     */
    public int findSlotIndex(Slot slot)
    {
        return slots.IndexOf(slot);
    }


    /*
     * Adds an item to the inventory.
     * Attempts stacking first, otherwise places it in the first empty slot.
     *
     * @param itemToAdd  The item to add into the inventory.
     */
    public void Add(Collectable itemToAdd)
    {
        
        // Try stacking
        foreach (Slot slot in slots)
        {
            if (slot.getType() == itemToAdd.getType() &&
                slot.getCategory() == itemToAdd.getCategory() &&
                slot.CanAddItem())
            {
                slot.AddItem(itemToAdd);
                setSelection(slot);     // Auto-select the stacked slot
                return;
            }
        }

        // Otherwise look for empty slot
        foreach (Slot slot in slots)
        {
            if (slot.getType() == CollectableType.NONE)
            {
                slot.AddItem(itemToAdd);
                setSelection(slot);     // Auto-select the new slot
                return;
            }
        }
    }
    

    /*
     * Removes one item from the specified slot.
     * Handles shifting if the slot becomes empty and adjusts selection.
     *
     * @param index  The index of the slot to remove an item from.
     */
    public void Remove(int index)
    {
        // Debug.Log("Remove called");

        if (index < 0 || index >= slots.Count) return;

        // Remove one item
        slots[index].RemoveItem();

        bool slotBecameEmpty = (slots[index].getCount() <= 0 ||
                                slots[index].getType() == CollectableType.NONE);

        // Shift only if slot became empty
        if (slotBecameEmpty)
        {
            for (int i = index; i < slots.Count - 1; i++)
            {
                slots[i].CopyFrom(slots[i + 1]);
            }

            slots[slots.Count - 1].Clear();

            // Fix selection
            if (selectionIndex == index)
            {
                if (slots[index].getType() != CollectableType.NONE)
                {
                    selectionIndex = index;
                    selection = slots[index];
                }
                else if (index > 0)
                {
                    selectionIndex = index - 1;
                    selection = slots[selectionIndex];
                }
                else
                {
                    selectionIndex = -1;
                    selection = null;
                }
            }

        }
        else
        {
            // Slot still contains items; keep selection
            if (selectionIndex == index)
            {
                selection = slots[index];
            }
        }

        OnInventoryChanged?.Invoke();
        // Debug.Log("OnInventoryChanged called");
    }


    /*
     * Removes an item from the currently selected slot.
     *
     * @return  void
     */
    public void RemoveSelection()
    {
        // Debug.Log("RemoveSelection called");

        if (selectionIndex < 0 || selectionIndex >= slots.Count)
            return;

        // Use existing remove logic (handles shift and selection fix)
        Remove(selectionIndex);

        // Force UI refresh
        OnInventoryChanged?.Invoke();
        // Debug.Log("OnInventoryChanged called");


    }
    /// <summary>
    /// Clears all items from the inventory.
    /// Resets slots, selection, and notifies listeners once.
    /// </summary>
    public void ClearInventory()
    {
        // 1. Clear all slots
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Clear();
        }

        // 2. Reset selection
        selection = null;
        selectionIndex = -1;

        // 3. Notify UI once
        OnInventoryChanged?.Invoke();

        Debug.Log("🧹 [Inventory] Inventory cleared");
    }
    
    /*
     * Filters the inventory to only keep items needed for the selected recipes.
     * Unneeded items are removed and stored for potential restoration.
     *
     * @param selectedRecipes  The list of recipes to filter for.
     * @param recipeTracker    The recipe tracker to check for sub-recipes.
     */
    public void FilterInventoryForRecipes(List<Recipe> selectedRecipes, RecipeTracker recipeTracker)
    {
        // Step 1: Identify all required items and their counts
        HashSet<(CollectableType, CollectableCategory)> requiredItems = new();
        foreach (var recipe in selectedRecipes)
        {
            AddRecipeRequirements(recipe, requiredItems, recipeTracker);
            
            // Include already prepared food for the recipe
            var foodKey = (recipe.foodType, CollectableCategory.FOOD);
            if (!requiredItems.Contains(foodKey))
            {
                requiredItems.Add(foodKey);
            }
        }

        // Step 2: Remove unnecessary items and store them in the dictionary
        Dictionary<(CollectableType, CollectableCategory), int> itemsToKeep = new();
        foreach (var slot in slots.ToList())
        {
            var itemKey = (slot.getType(), slot.getCategory());
            if (requiredItems.Contains(itemKey))
            {
                if (itemsToKeep.ContainsKey(itemKey))
                {
                    itemsToKeep[itemKey] += slot.getCount();
                }
                else
                {
                    itemsToKeep[itemKey] = slot.getCount();
                }
            }
            else
            {
                if (removedItems.ContainsKey(itemKey))
                {
                    removedItems[itemKey] += slot.getCount();
                }
                else
                {
                    removedItems[itemKey] = slot.getCount();
                }
            }
        }
        
        ClearInventory();
        
        // Step 4: Add back only the items to keep
        foreach (var kvp in itemsToKeep)
        {
            var itemKey = kvp.Key;
            int count = kvp.Value;

            for (int i = 0; i < count; i++)
            {
                string key = itemKey.Item2.ToString() + " - " + itemKey.Item1.ToString();
                Collectable itemPrefab = ItemManager.Instance.GetItemByCategoryType(key);

                if (itemPrefab != null)
                {
                    Add(itemPrefab);
                }
                else
                {
                    Debug.LogWarning($"Could not find prefab for required item: {itemKey}");
                }
            }
        }

        OnInventoryChanged?.Invoke();
    }
    
    /*
     * Recursively adds all ingredients required for a recipe,
     * including sub-recipes, into the provided set.
     *
     * @param recipe          The recipe to analyze.
     * @param requiredItems   The set to populate with required item types.
     * @param recipeTracker   The recipe tracker to check for sub-recipes.
     */
    private void AddRecipeRequirements(Recipe recipe, HashSet<(CollectableType, CollectableCategory)> requiredItems, RecipeTracker recipeTracker)
    {
        foreach (var ingredient in recipe.ingredientsNeeded)
        {
            var ingredientKey = (ingredient, CollectableCategory.INGREDIENT);
            if (!requiredItems.Contains(ingredientKey))
            {
                requiredItems.Add(ingredientKey);

                // Check if the ingredient is a food and has its own recipe
                var subRecipe = recipeTracker.GetUnlockedRecipes()
                    .FirstOrDefault(r => r.foodType == ingredient);
                if (subRecipe != null)
                {
                    AddRecipeRequirements(subRecipe, requiredItems, recipeTracker);
                }
                
                // Add ingredient as category FOOD if it is used in recipes
                var foodKey = (ingredient, CollectableCategory.FOOD);
                if (!requiredItems.Contains(foodKey))
                {
                    requiredItems.Add(foodKey);
                }
            }
        }
    }
    
    /* 
     * Restores previously removed items back into the inventory.
     * Clears the removed items dictionary after restoration.
     */
    public void RestoreFilteredItems()
    {
        foreach (var kvp in removedItems)
        {
            var itemKey = kvp.Key;
            int quantity = kvp.Value;
            
            for (int i = 0; i < kvp.Value; i++)
            {
                // Try INGREDIENT category first, then FOOD
                string key = itemKey.Item2.ToString() + " - " + itemKey.Item1.ToString();

                Collectable itemPrefab = ItemManager.Instance.GetItemByCategoryType(key);

                if (itemPrefab != null)
                {
                    Add(itemPrefab);
                }
                else
                {
                    Debug.LogWarning($"Could not find prefab for ingredient: {itemKey}");
                }
            }
        }

        removedItems.Clear();
        OnInventoryChanged?.Invoke();
    }
    
    /*
     * Removes all BURNT and TRASH items from the inventory.
     */
    public void RemoveBurntAndTrashItems()
    {
        foreach (var slot in slots.ToList())
        {
            if (slot.getType() == CollectableType.BURNT || slot.getType() == CollectableType.TRASH)
            {
                slot.Clear();
            }
        }

        OnInventoryChanged?.Invoke();
    }
}