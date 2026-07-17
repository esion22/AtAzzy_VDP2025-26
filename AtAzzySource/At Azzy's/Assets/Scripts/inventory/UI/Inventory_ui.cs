using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/*
 * Handles the UI logic for the player's inventory.
 * Manages displaying items, slot interactions, selection highlighting,
 * and dropping items from the inventory.
 */
public class Inventory_ui : MonoBehaviour
{
    private GameObject inventoryPanel;
    private PlayerItemController player;
    private Inventory inventory;
    public List<Slot_ui> slots = new List<Slot_ui>();
    /*
     * Waits for the GameManager to be initialized before continuing setup.
     * Prevents null reference issues during scene loading.
     */
    private void Awake()
    {
        StartCoroutine(WaitForGameManager());
    }

    /*
     * Coroutine that pauses execution until the GameManager instance is available.
     * Ensures dependent systems are ready before initialization.
     */
    private IEnumerator WaitForGameManager()
    {
        while (GameManager.instance == null)
            yield return null;

        InitAfterGameManagerReady();
    }
    /*
     * Initializes references and UI elements after the GameManager is ready.
     * Subscribes to inventory updates and prepares slot interactions.
     */
    private void InitAfterGameManagerReady()
    {
        player = FindObjectOfType<PlayerItemController>();
        inventory = GameManager.instance.playerInventory;
        inventory.OnInventoryChanged -= Refresh;
        inventory.OnInventoryChanged += Refresh;

        GameObject inventoryGO = GameObject.Find("inventoryPanel");
        if (inventoryGO != null)
        {
            Transform t = inventoryGO.transform.Find("background");
            if (t != null)
            {
                inventoryPanel = t.gameObject;
                inventoryPanel.SetActive(false);
            }
        }

        SetupSlotClickEvents();
        Refresh();
    }

    /*
     * Ensures the inventory panel is hidden when the object becomes enabled
     * and refreshes the UI to reflect the current inventory state.
     */
    private void OnEnable()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;
    }

    /*
     * Handles player input for toggling the inventory, exiting the panel,
     * and dropping the currently selected item.
     */
    private void Update()
    {
        if (inventory == null || inventoryPanel == null) return;

        //I can only activate the inventory if there isnt an UI panel already opened (timescale != 0)
        if (Input.GetButtonDown("ToggleInventory") && Time.timeScale != 0)
            ToggleInventory();

        if (Input.GetButtonDown("Exit") && inventoryPanel.activeSelf)
            ToggleInventory();

        if (Input.GetButtonDown("DropItem") && inventory.getSelection() != null)
        {
            int slotID = inventory.getSelectionIndex();
            if (slotID < 0) return;

            var gm = GameManager.instance;
            var itemManager = gm != null ? gm.getItemManager() : null;
            if (itemManager == null) return;

            var sel = inventory.getSelection();
            if (sel == null) return;
            

            if (!itemManager.isNoDroptableCategory(sel.getCategory()))
                Remove(slotID);
        }
    }
    /*
     * Toggles the visibility of the inventory panel.
     * Pauses the game while the inventory is open and resumes it when closed.
     */
    public void ToggleInventory()
    {
        if (!inventoryPanel.activeSelf && Time.timeScale == 0f)
            return;
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);

        if (inventoryPanel.activeSelf)
        {
            Time.timeScale = 0f;
            Refresh();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    /*
     * Returns the list of UI slots representing the inventory.
     *
     * @return A list of Slot_ui elements.
     */
    public List<Slot_ui> getSlotsUI()
    {
        return slots;
    }

    /*
     * Refreshes the inventory UI by syncing each slot with the current
     * inventory data and updating the selection highlight.
     */
    public void Refresh()
    {
        if (inventory == null) return;

        List<Slot> invSlots = inventory.getSlots();
        if (invSlots == null) return;

        for (int i = 0; i < slots.Count && i < invSlots.Count; i++)
        {
            if (invSlots[i].getType() != CollectableType.NONE)
                slots[i].SetItem(invSlots[i]);
            else
                slots[i].SetEmpty();
        }

        RefreshSelectionHighlight();
    }

    /*
     * Assigns click listeners to each inventory slot button.
     * Ensures that selecting a slot triggers the correct callback.
     */
    private void SetupSlotClickEvents()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int slotIndex = i;

            Button button = slots[i].GetComponent<Button>();
            if (button == null)
                button = slots[i].gameObject.AddComponent<Button>();

            button.transition = Selectable.Transition.None;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnSlotClicked(slotIndex));
        }
    }

    /*
     * Handles slot click events.
     * Selects the item if the clicked slot is not empty.
     *
     * @param slotID The index of the clicked slot.
     */
    public void OnSlotClicked(int slotID)
    {
        if (inventory == null) return;

        if (inventory.getSlots()[slotID].getType() != CollectableType.NONE)
        {
            SelectItem(slotID);
        }
    }
    /*
     * Updates the currently selected inventory slot
     * and refreshes the highlight to reflect the change.
     *
     * @param slotID The index of the slot to select.
     */
    public void SelectItem(int slotID)
    {
        if (inventory == null) return;

        inventory.setSelectionIndex(slotID);  //tell system to change selection 

        RefreshSelectionHighlight();         // real time update highlight
    }


    /*
     * Updates the visual highlight for the selected slot
     * while resetting all others to their default appearance.
     */
    private void RefreshSelectionHighlight()
    {
        if (inventory == null) return;

        int selectedIndex = inventory.getSelectionIndex();

        for (int i = 0; i < slots.Count; i++)
        {
            Slot_ui slotUI = slots[i];
            Image bg = slotUI.GetComponent<Image>();
            if (bg == null) continue;

            if (i == selectedIndex)
                bg.color = new Color(1f, 0.9f, 0.5f);  
            else
                bg.color = Color.white;               
        }
    }
     /*
     * Removes the item from the specified slot and drops it into the world.
     * Validates references before performing the operation.
     *
     * @param slotID The index of the slot to remove the item from.
     */
    public void Remove(int slotID)
    {
        if (slotID < 0 || slotID >= slots.Count) return;
        if (inventory == null) return;

        if (GameManager.instance != null && GameManager.instance.getItemManager() != null)
        {
            Collectable itemToDrop =
                GameManager.instance.getItemManager()
                .GetItemByCategoryType(inventory.getSlots()[slotID].getCategoryType());

            if (itemToDrop != null && player != null)
            {
                player.DropItem(itemToDrop);
                inventory.Remove(slotID);
                Refresh();
            }
        }
    }
}
