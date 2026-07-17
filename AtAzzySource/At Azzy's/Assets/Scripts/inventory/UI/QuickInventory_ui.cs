using UnityEngine;
using System.Collections.Generic;


/*
 * Quick selection UI for inventory – handles rapid item switching and display.
 */
public class QuickInventory_ui : MonoBehaviour
{
    [SerializeField] private Slot_ui previousSlot;
    [SerializeField] private Slot_ui currentSlot;
    [SerializeField] private Slot_ui nextSlot;

    private Inventory inventory;
    


    /*
     * Unity OnEnable callback.
     * Subscribes to inventory change event and refreshes UI.
     */
    private void OnEnable()
    {
        if (GameManager.instance != null)
        {
            inventory = GameManager.instance.playerInventory;
        }

        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshDisplay;
            inventory.OnInventoryChanged += RefreshDisplay;
        }

        RefreshDisplay();
    }

    /*
     * Unity OnDisable callback.
     * Unsubscribes from inventory event to avoid duplicate bindings.
     */
    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshDisplay;
        }
    }

    /*
     * Unity Update callback.
     * Handles navigation input for quick-slot switching.
     */
    private void Update()
    {
        if (inventory == null) return;
        
        if (Input.GetButtonDown("SelectPreviousSlot"))
        {
            SelectPreviousSlot();
        }
        if (Input.GetButtonDown("SelectNextSlot"))
        {
            SelectNextSlot();
        }
    }

    /*
     * Selects the next slot in inventory (circular).
     */
    public void SelectNextSlot()
    {
        if (inventory == null || inventory.getCount() <= 1)
            return;

        var slots = inventory.getSlots();
        int currentIndex = GetCurrentSelectedIndex();

        int nextIndex = FindValidIndex(currentIndex, +1, slots);

        inventory.setSelection(slots[nextIndex]);
    }


    /*
     * Selects the previous slot in inventory (circular).
     */
    public void SelectPreviousSlot()
    {
        if (inventory == null || inventory.getCount() <= 1)
            return;

        var slots = inventory.getSlots();
        int currentIndex = GetCurrentSelectedIndex();

        int previousIndex = FindValidIndex(currentIndex, -1, slots);

        inventory.setSelection(slots[previousIndex]);
    }


    /*
     * Updates the quick-slot UI to match the selected inventory slot.
     */
    private void UpdateSlotDisplay()
    {
        if (inventory == null) return;

        var slots = inventory.getSlots();
        int validCount = inventory.getCount();

        if (validCount == 0)
        {
            previousSlot.SetEmpty();
            currentSlot.SetEmpty();
            nextSlot.SetEmpty();
            return;
        }

        int currentIndex = GetCurrentSelectedIndex();
        SetSlot(currentSlot, slots[currentIndex]);


        if (validCount == 1)
        {
            previousSlot.SetEmpty();
            nextSlot.SetEmpty();
            return;
        }

 
        if (validCount == 2)
        {
            int otherIndex = FindValidIndex(currentIndex, +1, slots);

            SetSlot(nextSlot, slots[otherIndex]);
            previousSlot.SetEmpty();
            return;
        }


        int previousIndex = FindValidIndex(currentIndex, -1, slots);
        int nextIndex = FindValidIndex(currentIndex, +1, slots);

        SetSlot(previousSlot, slots[previousIndex]);
        SetSlot(nextSlot, slots[nextIndex]);


    }





    /*
     * Refreshes the quick-slot UI.
     * Ensures valid selection if inventory has items.
     */
    private void RefreshDisplay()
{
    if (currentSlot == null)
        currentSlot = GetComponentInChildren<Slot_ui>();

    if (inventory == null)
    {
        currentSlot?.SetEmpty();
        return;
    }

    var selection = inventory.getSelection();

    if (selection == null || selection.getType() == CollectableType.NONE)
    {
        previousSlot.SetEmpty();
        currentSlot.SetEmpty();
        nextSlot.SetEmpty();
        return;
    }

    UpdateSlotDisplay();
}


    /*
     * Returns the index of the currently selected slot.
     * @returns Slot index, or 0 if none.
     */
    private int GetCurrentSelectedIndex()
    {
        if (inventory == null || inventory.getSelection() == null)
            return 0;

        return inventory.getSlots().IndexOf(inventory.getSelection());
    }
    private void SetSlot(Slot_ui uiSlot, Slot slotData)
    {
        if (slotData != null && slotData.getType() != CollectableType.NONE)
            uiSlot.SetItem(slotData);
        else
            uiSlot.SetEmpty();
    }
    private int FindValidIndex(int startIndex, int direction, List<Slot> slots)
    {
        int count = slots.Count;

        for (int i = 1; i <= count; i++)
        {
            int index = (startIndex + i * direction + count) % count;

            if (slots[index].getType() != CollectableType.NONE)
                return index;
        }


        return startIndex;
    }


}