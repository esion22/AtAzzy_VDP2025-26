using System;
using UnityEngine;

namespace Orders
{
    [Serializable]
    public class DayStats
    {
        public int customersServed;
        public int ordersCompleted;
        public int ordersFailed;
        public int moneyEarned;
        public int currentDay;

        public DayStats()
        {
            customersServed = 0;
            ordersCompleted = 0;
            ordersFailed = 0;
            moneyEarned = 0;
            currentDay = 1;
        }

        public void Reset()
        {
            customersServed = 0;
            ordersCompleted = 0;
            ordersFailed = 0;
            moneyEarned = 0;
        }
    }
}