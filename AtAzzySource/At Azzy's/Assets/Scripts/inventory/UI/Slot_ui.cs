using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// </summary>
public class Slot_ui : MonoBehaviour
{
    private Image itemIcon;
    private TextMeshProUGUI quantityText;
    
    private bool initialized = false;  // Prevent duplicate initialization

    void Awake()
    {
        InitializeComponents();
    }

    void Start()
    {
        InitializeComponents();
    }

    /// <summary>
    /// Finds and caches child UI components under the slot.
    /// </summary>
    private void InitializeComponents()
    {
        if (initialized) return;  // Avoid reinitializing in both Awake and Start
        initialized = true;

        // Find icon under this slot
        Transform iconTransform = transform.Find("icon");
        if (iconTransform != null)
        {
            itemIcon = iconTransform.GetComponent<Image>();

            if (itemIcon == null)
            {
                // Debug.LogError("Slot_ui: No Image component found on icon object!");
            }
        }
        else
        {
            // Debug.LogError("Slot_ui: No 'icon' child object found!");
        }

        // Find quantity text under this slot
        Transform textTransform = transform.Find("quantity text");
        if (textTransform != null)
        {
            quantityText = textTransform.GetComponent<TextMeshProUGUI>();
            if (quantityText == null)
            {
                // Debug.LogError("Slot_ui: No TextMeshProUGUI component found on quantity text object!");
            }
            else
            {
                // Debug.Log("Slot_ui: Found quantity text component");
            }
        }
        else
        {
            // Debug.LogError("Slot_ui: No 'quantity text' child object found!");
        }

        // Note: We do not clear the icon here to avoid wiping out data before SetItem is called.
    }

    /// <summary>
    /// Displays item information in the slot.
    /// </summary>
    public void SetItem(Slot slot)
    {   //Debug.Log($"<color=green>[Slot_ui] SetItem CALLED → type={slot.getType()}, count={slot.getCount()}</color>");

        // 1️⃣ slot.getIcon() 是 Collectable.cs 返回的图标
        Sprite s = slot.getIcon();
        //Debug.Log($"<color=yellow>[Slot_ui] Slot icon = {s}</color>");

        // 2️⃣ icon 是你 UI 上用来显示道具图标的 Image
        //Debug.Log($"<color=yellow>[Slot_ui] icon component = {itemIcon}</color>");
        if (!initialized) InitializeComponents();

        if (slot == null)
        {
            SetEmpty();
            return;
        }
        
        if (itemIcon != null)
        {
            itemIcon.sprite = slot.getIcon();
            itemIcon.color = new Color(1, 1, 1, 1);  // Fully visible
            // Debug.Log("Slot_ui>> SetItem>> Final icon sprite=" + itemIcon.sprite?.name);
        }

        if (quantityText != null)
        {
            quantityText.text = slot.getCount() > 1 ? slot.getCount().ToString() : "";
        }
      


    }

    /// <summary>
    /// Clears the slot UI (empty state).
    /// </summary>
    public void SetEmpty()
    {
        // Debug.Log("Slot_ui >> SetEmpty()");

        if (!initialized) InitializeComponents();

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.color = new Color(1, 1, 1, 0);  // Transparent
        }
        
        if (quantityText != null)
        {
            quantityText.text = "";
        }
    }
}
