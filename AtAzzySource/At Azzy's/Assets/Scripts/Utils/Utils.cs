using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    /*
     * Useful util function to get a certain game object in the scene and display an error if it is not found
     * @param name The name of the gameobject you want to find in the scene
     * @returns The desired GameObject
     */ 
    public static GameObject FindGameObjectInScene(string name)
    {
            GameObject temp = GameObject.Find(name);
            if (temp != null)
            {
                //We have found the game object
                return temp;
            }
            else
            {
                #if UNITY_EDITOR
                    Debug.LogError($"Utils function did not find game object '{name}' in the scene.");
                #endif
                //Returning null will likely cause a cascade of errors, in case it doesn't the debug error will tell us that it failed
                return null;
            }
    }
    
    public static Dictionary<CollectableType, int> GetIngredientList(List<CollectableType> _ingredientsNeeded, List<int> _amountNeeded)
    {
        Dictionary<CollectableType, int> result = new Dictionary<CollectableType, int>();

        if (_amountNeeded.Count !=  _ingredientsNeeded.Count)
            return null;

        for (int i = 0; i < _amountNeeded.Count; i++)
        {
            result.Add(_ingredientsNeeded[i], _amountNeeded[i]);
        }
            
        return result;
    }
}
