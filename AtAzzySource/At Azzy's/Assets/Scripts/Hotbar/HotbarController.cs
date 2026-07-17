using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;

public class HotbarController : MonoBehaviour
{
    public SpellSlotHighlighter[] slots;
    public int selectedSlot = 0;
    private List<string> buttonNames;

    void Start()
    {
        buttonNames = new List<string>() { "Spell Fire", "Spell Water", "Spell Growth" , "Spell Cut" };
    }
    /// <summary>
    /// Updates the highlighting of spell slots based on number key inputs
    /// </summary>
    void Update()
    {
        if (Time.timeScale == 0f) return;
        HandleScrollInput();
        HandleNumberInput();
    }

    /// <summary>
    /// Handles mouse scroll input to change selected slot
    /// </summary>
    void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
    
        if (scroll > 0f)
        {
            selectedSlot--;
            if (selectedSlot < 0)
            {
                selectedSlot = slots.Length - 1;
            }

            UpdateSlotHighlights();
        }
        else if (scroll < 0f)
        {
            selectedSlot++;
            if (selectedSlot >= slots.Length)
            {
                selectedSlot = 0;
            }
            UpdateSlotHighlights();
        }
    }

    /// <summary>
    /// Handles number key input to change selected slot
    /// </summary>
    void HandleNumberInput()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Input.GetButtonDown(buttonNames[i]))
            {
                selectedSlot = i;
                UpdateSlotHighlights();
            }
        }
    }

    /// <summary>
    /// Updates the highlighting of all spell slots based on the selected slot
    /// </summary>
    void UpdateSlotHighlights()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == selectedSlot)
            {
                slots[i].Highlight(true);
            }
            else
            {
                slots[i].Highlight(false);
            }
        }
    }

}