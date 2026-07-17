using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Orders
{
    public class EndOfDayManager: MonoBehaviour
    {
        public static EndOfDayManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private OrderManager _orderManager;
        [SerializeField] private EndOfDayUI _endOfDayUI;

        [Header("Settings")]
        [SerializeField] private float _transitionDuration = 1.5f;
        [SerializeField] private float delayBeforeTransition = 1f;

        private DayStats _currentDayStats;
        private bool _dayEnded = false;

        public UnityEvent OnDayEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _currentDayStats = new DayStats();
            _currentDayStats.currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
        }

        private void Start()
        {
            if (_orderManager == null)
            {
                _orderManager = FindFirstObjectByType<OrderManager>();
            }

            if (_endOfDayUI == null)
            {
                _endOfDayUI = FindFirstObjectByType<EndOfDayUI>();
            }

            if (GameManager.instance != null)
            {
                _currentDayStats.currentDay = GameManager.instance.GetCurrentDay();
            }

            // Instance.CheckForDayEnd();
        }

        private void Update(){}
        
        public void CheckForDayEnd()
        {
            if (_orderManager == null) return;
            
            // Do not end the day if the cooking tutorial is active
            if (Tutorial.CookingTutorial.Instance != null && PlayerPrefs.GetInt("CookingTutorialDone", 0) == 0)
            {
                return;
            }

            bool allOrdersProcessed = _orderManager.AreAllOrdersProcessed();
            bool noIngredientsLeft = _orderManager.HasNoValidRecipes();

            if (allOrdersProcessed || noIngredientsLeft)
            {
                TriggerEndOfDay();
            }
        }

        public void TriggerEndOfDay()
        {
            if (_dayEnded) return;
            _dayEnded = true;

            StartCoroutine(EndOfDaySequence());
        }

        private IEnumerator EndOfDaySequence()
        {
            // Attendi prima di iniziare la transizione
            yield return new WaitForSeconds(delayBeforeTransition);

            // Raccogli statistiche da OrderManager
            _currentDayStats.ordersCompleted = _orderManager.GetSuccessfulOrders();
            _currentDayStats.ordersFailed = _orderManager.GetFailedOrders();
            _currentDayStats.moneyEarned = _orderManager.GetMoneyGained();
            _currentDayStats.customersServed = _currentDayStats.ordersCompleted + _currentDayStats.ordersFailed;

            OnDayEnded?.Invoke();

            if (_endOfDayUI != null)
            {
                _endOfDayUI.ShowEndOfDayScreen(_currentDayStats, _transitionDuration);
            }
        }

        public void EndDayAndGoToFarm()
        {
            Inventory playerInventory = FindFirstObjectByType<PlayerItemController>().GetInventory();
            playerInventory.RestoreFilteredItems();
            playerInventory.RemoveBurntAndTrashItems();
            
            // Increases the current day count
            if (GameManager.instance != null)
            {
                GameManager.instance.IncrementDay();
            }

            // Reset time scale
            Time.timeScale = 1f;
            
            if (_endOfDayUI != null)
            {
                Destroy(_endOfDayUI.gameObject);
            }

            // Load the farm scene
            SceneController sceneController = FindFirstObjectByType<SceneController>();
            if (sceneController != null)
            {
                sceneController.LoadFarm();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("FarmScene");
            }
        }

        public DayStats GetCurrentDayStats()
        {
            return _currentDayStats;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}