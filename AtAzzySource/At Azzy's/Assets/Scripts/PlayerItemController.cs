using UnityEngine;
using System.Collections;

/*
 * Player controller responsible for:
 * - Inventory management
 * - Item dropping mechanics
 * - Directional facing logic
 */
public class PlayerItemController : MonoBehaviour
{
    private Inventory inventory;

    [Header("Drop Settings")]
    public float dropDistance = 1.5f; // Distance in front of player where items are dropped
    
    [Header("Inventory Clips")]
    [SerializeField] private AudioClip dropItemClip;

    private AudioManager _audioManager;

    // Directions used to determine where items should be dropped
    public enum FacingDirection { Up, Down, Left, Right }
    private FacingDirection currentFacingDirection = FacingDirection.Right;

    /*
     * Unity Start coroutine.
     * Waits until GameManager is initialized and links inventory.
     * @returns IEnumerator coroutine.
     */
    private void Start()
    {
        inventory = GameManager.instance.playerInventory;
    }


    /*
     * Unity Awake callback.
     * (Currently empty.)
     */
    private void Awake()
    {
        _audioManager = FindFirstObjectByType<AudioManager>();
    }

    /*
     * Unity Update callback.
     * Continuously updates player facing direction based on movement input.
     */
    private void Update()
    {
        UpdateFacingDirection();
    }

    /*
     * Returns the player's inventory.
     * @returns Player Inventory reference.
     */
    /*
 * Returns the player's inventory.
 * Checks if the inventory has been assigned,
 * if not, tries to get it from the GameManager again.
 * @returns Player Inventory reference.
 */
    public Inventory GetInventory()
    {
        if (inventory == null)
        {
            if (GameManager.instance != null && GameManager.instance.playerInventory != null)
            {
                inventory = GameManager.instance.playerInventory;
                // Debug.Log("Player>> GetInventory() >> inventory retrieved from GameManager");
            }
            else
            {
                Debug.LogWarning("Player>> GetInventory() >> GameManager inventory is still null!");
            }
        }

        return inventory;
    }


    /*
     * Updates current facing direction based on movement input.
     * Horizontal input has priority over vertical input.
     */
    private void UpdateFacingDirection()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            currentFacingDirection = horizontalInput > 0 ? FacingDirection.Right : FacingDirection.Left;
        }
        else if (Mathf.Abs(verticalInput) > 0.1f)
        {
            currentFacingDirection = verticalInput > 0 ? FacingDirection.Up : FacingDirection.Down;
        }
    }

    /*
     * Drops a collectable item in front of the player.
     * @param item The collectable object to drop.
     */
    public void DropItem(Collectable item)
    {
        // Debug.Log("Player>> DropItem() >> item: " + item);
        if (item == null) return;

        Vector3 dropPosition = CalculateDropPosition();
        _audioManager.PlaySFX(dropItemClip);

        // Debug.Log("Player>> DropItem() >> Dropping item: " + item.name + " with icon: " + item.getIcon());
        Collectable droppedItem = Object.Instantiate(item, dropPosition, Quaternion.identity);
        // Debug.Log($"Dropped item at position: {dropPosition} (facing: {currentFacingDirection})");
    }

    /*
     * Calculates the world position where an item should be dropped based on facing direction.
     * @returns A Vector3 drop position in front of the player.
     */
    private Vector3 CalculateDropPosition()
    {
        Vector3 basePosition = transform.position;

        switch (currentFacingDirection)
        {
            case FacingDirection.Right:
                return basePosition + Vector3.right * dropDistance;
            case FacingDirection.Left:
                return basePosition + Vector3.left * dropDistance;
            case FacingDirection.Up:
                return basePosition + Vector3.up * dropDistance;
            case FacingDirection.Down:
                return basePosition + Vector3.down * dropDistance;
            default:
                return basePosition + Vector3.right * dropDistance;
        }
    }

    /*
     * Manually sets the player's facing direction.
     * @param direction The direction to set.
     */
    public void SetFacingDirection(FacingDirection direction)
    {
        currentFacingDirection = direction;
    }

    /*
     * Returns the player's current facing direction.
     * @returns FacingDirection enum value.
     */
    public FacingDirection GetCurrentFacingDirection()
    {
        return currentFacingDirection;
    }

    /*
     * Alternative version of UpdateFacingDirection.
     * Keeps last input direction and prioritizes the stronger axis.
     */
    private void UpdateFacingDirectionWithLastInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f)
        {
            if (Mathf.Abs(horizontalInput) >= Mathf.Abs(verticalInput))
            {
                currentFacingDirection = horizontalInput > 0 ? FacingDirection.Right : FacingDirection.Left;
            }
            else
            {
                currentFacingDirection = verticalInput > 0 ? FacingDirection.Up : FacingDirection.Down;
            }
        }
    }
}
