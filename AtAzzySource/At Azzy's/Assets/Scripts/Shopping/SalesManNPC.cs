using UnityEngine;

/// <summary>
/// The SalesMan NPC that opens the shop UI
/// </summary>
public class SalesManNPC : MonoBehaviour
{
    public ShopManager shopManager;
    public ShopPanelUI shopPanelUI;
    private bool playerInRange = false;
    private GameObject _highlight;
    
    private void Awake()
    {
        if (shopPanelUI == null)
        {
            shopPanelUI = FindObjectOfType<ShopPanelUI>();
            if (shopPanelUI == null)
            {
                Debug.LogError("SalesManNPC: ShopPanelUI not found in scene");
            }
            else
            {
                Debug.Log("SalesManNPC: ShopPanelUI found in scene");
            }
        }
        // find the highlight child object
        _highlight = transform.Find("Highlight").gameObject;
        if (_highlight == null)
        {
            Debug.LogError("SalesManNPC: Highlight child object not found");
        }
        _highlight.SetActive(false);
    }

    /// <summary>
    /// Opens the shop UI
    /// </summary>
    public void StartShop()
    {
        shopPanelUI.OpenShopUI(shopManager);
    }
    
    /// <summary>
    /// Handles player entering the NPC's interaction range
    /// </summary>
    /// <param name="other">The Collider of the player.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (_highlight != null)
            {
                _highlight.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Handles player exiting the NPC's interaction range
    /// </summary>
    /// <param name="other">The Collider of the player.</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (_highlight != null)
            {
                _highlight.SetActive(false);
            }
        }
    }
}