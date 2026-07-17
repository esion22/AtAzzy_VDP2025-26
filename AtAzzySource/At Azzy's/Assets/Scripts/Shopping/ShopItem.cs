using UnityEngine;

/// <summary>
/// Represents an item available for purchase in the shop
/// </summary>
[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item")]
public class ShopItem : ScriptableObject
{
    public Collectable itemToGive;
    public int price;
}