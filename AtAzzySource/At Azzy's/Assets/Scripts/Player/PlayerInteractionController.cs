using UnityEngine;

/// <summary>
/// Manages player interactions with counter slots
/// </summary>
public class PlayerInteractionController : MonoBehaviour
{
    private CounterSlot nearbySlot;
    private Inventory playerInventory;
    
    [SerializeField] private GameObject cookingHighlightPrefab;
    private GameObject cookingHighlightInstance;
    private static PlayerInteractionController _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[PlayerInteractionController] Duplicate found, destroying instance: {GetInstanceID()}");
            Destroy(this);
            return;
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
    
    /// <summary>
    /// Initializes the player inventory and cooking highlight
    /// </summary>
    private void Start()
    {
        playerInventory = GameManager.instance.playerInventory;
        
        var allControllers = FindObjectsOfType<PlayerInteractionController>();
        
        cookingHighlightInstance = GameObject.Find("CookingHighlight(Clone)");
        if (cookingHighlightInstance == null)
        {
            cookingHighlightInstance = Instantiate(cookingHighlightPrefab, transform.position, Quaternion.identity);
        }
        cookingHighlightInstance.SetActive(false);
    }

    /// <summary>
    /// Handles player input for interacting with counter slots
    /// </summary>
    private void Update()
    {
        if (nearbySlot != null && Input.GetButtonDown("Interaction"))
        {
            Slot selectedSlot = playerInventory.getSelection();
            int selectionIndex = playerInventory.getSelectionIndex();
            
            // Swap items
            if (nearbySlot.HasItem() && selectedSlot != null)
            {
                Collectable counterItem = nearbySlot.PickUpItem();

                if (counterItem != null)
                {
                    // Put counter item into inventory
                    counterItem.isOnCounter = false;
                    playerInventory.Add(counterItem);
                    Destroy(counterItem.gameObject);

                    // Place inventory item onto counter
                    nearbySlot.PlaceItem(selectedSlot.getCategoryType());

                    int index = playerInventory.findSlotIndex(selectedSlot);
                    if (index != -1)
                    {
                        playerInventory.Remove(index);
                    }
                }
            }
            // Pick up only
            else if (nearbySlot.HasItem())
            {
                Collectable counterItem = nearbySlot.PickUpItem();
                if (counterItem != null)
                {
                    counterItem.isOnCounter = false;
                    playerInventory.Add(counterItem);
                    Destroy(counterItem.gameObject);
                }
            }
            // Place only
            else if (selectedSlot != null)
            {
                nearbySlot.PlaceItem(selectedSlot.getCategoryType());

                int index = playerInventory.findSlotIndex(selectedSlot);
                if (index != -1)
                {
                    playerInventory.Remove(index);
                }
            }
        }
    }

    /// <summary>
    /// Detects when the player enters a counter slot's trigger area
    /// </summary>
    /// <param name="other">The collider of the other object</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        CounterSlot slot = other.GetComponent<CounterSlot>();
        if (slot != null && cookingHighlightInstance != null)
        {
            nearbySlot = slot;
            cookingHighlightInstance.transform.position = nearbySlot.transform.position;
            cookingHighlightInstance.SetActive(true);
        }
    }

    /// <summary>
    /// Detects when the player exits a counter slot's trigger area
    /// </summary>
    /// <param name="other"> The collider of the other object</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        
        if (other.GetComponent<CounterSlot>() == nearbySlot && cookingHighlightInstance != null)
        {
            cookingHighlightInstance.SetActive(false);
            nearbySlot = null;
        }
    }
}