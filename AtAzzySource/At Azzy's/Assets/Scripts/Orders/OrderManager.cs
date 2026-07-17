using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Orders;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour
{
    [Header("Recipes")]
    [SerializeField] private List<Recipe> recipes;

    [Header("Order UI")]
    [SerializeField] private GameObject orderPrefab;
    [SerializeField] private Transform orderParent;

    [Header("Prototype Mode")]
    [SerializeField] private bool isPrototype;
    [SerializeField] private List<Recipe> prototypeRecipes;

    [Header("Items")]
    [SerializeField] private List<GameObject> itemsToSpawn;

    [Header("Order Generation Settings")]
    [SerializeField] private int initialOrders = 1;
    [SerializeField] private float timeBetweenOrders = 15f;
    [SerializeField] private float deltaTimeBetweenOrders = 3f;
    [SerializeField] private int maxOrdersPresentOnScreen = 3;
    [SerializeField] private int minTotalOrders = 4;
    [SerializeField] private int maxTotalOrders = 7;
    private int _totalNumOfOrders;
    
    [Header("Customer Setup")]
    public CustomerQueueManager customerQueue;
    public List<GameObject> customerPrefabs;
    public Transform customerSpawnPoint;
    public Transform counterPoint;
    public Transform exitPoint;
    public CounterSlot counterSlot;

    private readonly List<Order> _currentOrders = new();

    private PlayerStats _playerStats;

    private bool canGenerateOrders = false;
    private int ordersGenerated = 0;
    private int prototypeIndex = 0;

    private int successfulOrders = 0;
    private int failedOrders = 0;
    private int moneyGained = 0;

    /**
     * Unity lifecycle method.
     * Initializes player stats and prototype data.
     * @returns {void}
     */
    private void Start()
    {
        StartCoroutine(WaitForGameManager());
        _totalNumOfOrders = Random.Range(minTotalOrders, maxTotalOrders + 1);

    }
    private IEnumerator WaitForGameManager()
    {
        // Debug.Log($"[OrderManager.Start] instanceID={GetInstanceID()} name={name}");
        // Debug.Log($"[OrderManager.Start] isPrototype={isPrototype}");
        // Debug.Log($"[OrderManager.Start] recipes.Count={recipes?.Count} prototypeRecipes.Count={prototypeRecipes?.Count}");

        while (GameManager.instance == null)
            yield return null;

        _playerStats = GameManager.instance.GetComponent<PlayerStats>();

        if (isPrototype && prototypeRecipes.Count > 0)
        {
            Shuffle(prototypeRecipes);
            // Debug.Log("[OrderManager.Start] prototypeRecipes shuffled");
        }

    }

    /* =====================================================
     *  PUBLIC API
     * ===================================================== */

    /**
     * Starts the order generation coroutine.
     * @returns {void}
     */
    public void StartOrderGeneration()
    {
        // Debug.Log($"[OrderManager.StartOrderGeneration] called canGenerateOrders={canGenerateOrders}");

        if (canGenerateOrders)
            return;

        canGenerateOrders = true;
        // Debug.Log("[OrderManager.StartOrderGeneration] Coroutine started");
        StartCoroutine(OrderGenerationCoroutine());
    }

    /**
     * Returns the list of currently active orders.
     * @returns {List<Order>} Current orders.
     */
    public List<Order> GetCurrentOrders()
    {
        return _currentOrders;
    }

    /**
     * Attempts to fulfill an order with the given item.
     * @param {Collectable} item - Item used to fulfill an order.
     * @returns {bool} True if an order was fulfilled.
     */
    public bool TryToFurfillOrder(Order order, Collectable item)
    {
        Debug.Log($"👉 当前玩家钱（提交前）: {_playerStats.GetMoney()}");
        if (order.GetRecipe().foodType == item.type)
        {
            if (order.IsTutorialOrder() && CookingTutorial.Instance != null && CookingTutorial.Instance.IsActive)
            {
                CookingTutorial.Instance.OnTutorialOrderFulfilled(item.type);
                RemoveTutorialOrder(order);
                return true;
            }
            
            successfulOrders++;
            moneyGained += order.GetRecipe().money;
            
            Debug.Log($"👉 订单奖励金额: {order.GetRecipe().money}");
            Debug.Log($"👉 recipe.foodType: {order.GetRecipe().foodType}");

            _playerStats.AddMoney(order.GetRecipe().money);
            

            RemoveOrder(order, false);
            return true;
        }
        return false;
    }

    /**
     * Deletes an order and marks it as failed.
     * @param {Order} order - Order to delete.
     * @returns {void}
     */
    public void DeleteOrder(Order order)
    {
        failedOrders++;
        RemoveOrder(order, true);
    }

    /* =====================================================
     *  ORDER GENERATION
     * ===================================================== */

    /**
     * Coroutine responsible for timed order generation.
     * @returns {IEnumerator}
     */
    private IEnumerator OrderGenerationCoroutine()
    {
        
        // Initial check for valid recipes
        if (!isPrototype)
        {
            List<Recipe> initialValidRecipes = checkValidRecipes();
            if (initialValidRecipes.Count == 0)
            {
                yield return new WaitForSeconds(0.5f);
                if (EndOfDayManager.Instance != null)
                {
                    EndOfDayManager.Instance.CheckForDayEnd();
                }
                yield break;
            }
        }
        
        for (int i = 0; i < initialOrders; i++)
        {
            // Debug.Log($"[OrderManager.OrderGenerationCoroutine] Generate initial order {i}");
            yield return new WaitForSeconds(0.5f);
            generateNewOrder();
        }

        ordersGenerated = initialOrders;
        // Debug.Log($"[OrderManager.OrderGenerationCoroutine] ordersGenerated={ordersGenerated}");

        while (ordersGenerated < _totalNumOfOrders)
        {
            float wait =
                timeBetweenOrders + Random.Range(-deltaTimeBetweenOrders, deltaTimeBetweenOrders);

            // Debug.Log($"[OrderManager.OrderGenerationCoroutine] waiting {wait}s");
            yield return new WaitForSeconds(wait);

            if (_currentOrders.Count < maxOrdersPresentOnScreen)
            {
                // Debug.Log("[OrderManager.OrderGenerationCoroutine] generating next order");
                generateNewOrder();
                ordersGenerated++;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    /**
     * Generates a new order if allowed.
     * @returns {void}
     */
    public void generateNewOrder()
    {
        // Debug.Log($"[OrderManager.generateNewOrder] called canGenerateOrders={canGenerateOrders}");

        if (!canGenerateOrders)
            return;

        List<Recipe> validRecipes = checkValidRecipes();
        // Debug.Log($"[OrderManager.generateNewOrder] validRecipes.Count={validRecipes.Count}");

        if (validRecipes.Count == 0)
        {
            // Debug.LogError("[OrderManager.generateNewOrder] No valid recipe found");
            return;
        }

        int chosenIndex = Random.Range(0, validRecipes.Count);
        // Debug.Log($"[OrderManager.generateNewOrder] chosen recipe={validRecipes[chosenIndex].name}");

        GameObject orderGO = Instantiate(orderPrefab, orderParent, false);

        RectTransform rt = orderGO.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(_currentOrders.Count * 300, 0);

        Order order = orderGO.GetComponent<Order>();
        order.SetRecipe(validRecipes[chosenIndex]);

        _currentOrders.Add(order);
        // Debug.Log($"[OrderManager.generateNewOrder] order created currentOrders={_currentOrders.Count}");

        GameObject selectedCustomer = selectRandomCustomer();
        GameObject customerGO = Instantiate(
            selectedCustomer,
            customerSpawnPoint.position,
            Quaternion.identity
        );

        Customer customer = customerGO.GetComponent<Customer>();

        customer.Initialize(
            counterSlot,
            counterPoint,
            exitPoint,
            this,
            order,
            customerQueue.GetNextQueuePosition()
        );
        order.SetCustomerImage(customer.GetCustomerPortraitImage());
        customerQueue.AddCustomerToQueue(customer);
    }
    
    private GameObject selectRandomCustomer()
    {
        List<GameObject> availableCustomers = new List<GameObject>();
        foreach (GameObject customerPrefab in customerPrefabs)
        {
            if (GameObject.Find(customerPrefab.name + "(Clone)") == null)
            {
                //Customer is not present in scene, add it to the list
                availableCustomers.Add(customerPrefab);
            }
        }
        int index = Random.Range(0, availableCustomers.Count);
        return availableCustomers[index];
    }
    /**
     * Notifies the order manager that a customer has left the counter.
     * @returns {void}
     */
    public void OnCustomerLeavingCounter()
    {
        customerQueue.OnCustomerLeftCounter();
    }

    /**
     * Checks which recipes are currently valid to generate.
     * @returns {List<Recipe>} Valid recipes.
     */
    private List<Recipe> checkValidRecipes()
    {
        List<Recipe> valid = new();

        // Debug.Log($"[OrderManager.checkValidRecipes] isPrototype={isPrototype}");

        // ===== PROTOTYPE MODE =====
        if (isPrototype)
        {
            if (prototypeRecipes.Count == 0)
                return valid;

            Recipe recipe = prototypeRecipes[prototypeIndex];
            valid.Add(recipe);
            AddIngredientsToInventory(recipe);

            prototypeIndex = (prototypeIndex + 1) % prototypeRecipes.Count;
            return valid;
        }

        // ===== NORMAL MODE =====
        Dictionary<CollectableType, int> owned = getIngredientsFromInventory();
        owned = removeIngredientsForRunningOrders(owned);

        foreach (Recipe recipe in recipes)
        {
            Dictionary<CollectableType, int> needed =
                Utils.GetIngredientList(recipe.ingredientsNeeded, recipe.amountNeeded);

            if (CanCookWithIngredients(owned, needed))
            {
                valid.Add(recipe);
            }
        }

        return valid;
    }
    
    /// <summary>
    /// Recursively checks if a recipe can be cooked with the available ingredients, considering sub-recipes for missing ingredients.
    /// </summary>
    /// <param name="available">Dictionary of currently available ingredients and their counts.</param>
    /// <param name="needed">Dictionary of required ingredients for the recipe and their counts.</param>
    /// <returns>True if the recipe can be cooked with the available ingredients, false otherwise.</returns>
    private bool CanCookWithIngredients(
        Dictionary<CollectableType, int> available, 
        Dictionary<CollectableType, int> needed)
    {
        Dictionary<CollectableType, int> workingInventory = new(available);

        foreach (var (ingredientType, amountNeeded) in needed)
        {
            int amountOwned = workingInventory.GetValueOrDefault(ingredientType, 0);

            if (amountOwned >= amountNeeded)
            {
                workingInventory[ingredientType] -= amountNeeded;
                continue;
            }

            Recipe subRecipe = RecipeTracker.Instance.GetUnlockedRecipe(ingredientType);
            
            if (subRecipe != null)
            {
                Dictionary<CollectableType, int> subRecipeNeeds =
                    Utils.GetIngredientList(subRecipe.ingredientsNeeded, subRecipe.amountNeeded);

                int craftCount = amountNeeded - amountOwned;

                Dictionary<CollectableType, int> totalSubNeeds = new();
                foreach (var (subType, subAmount) in subRecipeNeeds)
                {
                    totalSubNeeds[subType] = subAmount * craftCount;
                }

                if (CanCookWithIngredients(workingInventory, totalSubNeeds))
                {
                    foreach (var (subType, subAmount) in totalSubNeeds)
                    {
                        workingInventory[subType] = 
                            workingInventory.GetValueOrDefault(subType, 0) - subAmount;
                    }
                    
                    workingInventory[ingredientType] = 
                        workingInventory.GetValueOrDefault(ingredientType, 0) + craftCount;
                }
                else
                {
                    Debug.Log("Cannot cook sub-recipe for ingredient " + ingredientType);
                    return false;
                }
            }
            else
            {
                Debug.Log(ingredientType + " is not a recipe");
                return false;
            }
        }
        
        return true;
    }

    /* =====================================================
     *  ORDER REMOVAL
     * ===================================================== */

    /**
     * Removes an order and optionally triggers a replacement.
     * @param {Order} order - Order to remove.
     * @param {bool} failed - Whether the order failed.
     * @returns {void}
     */
    private void RemoveOrder(Order order, bool failed)
    {
        if (_currentOrders.Count == 1 && ordersGenerated < _totalNumOfOrders)
        {
            generateNewOrder();
            ordersGenerated++;
        }

        StartCoroutine(RemoveOrderAnimation(order, failed));
    }

    /**
     * Plays order removal animation.
     * @param {Order} order - Order being removed.
     * @param {bool} failed - Whether the order failed.
     * @returns {IEnumerator}
     */
    private IEnumerator RemoveOrderAnimation(Order order, bool failed)
    {
        Image img = order.GetComponent<Image>();
        img.color = failed ? Color.red : Color.green;

        yield return new WaitForSeconds(0.8f);

        _currentOrders.Remove(order);
        Destroy(order.gameObject);
        refreshOrders();
        
        // Check for end of day conditions
        if (EndOfDayManager.Instance != null)
        {
            Debug.Log("Checking for day end conditions after order removal");
            EndOfDayManager.Instance.CheckForDayEnd();
        }
    }

    /**
     * Refreshes the UI positions of active orders.
     * @returns {void}
     */
    private void refreshOrders()
    {
        for (int i = 0; i < _currentOrders.Count; i++)
        {
            _currentOrders[i]
                .GetComponent<RectTransform>()
                .anchoredPosition = new Vector2(i * 300, 0);
        }
    }

    /* =====================================================
     *  INVENTORY HELPERS
     * ===================================================== */

    /**
     * Adds required ingredients for a recipe to the inventory.
     * @param {Recipe} recipe - Recipe whose ingredients are added.
     * @returns {void}
     */
    private void AddIngredientsToInventory(Recipe recipe)
    {
        PlayerItemController controller = FindFirstObjectByType<PlayerItemController>();

        for (int i = 0; i < recipe.amountNeeded.Count; i++)
        {
            CollectableType type = recipe.ingredientsNeeded[i];
            int amount = recipe.amountNeeded[i];

            foreach (GameObject obj in itemsToSpawn)
            {
                Collectable c = obj.GetComponent<Collectable>();
                if (c.getType() == type)
                {
                    for (int j = 0; j < amount; j++)
                        controller.GetInventory().Add(c);
                    break;
                }
            }
        }
    }

    /**
     * Retrieves ingredient counts from inventory.
     * @returns {Dictionary<CollectableType, int>} Owned ingredients.
     */
    private Dictionary<CollectableType, int> getIngredientsFromInventory()
    {
        Inventory inv = FindFirstObjectByType<PlayerItemController>().GetInventory();
        Dictionary<CollectableType, int> result = new();

        foreach (Slot s in inv.getSlots())
        {
            if (s.getCategory() == CollectableCategory.INGREDIENT)
                result[s.getType()] = s.getCount();
        }

        return result;
    }

    /**
     * Deducts ingredients already reserved by active orders.
     * @param {Dictionary<CollectableType, int>} owned - Current inventory.
     * @returns {Dictionary<CollectableType, int>} Updated inventory.
     */
    private Dictionary<CollectableType, int> removeIngredientsForRunningOrders(
        Dictionary<CollectableType, int> owned)
    {
        foreach (Order order in _currentOrders)
        {
            Recipe r = order.GetRecipe();
            for (int i = 0; i < r.ingredientsNeeded.Count; i++)
            {
                CollectableType t = r.ingredientsNeeded[i];
                int a = r.amountNeeded[i];

                if (!owned.ContainsKey(t)) continue;

                owned[t] -= a;
                if (owned[t] <= 0)
                    owned.Remove(t);
            }
        }
        return owned;
    }

    /**
     * Checks whether all required ingredients are available.
     * @param {Dictionary<CollectableType, int>} owned - Owned ingredients.
     * @param {Dictionary<CollectableType, int>} needed - Required ingredients.
     * @returns {bool} True if all ingredients are available.
     */
    private bool allIngredientsAreInStock(
        Dictionary<CollectableType, int> owned,
        Dictionary<CollectableType, int> needed)
    {
        foreach (var col in needed.Keys)
        {
            if (!owned.ContainsKey(col) || owned[col] < needed[col])
                return false;
        }
        return true;
    }

    /**
     * Shuffles a list using Fisher–Yates algorithm.
     * @param {IList<T>} list - List to shuffle.
     * @returns {void}
     */
    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    
    /**
     * Creates a tutorial order that never expires.
     * @param {Recipe} recipe - The recipe for the tutorial order.
     * @returns {Order} The created tutorial order.
     */
    public Order CreateTutorialOrder(Recipe recipe)
    {
        GameObject orderGO = Instantiate(orderPrefab, orderParent, false);

        RectTransform rt = orderGO.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(_currentOrders.Count * 300, 0);

        Order order = orderGO.GetComponent<Order>();
        order.SetRecipe(recipe);
        order.SetTutorialMode(true); // Impedisce la scadenza
        
        
        _currentOrders.Add(order);
        GameObject selectedCustomer = selectRandomCustomer();
        // Spawn customer for tutorial order
        GameObject customerGO = Instantiate(
            selectedCustomer,
            customerSpawnPoint.position,
            Quaternion.identity
        );

        Customer customer = customerGO.GetComponent<Customer>();
       
        customer.Initialize(
            counterSlot,
            counterPoint,
            exitPoint,
            this,
            order,
            customerQueue.GetNextQueuePosition()
        );
        
        order.SetCustomerImage(customer.GetCustomerPortraitImage());
        
        customerQueue.AddCustomerToQueue(customer);

        return order;
    }
    
    /**
     * Removes a tutorial order without generating new ones.
     * @param {Order} order - The order to remove.
    */
    public void RemoveTutorialOrder(Order order)
    {
        _currentOrders.Remove(order);
        Destroy(order.gameObject);
        refreshOrders();
    }
    
    /*
     * Getters for statistics
     */
    public int GetSuccessfulOrders()
    {
        return successfulOrders;
    }

    /*
     * Getters for statistics
     */
    public int GetFailedOrders()
    {
        return failedOrders;
    }

    /*
     * Getters for statistics
     */
    public int GetMoneyGained()
    {
        return moneyGained;
    }

    /*
     * Checks if all orders have been processed
     */
    public bool AreAllOrdersProcessed()
    {
        return ordersGenerated >= _totalNumOfOrders && _currentOrders.Count == 0;
    }

    /*
     * Checks if there are no valid recipes left to prepare
     */
    public bool HasNoValidRecipes()
    {
        if (isPrototype) return false;
    
        List<Recipe> validRecipes = checkValidRecipes();
        return validRecipes.Count == 0 && _currentOrders.Count == 0;
    }
}