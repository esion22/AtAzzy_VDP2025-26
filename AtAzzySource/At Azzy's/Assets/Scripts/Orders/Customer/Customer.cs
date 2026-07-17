using Orders;
using Orders.Customer;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a customer in the game who can queue, order food, and leave.
/// </summary>
public class Customer : MonoBehaviour
{
    // Spawn and Target points
    private CounterSlot counterSlot;
    private Transform counterPoint;
    private Transform exitPoint;

    // Movement
    public float moveSpeed = 2f;
    private Animator animator;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _movement;
    private Vector2 _previousMovement;

    // Order
    private OrderManager orderManager;
    private Order order;
    [SerializeField] private SpriteRenderer boxSpriteRenderer;
    [SerializeField] private SpriteRenderer foodSpriteRenderer;
    [SerializeField] private Sprite customerPortraitImage;

    // Queue
    private Vector3 queuePosition;
    private CustomerState state;
    private bool isAtCounter = false;

    /// <summary>
    /// Initializes the customer with necessary references and starting position.
    /// </summary>
    /// <param name="slot"> The counter slot the customer will interact with.</param>
    /// <param name="counter"> The transform of the counter point.</param>
    /// <param name="exit"> The transform of the exit point.</param>
    /// <param name="linkedOrderManager"> The order manager handling orders.</param>
    /// <param name="linkedOrder"> The order associated with this customer.</param>
    /// <param name="initialQueuePosition"> The initial position in the queue.</param>
    public void Initialize(
        CounterSlot slot,
        Transform counter,
        Transform exit,
        OrderManager linkedOrderManager,
        Order linkedOrder,
        Vector3 initialQueuePosition)
    {
        counterSlot = slot;
        counterPoint = counter;
        exitPoint = exit;
        orderManager = linkedOrderManager;
        order = linkedOrder;
        queuePosition = initialQueuePosition;

        order.OnOrderExpired += OnOrderExpired;
        
        SetOrderSprite(order.GetRecipe().image);

        queuePosition = transform.position;
    }

    /// <summary>
    /// Initializes references
    /// </summary>
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Sets up the customer on start
    /// </summary>
    private void Start()
    {
        animator = GetComponent<Animator>();

        if (boxSpriteRenderer != null)
        {
            boxSpriteRenderer.enabled = true;
            Debug.Log("box sprite renderer: " + boxSpriteRenderer.name);
        }

        if (foodSpriteRenderer != null)
        {
            Debug.Log("food sprite renderer: " + foodSpriteRenderer.name);
            foodSpriteRenderer.enabled = true;
        }


        Debug.Log($"Spawned at {transform.position}, state: {state}");
    }

    /// <summary>
    /// Updates the customer behavior based on their current state
    /// </summary>
    private void Update()
    {
        switch (state)
        {
            case CustomerState.WaitingInQueue:
                MoveTo(queuePosition);
                if (Reached(queuePosition))
                {
                    StopMoving();
                    _spriteRenderer.flipX = true; // Ensure the customer faces left while waiting in queue
                }

                break;
            case CustomerState.WalkingToCounter:
                MoveTo(counterPoint.position);
                if (Reached(counterPoint.position))
                {
                    // Debug.Log("Reached counter");
                    StopMoving();
                    isAtCounter = true;
                    state = CustomerState.WaitingAtCounter;
                }

                break;

            case CustomerState.WaitingAtCounter:
                // Debug.Log("WaitingAtCounter");
                CheckOrder();
                break;

            case CustomerState.Leaving:
                MoveTo(exitPoint.position);
                if (Reached(exitPoint.position))
                {
                    Destroy(gameObject);
                }

                break;
        }

        UpdateOrderDisplay();
    }

    /// <summary>
    /// Sets the sprite representing the customer's order
    /// </summary>
    /// <param name="foodSprite"></param>
    public void SetOrderSprite(Sprite foodSprite)
    {
        if (foodSpriteRenderer != null)
        {
            foodSpriteRenderer.sprite = foodSprite;
            foodSpriteRenderer.enabled = false;
        }

        if (boxSpriteRenderer != null)
        {
            boxSpriteRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Updates the visibility of the order display based on customer state
    /// </summary>
    private void UpdateOrderDisplay()
    {
        bool shouldShow = state == CustomerState.WaitingAtCounter;

        if (state == CustomerState.WaitingAtCounter || state == CustomerState.WaitingInQueue)
        {
            shouldShow = true;
        }
        else
        {
            shouldShow = false;
        }

        if (foodSpriteRenderer != null)
        {
            foodSpriteRenderer.enabled = shouldShow;
        }

        if (boxSpriteRenderer != null)
        {
            boxSpriteRenderer.enabled = shouldShow;
        }
    }

    /// <summary>
    /// Moves the customer to a specified position in the queue
    /// </summary>
    /// <param name="position"></param>
    public void MoveToQueuePosition(Vector3 position)
    {
        queuePosition = position;
        state = CustomerState.WaitingInQueue;
        Debug.Log($"Moving to queue position {position}");
    }

    /// <summary>
    /// Changes the customer state to walking to the counter
    /// </summary>
    public void MoveToCounter()
    {
        Debug.Log("Changing state to WalkingToCounter");
        state = CustomerState.WalkingToCounter;
    }

    /// <summary>
    /// Handles sprite flipping based on movement direction
    /// </summary>
    /// <param name="movement"></param>
    private void HandleSpriteFlip(Vector2 movement)
    {
        float x = movement.x != 0 ? movement.x : _previousMovement.x;

        _spriteRenderer.flipX = (x <= 0);
    }

    /// <summary>
    /// Moves the customer towards a target position
    /// </summary>
    /// <param name="target"></param>
    public void MoveTo(Vector3 target)
    {
        Vector3 newPos = transform.position;
        _movement = newPos - transform.position;
        transform.position = newPos;
        
        HandleSpriteFlip(_movement);

        // Axis-aligned: horizontal zuerst
        if (Mathf.Abs(target.x - transform.position.x) > 0.01f)
            newPos.x = Mathf.MoveTowards(transform.position.x, target.x, moveSpeed * Time.deltaTime);
        else if (Mathf.Abs(target.y - transform.position.y) > 0.01f)
            newPos.y = Mathf.MoveTowards(transform.position.y, target.y, moveSpeed * Time.deltaTime);

        _movement = newPos - transform.position;
        transform.position = newPos;

        UpdateAnimatorCustomer(_movement);
    }

    /// <summary>
    /// Updates the animator parameters based on customer movement
    /// </summary>
    /// <param name="movement"></param>
    private void UpdateAnimatorCustomer(Vector2 movement)
    {
        bool moving = movement.x != 0 || movement.y != 0;

        if (moving)
        {
            animator.SetFloat("HorizontalMovement", movement.x);
            animator.SetFloat("VerticalMovement", movement.y);
            _previousMovement = movement;
        }
        else
        {
            animator.SetFloat("PreviousHorizontalMovement", _previousMovement.x);
            animator.SetFloat("PreviousVerticalMovement", _previousMovement.y);
        }

        animator.SetBool("Moving", moving);
    }

    /// <summary>
    /// Stops the customer's movement and updates animator accordingly
    /// </summary>
    private void StopMoving()
    {
        if (animator != null)
        {
            animator.SetFloat("HorizontalMovement", 0f);
            animator.SetFloat("VerticalMovement", 0f);
            animator.SetBool("Moving", false);
        }
    }

    /// <summary>
    /// Checks if the customer has reached the target position
    /// </summary>
    /// <param name="target"></param>
    /// <returns>reached or not</returns>
    private bool Reached(Vector3 target)
    {
        float distance = Vector3.Distance(transform.position, target);
        // Debug.Log($"Distance to target: {distance}");
        return distance < 0.02f;
    }

    /// <summary>
    /// Checks if the item on the counter fulfills the customer's order
    /// </summary>
    private void CheckOrder()
    {
        if (order == null)
        {
            return;
        }
        
        //Debug.Log($"Checking order. HasItem: {counterSlot.HasItem()}");

        if (!counterSlot.HasItem())
            return;

        Collectable counterItem = counterSlot.getCurrentItem();
        Debug.Log(
            $"Counter item found: {counterItem.name}, Category: {counterItem.category}, Type: {counterItem.type}");

        if (counterItem.category != CollectableCategory.FOOD)
        {
            Debug.Log("Item is not food");
            return;
        }

        Debug.Log($"Trying to fulfill order with food type: {counterItem.type}");
        Debug.Log("counterItem.getType(), order.GetRecipe().foodType" + counterItem.getType() + ", " +
                  order.GetRecipe().foodType);

        if (orderManager.TryToFurfillOrder(order, counterItem))
        {
            Debug.Log("Order fulfilled! Customer leaving");
            Destroy(counterItem.gameObject);

            // Notify queue manager that this customer is leaving
            if (isAtCounter)
            {
                orderManager.OnCustomerLeavingCounter();
                isAtCounter = false;
            }

            state = CustomerState.Leaving;
            order = null;
        }
        else
        {
            Debug.Log("Order NOT fulfilled - wrong item type");
        }
    }

    /// <summary>
    /// Handles order expiration event
    /// </summary>
    /// <param name="expiredOrder"> The order that has expired.</param>
    private void OnOrderExpired(Order expiredOrder)
    {
        if (expiredOrder != order)
        {
            return;
        }

        if (isAtCounter)
        {
            orderManager.OnCustomerLeavingCounter();
        }

        state = CustomerState.Leaving;
    }

    /// <summary>
    /// Cleans up event subscriptions on destroy
    /// </summary>
    private void OnDestroy()
    {
        if (order != null)
        {
            order.OnOrderExpired -= OnOrderExpired;
        }
    }
    
    /// <summary>
    /// Gets the current state of the customer
    /// </summary>
    /// <returns>The current CustomerState</returns>
    public CustomerState getState()
    {
        return state;
    }
    
    public Sprite GetCustomerPortraitImage()
    {
        return customerPortraitImage;
    }
}