using NUnit.Framework.Constraints;
using UnityEngine;

/*
 * Represents a collectable item in the game world.
 * Handles automatic icon assignment and player collection.
 */
public class Collectable : MonoBehaviour
{
    public bool isOnCounter;
    public CollectableCategory category;
    public CollectableType type;  
    private Sprite icon;
    private Rigidbody2D rb2d;
    private SpriteRenderer spriteRenderer;
    private AudioManager _audioManager;

    /*
     * Unity Awake callback.
     * Initializes components and auto-assigns icon from SpriteRenderer if available.
     */
    private void Awake()
    {
        isOnCounter = false;
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null && icon == null)
        {
            icon = spriteRenderer.sprite;
            // Debug.Log($"Collectable: Auto-set icon for {gameObject.name}: {icon?.name}");
        }
        else if (spriteRenderer == null)
        {
            // Debug.LogWarning($"Collectable: {gameObject.name} missing SpriteRenderer - cannot auto-set icon");
        }
        _audioManager = FindFirstObjectByType<AudioManager>();
    }

    /*
     * Unity Start callback.
     * Ensures icon assignment if Awake did not set it properly.
     */
    private void Start()
    {
        if (icon == null && spriteRenderer != null)
        {
            icon = spriteRenderer.sprite;
            // Debug.Log($"Collectable: Start set icon to {spriteRenderer.sprite?.name}");
        }
    }

    /*
     * Trigger callback for detecting player collision.
     * @param collision The collider object entering the trigger area.
     * Adds item to player's inventory and destroys this object.
     */
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.instance.getItemManager().isNoCollisionCategory(category) || isOnCounter)
        {
            return;
        }

        // Debug.Log("Collectable>> Player detected: !!!!!");

        PlayerItemController player = collision.GetComponent<PlayerItemController>();

        // Debug.Log("Collectable>> Player detected: " + player);

        if (player)
        {
            _audioManager.PlayPickupItemSound();
            player.GetInventory().Add(this);
            // Debug.Log("Collectable>> getCategoryType: " + getCategoryType());

            Destroy(this.gameObject);
        }
    }

    /*
     * Returns category and type as a formatted string.
     * @returns Combined category-type string.
     */
    public string getCategoryType()
    {
        return category + " - " + type;
    }

    /*
     * Gets the collectable category.
     * @returns The category of this collectable.
     */
    public CollectableCategory getCategory()
    {
        return category;
    }

    /*
     * Gets the collectable type.
     * @returns The type of this collectable.
     */
    public CollectableType getType()
    {
        return type;
    }

    /*
     * Retrieves the icon for this collectable.
     * Auto-loads from SpriteRenderer if not assigned.
     * @returns The sprite icon representing this collectable.
     */
    public Sprite getIcon()
    {
        if (icon != null)
            return icon;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            icon = spriteRenderer.sprite;

        return icon;
    }

    /*
     * Unity Reset callback (Editor only).
     * Automatically assigns icon when component is reset.
     */
    private void Reset()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            icon = sr.sprite;
            // Debug.Log($"Collectable: Reset auto-set icon to {sr.sprite?.name}");
        }
    }

    /*
     * Manually updates icon from the current sprite.
     * (Shown in right-click context menu)
     */
    [ContextMenu("Update Icon from Current Sprite")]
    private void UpdateIconFromSprite()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            icon = spriteRenderer.sprite;
            // Debug.Log($"Updated icon: {icon?.name}");
        }
        else
        {
            // Debug.LogError("SpriteRenderer component not found");
        }
    }
}
