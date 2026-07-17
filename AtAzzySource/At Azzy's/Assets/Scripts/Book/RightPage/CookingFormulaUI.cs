using UnityEngine;
using UnityEngine.UI;
using Orders;
using TMPro;

/// <summary>
/// Handles the UI representation of a cooking formula, including station, ingredients, and spells.
/// </summary>
public class CookingFormulaUI : MonoBehaviour
{
    [Header("Parents")] public Transform stationParent;
    public Transform ingredientsParent;
    public Transform spellsParent;

    [Header("Prefabs")] public Image stationIconPrefab;
    public GameObject ingredientIconPrefab;
    public Image spellIconPrefab;

    [Header("Icons")] public Sprite[] stationIcons; // index = StationType
    public Sprite[] spellIcons; // index = SpellType

    /// <summary>
    /// Sets up the cooking formula UI based on the provided recipe.
    /// </summary>
    /// <param name="recipe"> The recipe containing the cooking formula details.</param>
    public void Setup(Recipe recipe)
    {
        ClearAll();

        var formula = recipe.formula;

        // Station Icon
        var stationIcon = Instantiate(stationIconPrefab, stationParent);
        stationIcon.sprite = stationIcons[(int)formula.station];
        stationIcon.SetNativeSize();
        stationIcon.transform.localScale = Vector3.one * GetStationScale(formula.station);
        stationIcon.gameObject.SetActive(true);

        // Ingredient Icon(s)
        for (int i = 0; i < recipe.ingredientsNeeded.Count; i++)
        { 
            CollectableType ingredient = recipe.ingredientsNeeded[i];
            GameObject ingIconGO = Instantiate(ingredientIconPrefab, ingredientsParent);
            Image ingIcon = ingIconGO.GetComponent<Image>();
            int amount = recipe.amountNeeded[i];

            Collectable ingredientCollectable = getCollectableByCategoryType(ingredient);
            if (ingredientCollectable != null)
            {
                ingIcon.sprite = ingredientCollectable.getIcon();

                TMP_Text amountText = ingIconGO.GetComponentInChildren<TMP_Text>(true);
                if (amountText != null)
                {
                    amountText.text = amount + "x";
                    amountText.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"Amount Text component not found in ingredient icon prefab for ingredient {ingredient}, cannot set amount {amount}");
                }
            }
            else
            {
                Debug.LogWarning($"Collectable not found for ingredient {ingredient}");
            }

            ingIcon.gameObject.SetActive(true);
        }

        // Spell Icon(s)
        foreach (SpellType spell in formula.spells)
        {
            Image spellIcon = Instantiate(spellIconPrefab, spellsParent);
            spellIcon.sprite = spellIcons[(int)spell];
            spellIcon.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Retrieves a collectable item based on its category type.
    /// </summary>
    /// <param name="collectableType"> The type of the collectable item.</param>
    /// <returns> The corresponding collectable item, or null if not found.</returns>
    Collectable getCollectableByCategoryType(CollectableType collectableType)
    {
        string keyFood = CollectableCategory.FOOD.ToString() + " - " + collectableType;
        string keyIngredient = CollectableCategory.INGREDIENT.ToString() + " - " + collectableType;

        // Try to get collectable
        Collectable collectable = ItemManager.Instance.GetItemByCategoryType(keyFood);
        if (collectable == null)
        {
            collectable = ItemManager.Instance.GetItemByCategoryType(keyIngredient);
        }

        return collectable;

    }
    
    /// <summary>
    /// Gets the scale factor for the station icon based on the station type.
    /// </summary>
    /// <param name="station"> The type of the cooking station.</param>
    /// <returns> The scale factor for the station icon.</returns>
    private float GetStationScale(StationType station)
    {
        switch (station)
        {
            case StationType.CuttingBoard:
                return 0.75f;
            case StationType.Stove:
                return 0.35f;
            case StationType.Oven:
                return 0.25f;
            default:
                return 0.5f; 
        }
    }

    /// <summary>
    /// Clears all existing icons from the UI.
    /// </summary>
    void ClearAll()
    {
        foreach (Transform t in stationParent)
        {
            Destroy(t.gameObject);
        }

        foreach (Transform t in ingredientsParent)
        {
            Destroy(t.gameObject);
        }

        foreach (Transform t in spellsParent)
        {
            Destroy(t.gameObject);
        }
    }
}