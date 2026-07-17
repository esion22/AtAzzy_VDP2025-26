using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*
 * Manages collectable items and provides item lookup functionality.
 */
public class ItemManager : MonoBehaviour
{
    public Collectable[] collectableItems;
    private List<CollectableCategory> NoCollisionCategory = new List<CollectableCategory>(){};
    private List<CollectableCategory> NoDroptableCategory = new List<CollectableCategory>(){};
    public Dictionary<string, Collectable> collectableDictionary = new Dictionary<string, Collectable>();
    public static ItemManager Instance { get; private set; }

    /*
     * Unity Awake callback.
     * Initializes the item dictionary with all collectable prefabs.
     */
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Se esiste già un'istanza, distruggi questo duplicato.
            Destroy(gameObject);
        }
        else
        {
            // Altrimenti, imposta questa come istanza e rendila persistente tra le scene.
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        foreach (Collectable item in collectableItems)
        {
            // Debug.Log("ItemManager>> Adding item: " + item.name);
            AddItem(item);
        }

        foreach (var kv in collectableDictionary)
        {
            // Debug.Log($"ItemManager>>awake>>DICT KEY={kv.Key}, PREFAB={kv.Value.name}");
        }

        // Debug.Log("ItemManager>>Awake>>Dictionary initialized with " + collectableDictionary.Count + " items");
    }

    /*
     * Checks if a category is in the no-drop list.
     * @param category The category to check.
     * @returns True if category cannot be dropped.
     */
    public bool isNoDroptableCategory(CollectableCategory category)
    {
        return NoDroptableCategory.Contains(category);
    }

    /*
     * Adds a collectable item into the dictionary (if not already present).
     * @param item The collectable prefab being registered.
     */
    private void AddItem(Collectable item)
    {
        // Debug.Log("ItemManager>>AddItem>> Adding item with type: " + item.getType());

        if (!collectableDictionary.ContainsKey(item.getCategoryType()))
        {
            collectableDictionary.Add(item.getCategoryType(), item);
            // Debug.Log("ItemManager>>AddItem>> Adding item to dictionary: " + item.name);
        }

        foreach (var kv in collectableDictionary)
        {
            // Debug.Log($"ItemManager>>AddItem>>DICT KEY={kv.Key}, PREFAB={kv.Value.name}");
        }
    }

    /*
     * Checks if a category should not trigger collisions.
     * @param categoryType The category to check.
     * @returns True if collision should be ignored.
     */
    public bool isNoCollisionCategory(CollectableCategory categoryType)
    {
        return NoCollisionCategory.Contains(categoryType);
    }

    /*
     * Returns collectable prefab by category-type string.
     * @param CategoryType Combined category-type key.
     * @returns Collectable prefab, or null if not found.
     */
    public Collectable GetItemByCategoryType(string CategoryType)
    {
        // Debug.Log("ItemManager>> Getting item by type: " + CategoryType);
        // Debug.Log("ItemManager>>Dictionary count: " + collectableDictionary.Count);

        foreach (var kv in collectableDictionary)
        {
            // Debug.Log($"ItemManager>>DICT KEY={kv.Key}, PREFAB={kv.Value.name}");
        }

        if (collectableDictionary.ContainsKey(CategoryType))
        {
            // Debug.Log("ItemManager>> Found item: " + collectableDictionary[CategoryType].name);
            return collectableDictionary[CategoryType];
        }
        else
        {
            return null;
        }
    }
}
