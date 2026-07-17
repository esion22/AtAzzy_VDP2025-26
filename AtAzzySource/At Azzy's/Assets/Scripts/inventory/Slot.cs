using UnityEngine;

[System.Serializable]
public class Slot
{
    private CollectableCategory category;
    private CollectableType type;
    private int count;
    private int maxAllowed;
    private Sprite icon;

    /*
     * Constructor - initializes an empty slot.
     */
    public Slot()
    {
        type = CollectableType.NONE;
        count = 0;
        maxAllowed = 99;
    }
    
    /*
     * Returns the type of item stored in this slot.
     * @returns The CollectableType of this slot.
     */
	public CollectableType getType()
    {
        return type;
    }

    /*
     * Returns the collectable category stored in this slot.
     * @returns The CollectableCategory of this slot.
     */
    public CollectableCategory getCategory()
    {
        return category;
    }

    /*
     * Returns category and type combined as formatted string.
     * @returns String formatted as "Category - Type".
     */
    public string getCategoryType()
    {
        return category + " - " + type;
    }

    /*
     * Returns the item count stored in this slot.
     * @returns Number of items in slot.
     */
    public int getCount()
    {
        return count;
    }

    /*
     * Returns the icon associated with this slot's item.
     * @returns Sprite icon of the stored item.
     */
    public Sprite getIcon()
    {
        return icon;
    }	
	
    /*
     * Checks whether more items can be added.
     * @returns True if count < maxAllowed.
     */
    public bool CanAddItem()
    {
        return count < maxAllowed;
    }

    /*
     * Adds an item to the slot.
     * If the slot is empty, initializes it with the item's data.
     * @param item The Collectable being added.
     */
    public void AddItem(Collectable item)
    {
        // Debug.Log($"<color=yellow>[Slot.AddItem] Adding {item.name} to slot</color>");

        if (this.type == CollectableType.NONE)
        {
            this.type = item.getType();
            this.category = item.getCategory();
            this.count = 1;
            this.icon = item.getIcon();
        }
        else
        {
            this.count += 1;
        }

        // Debug.Log($"[Slot.AddItem] Now count = {this.count}, type = {this.type}");
    }

    /*
     * Removes one item from the slot.
     * Automatically clears slot if count reaches zero.
     */
    public void RemoveItem()
    {
        if (count > 0)
        {
            count--;

            if (count == 0)
            {
                icon = null;
                type = CollectableType.NONE;
                category = CollectableCategory.NONE;
            }
        }
    }

    /*
     * Copies all data from another slot.
     * @param other The Slot whose data will be copied.
     */
	public void CopyFrom(Slot other)
	{
    	this.type = other.getType();
    	this.category = other.getCategory();
    	this.count = other.getCount();
    	this.icon = other.getIcon();
	}

    /*
     * Clears all data in this slot and resets to empty state.
     */
	public void Clear()
	{
    	this.type = CollectableType.NONE;
    	this.category = CollectableCategory.NONE;
    	this.count = 0;
    	this.icon = null;
	}


}
