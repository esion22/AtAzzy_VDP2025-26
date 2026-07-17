using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Highlights a shop item when the pointer hovers over it
/// </summary>
public class ShopItemHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemImage;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    /// <summary>
    /// Initializes the normal color
    /// </summary>
    void Start()
    {
        normalColor = itemImage.color;
    }

    /// <summary>
    /// Highlights the item when the pointer enters
    /// </summary>
    /// <param name="eventData">Pointer event data</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        itemImage.color = highlightColor;
    }

    /// <summary>
    /// Removes highlight when the pointer exits
    /// </summary>
    /// <param name="eventData">Pointer event data</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        itemImage.color = normalColor;
    }
}