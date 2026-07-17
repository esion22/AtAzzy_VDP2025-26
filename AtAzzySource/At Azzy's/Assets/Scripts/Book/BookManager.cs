using UnityEngine;

/// <summary>
/// Handles input for opening and closing the book UI
/// </summary>
public class BookInput : MonoBehaviour
{
    [SerializeField] private BookPanelUI bookPanel;

    private bool isOpen = false;

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (Input.GetButtonDown("ToggleBook"))
        {
            ToggleBook();
        }
        
        //Close book by pressing esc
        if (Input.GetButtonDown("Exit") && isOpen)
        {
            ToggleBook();
        }
    }

    /// <summary>
    /// Toggles the book UI open or closed
    /// </summary>
    private void ToggleBook()
    {
        if (!isOpen)
        {
            bookPanel.OpenBook();
            isOpen = true;
        }
        else
        {
            bookPanel.CloseBook();
            isOpen = false;
        }
    }
}