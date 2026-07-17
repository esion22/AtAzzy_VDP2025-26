using CookingStations;
using UnityEngine;

public class PlayerCook : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private GameObject cookingHighlightPrefab;

    private GameObject _cookingHighlightInstance;
    private PlayerItemController playerItemController;
    private AbstractCookingStation currentStation;
    private ItemManager itemManager;

    /*
     * Start is called before the first frame update
     */
    void Start()
    {
        playerItemController = GetComponent<PlayerItemController>();
        itemManager = ItemManager.Instance;
        
        // istantiate the cooking highlight prefab in the scene
        if (cookingHighlightPrefab != null)
        {
            _cookingHighlightInstance = Instantiate(cookingHighlightPrefab, transform.position, Quaternion.identity);
            _cookingHighlightInstance.SetActive(false);
        }
    }

    /*
     * Update is called once per frame
     */
    void Update()
    {
        FindClosestStation();

        // If we are near a station and the interaction button is pressed
        if (currentStation != null)
        {
            if (Input.GetButtonDown("Interaction"))
            {
                // Pass the playerItemController to the station to try to add an ingredient
                currentStation.TryToAddIngredient(playerItemController);
            }
            
            // Retrieve ingredients with 'R' key (set "Retrieve" in Input Manager)
            if (Input.GetButtonDown("Retrieve"))
            {
                if (currentStation.TryRetrieveIngredients(playerItemController))
                {
                    Debug.Log("Ingredients retrieved from cooking station.");
                }
            }
            
            // Check for collecting the cooked item
            CollectAndResetStation();
        }
    }

    /*
     * Collects the output from the current cooking station and resets its output slot
     */
    private void CollectAndResetStation()
    {
        // Check if there is an output to collect
        CollectableType outputType = currentStation.GetOutputSlot();
        CollectableCategory outputCategory1 = CollectableCategory.FOOD;
        CollectableCategory outputCategory2 = CollectableCategory.INGREDIENT;
        if (outputType != CollectableType.NONE)
        {
            // if itemManager is null, log error and return
            if (itemManager == null) return;
            
            string outputString1 = outputCategory1.ToString() + " - " + outputType.ToString();
            string outputString2 = outputCategory2.ToString() + " - " + outputType.ToString();
            // Use the ItemManager to get the prefab for the output type
            Collectable itemPrefab = itemManager.GetItemByCategoryType(outputString1);
            if (itemPrefab == null)
            {
                itemPrefab = itemManager.GetItemByCategoryType(outputString2);
            }
            if (itemPrefab != null)
            {
                // Add the item to the player's inventory
                playerItemController.GetInventory().Add(itemPrefab);
                // Reset the station's output slot
                currentStation.ResetOutputSlot(); 
            }
            else
            {
                
                Debug.LogError("Prefab not found for: " + outputType + ". Make sure that it was added to ItemManager.");
            }
        }
    }
    
    /*
     * Finds the closest cooking station within interaction distance
     */
    private void FindClosestStation()
    {
        AbstractCookingStation closestStation = null;
        float minDistance = float.MaxValue;

        // Find all cooking stations in the scene
        AbstractCookingStation[] allStations = FindObjectsOfType<AbstractCookingStation>();

        foreach (var station in allStations)
        {
            var stationRenderer = station.GetComponentInChildren<Renderer>();
            Vector3 stationCenter = (stationRenderer != null) ? stationRenderer.bounds.center : station.transform.position;
            // if it is an oven, adjust the center downwards
            if (station is OvenStation)
            {
                stationCenter.y -= 1.0f;
            }
            float distance = Vector2.Distance(transform.position, (Vector2)stationCenter);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestStation = station;
            }
        }

        // If the closest station is within interaction distance, set it as current
        if (minDistance <= interactionDistance)
        {
            if (currentStation != closestStation)
            {
                currentStation = closestStation;
                // Visual feedback: the player is near a station
                var renderer = currentStation.GetComponent<Renderer>();
                Vector3 center = (renderer != null) ? renderer.bounds.center : currentStation.transform.position;
                // if it is an oven, adjust the center downwards
                if (currentStation is OvenStation)
                {
                    center.y -= 1.0f;
                }
                _cookingHighlightInstance.transform.position = new Vector3(center.x, center.y, _cookingHighlightInstance.transform.position.z);
                _cookingHighlightInstance.SetActive(true);
            }
        }
        else
        {
            if (currentStation != null)
            {
                // Visual feedback: the player moved away from the station
                currentStation = null;
                _cookingHighlightInstance.SetActive(false);
            }
        }
    }
}
