using UnityEngine;
using TMPro;

/// <summary>
/// Displays the player's current money in the UI
/// </summary>
public class MoneyDisplay : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    
    /// <summary>
    /// Updates the money display when it is updated
    /// </summary>
    public  void UpdateMoneyDisplay(int money)
    {
        moneyText.text = money.ToString();
    }

}
