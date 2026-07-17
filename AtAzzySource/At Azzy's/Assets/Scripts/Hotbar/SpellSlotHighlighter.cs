using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the highlighting of a spell slot in the hotbar
/// </summary>
public class SpellSlotHighlighter : MonoBehaviour
{
    public Image slotImage;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    // Tutorial highlight color (set by CookingTutorial)
    private Color? _tutorialColor = null;
    private bool _isSelected = false;
    
    /// <summary>
    /// Initializes the spell slot to its normal color
    /// </summary>
    void Start()
    {
        if (slotImage != null)
            slotImage.color = normalColor;
    }

    /// <summary>
    /// Highlights or unhighlights the spell slot
    /// </summary>
    /// <param name="bool">True to highlight, false to unhighlight</param>
    public void Highlight(bool on)
    {
        _isSelected = on;
        UpdateColor();
    }
    
    /// <summary>
    /// Sets a tutorial highlight color. Pass null to clear.
    /// </summary>
    public void SetTutorialHighlight(Color? color)
    {
        _tutorialColor = color;
        UpdateColor();
    }

    /// <summary>
    /// Updates the slot color based on selection and tutorial state
    /// </summary>
    private void UpdateColor()
    {
        if (slotImage == null) return;

        if (_tutorialColor.HasValue)
        {
            // Tutorial active: bright color, or darker if also selected
            Color baseColor = _tutorialColor.Value;
            slotImage.color = _isSelected ? DarkenColor(baseColor, 0.7f) : baseColor;
        }
        else
        {
            // Normal behavior
            slotImage.color = _isSelected ? highlightColor : normalColor;
        }
    }

    /// <summary>
    /// Darkens a color by a given factor
    /// @param color The original color
    /// @param factor The darkening factor (0 to 1)
    /// @returns The darkened color
    /// </summary>
    private Color DarkenColor(Color color, float factor)
    {
        return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
    }
}