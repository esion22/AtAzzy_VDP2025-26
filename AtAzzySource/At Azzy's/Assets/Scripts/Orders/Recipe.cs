using System.Collections.Generic;
using UnityEngine;

namespace Orders
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "Orders/Recipe", order = 1)]
    public class Recipe : ScriptableObject
    {
        // These fields contain all of the ingredients that need to be used to make this recipe, this is used by the order manager to decide which recipes the player can cook
        [Tooltip("This ingredient refers to the amount at the same index as below")]
        public List<CollectableType> ingredientsNeeded;
        [Tooltip("This amount refers to the ingredient at the same index as above")]
        public List<int> amountNeeded;
        [Tooltip("Time (in seconds) that the player will have to prepare this order")]
        public int orderTime;
        [Tooltip("Image that will be displayed in the order")]
        public Sprite image;
        [Tooltip("Amount of money that will be awarded when the order is completed")]
        public int money;
        [Tooltip("Collectible Type that the food has to be in order to fulfill the order")]
        public CollectableType foodType;
        [Tooltip("Steps required to cook this recipe")]
        public CookingFormula formula;
    }
}
