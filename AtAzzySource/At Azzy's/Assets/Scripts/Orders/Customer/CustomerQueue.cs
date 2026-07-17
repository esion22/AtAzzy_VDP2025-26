using System.Collections.Generic;
using System.Linq;
using Orders.Customer;
using UnityEngine;

/// <summary>
/// Manages the queue of customers waiting at a counter
/// </summary>
public class CustomerQueueManager : MonoBehaviour
{
    [Header("Queue Settings")] [SerializeField]
    private Transform counterPoint;

    [SerializeField] private float spaceBetweenCustomers = 1.5f;
    [SerializeField] private int maxQueueSize = 5;

    private Queue<Customer> waitingCustomers = new Queue<Customer>();
    private Customer currentCustomerAtCounter;

    /// <summary>
    /// Checks if the queue is full
    /// </summary>
    /// <returns>True if full, false otherwise</returns>
    public bool IsFull()
    {
        if (waitingCustomers.Count >= maxQueueSize)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a customer to the queue or sends them directly to the counter if it's free
    /// </summary>
    /// <param name="customer">The customer to add</param>
    public void AddCustomerToQueue(Customer customer)
    {
        if (IsFull())
        {
            Debug.LogWarning("Queue is full! Customer cannot join.");
            return;
        }

        // If counter is empty, send customer directly to counter (don't add to queue)
        if (currentCustomerAtCounter == null)
        {
            currentCustomerAtCounter = customer;
            customer.MoveToCounter();
            Debug.Log("Customer sent directly to counter (no queue needed)");
        }
        else
        {
            // Counter is full, add to queue
            waitingCustomers.Enqueue(customer);
            UpdateQueuePositions();
            Debug.Log($"Customer added to queue. Queue length: {waitingCustomers.Count}");
        }
    }

    /// <summary>
    /// Moves the next customer in the queue to the counter
    /// </summary>
    public void MoveNextCustomerToCounter()
    {
        if (currentCustomerAtCounter != null)
        {
            return;
        }

        while (waitingCustomers.Count > 0)
        {
            Customer customer = waitingCustomers.Dequeue();

            if (customer.getState() == CustomerState.WaitingInQueue)
            {
                currentCustomerAtCounter = customer;
                customer.MoveToCounter();
                UpdateQueuePositions();
                break;
            }
        }
    }

    /// <summary>
    /// Called when the current customer leaves the counter
    /// </summary>
    public void OnCustomerLeftCounter()
    {
        Debug.Log("Customer left counter, moving next customer");
        currentCustomerAtCounter = null;
        MoveNextCustomerToCounter();
    }

    /// <summary>
    /// Updates the positions of customers in the queue
    /// </summary>
    private void UpdateQueuePositions()
    {
        int index = 0;
        foreach (Customer customer in waitingCustomers)
        {
            Vector3 queuePosition = counterPoint.position +
                                    (counterPoint.right * spaceBetweenCustomers * (index + 1));
            customer.MoveToQueuePosition(queuePosition);
            index++;
        }
    }

    /// <summary>
    /// Gets the position for the next customer to join the queue
    /// </summary>
    /// <returns>The next queue position</returns>
    public Vector3 GetNextQueuePosition()
    {
        int index = waitingCustomers.Count;
        Vector3 nextPosition = counterPoint.position + (counterPoint.right * spaceBetweenCustomers * (index + 1));
        return nextPosition;
    }

}