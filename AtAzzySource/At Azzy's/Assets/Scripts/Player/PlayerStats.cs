using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the player's statistics such as money
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int money;

    private MoneyDisplay _moneyDisplay;

    public void Initialize()
    {
        FindMoneyDisplay();
        if (_moneyDisplay != null)
        {
            _moneyDisplay.UpdateMoneyDisplay(money);
        }
    }

    private void FindMoneyDisplay()
    {
        if (SceneManager.GetActiveScene().name == "BootScene")
        {
            Debug.Log("BootScene detected: skipping MoneyDisplay search");
            return;
        }
        
        MoneyDisplay[] temp = GameObject.FindObjectsByType<MoneyDisplay>(FindObjectsInactive.Include ,FindObjectsSortMode.InstanceID);
        if (temp == null || temp.Length == 0)
        {
            Debug.LogError("Shopping: PlayerStats can't find MoneyDisplay");
            return;
        }
        _moneyDisplay = temp[0];
        Debug.Log(_moneyDisplay);
    }
    
    /// <summary>
    /// Checks if the player has enough money to afford an item
    /// </summary>
    /// <param name="amount">Price of item.</param>
    public bool HasMoney(int amount)
    {
        return money >= amount;
    }

    /// <summary>
    /// Spends money if the player can afford the item
    /// </summary>
    /// <param name="amount">Price of item.</param>
    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            if (_moneyDisplay == null)
            {
                FindMoneyDisplay();
            }
            _moneyDisplay.UpdateMoneyDisplay(money);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds money to the player's total
    /// </summary>
    /// <param name="amount">Amount of money.</param>   
    public void AddMoney(int amount)
    {
        money += amount;
        if (_moneyDisplay == null)
        {
            FindMoneyDisplay();
        }
        _moneyDisplay.UpdateMoneyDisplay(money);
    }
    
    public int GetMoney()
    {
        return money;
    }
}