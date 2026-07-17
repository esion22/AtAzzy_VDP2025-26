using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace Orders
{
    public class Order : MonoBehaviour
    {
        private Recipe _recipe;
        private Image _timeRemainingBar;
        private RectTransform _timeRemainingBarRectTransform;
        private Image _customerImage;
        private int _remainingTime;
        public event Action<Order> OnOrderExpired;
        private GameObject _orderGO;
        private OrderManager _orderManager;
        private bool _isTutorialOrder = false;

        public void SetRecipe(Recipe recipe)
        {
            _recipe = recipe;
            SetImages();
            _remainingTime = recipe.orderTime;
            
            if (!_isTutorialOrder)
                StartCoroutine(OrderTimer());
            try
            {
                _orderManager = Utils.FindGameObjectInScene("OrderManager").GetComponent<OrderManager>();
            }
            catch (Exception e)
            {
                _orderManager = Utils.FindGameObjectInScene("PrototypeOrderManager").GetComponent<OrderManager>();
            }
        }

        public Recipe GetRecipe()
        {
            return _recipe;
        }

        private void SetImages()
        {
            Image[] images = gameObject.GetComponentsInChildren<Image>();
            foreach (Image i in images)
            {
                string name = i.gameObject.name;
                switch (name)
                {
                    case "OrderTimer":
                        _timeRemainingBar = i;
                        _timeRemainingBarRectTransform = i.GetComponent<RectTransform>();
                        break;
                    case "Customer":
                        _customerImage = i;
                        break;
                    case "Recipe":
                        i.sprite = _recipe.image;
                        break;
                    case "Order(Clone)":
                        _orderGO = i.gameObject;
                        break;
                    default:
                        Debug.LogError("While creating an order, found game object of name "+ name + " with image component" +
                                       " that was not expected");
                        break;
                }
            }
        }
        
        public void SetCustomerImage(Sprite customerImage)
        {
            _customerImage.sprite = customerImage;
        }
        
        IEnumerator OrderTimer()
        {
            var nextTime = Time.time + 1f;

            while (true)
            {
                yield return new WaitUntil(() => Time.time >= nextTime);

                _remainingTime--;

                if (_timeRemainingBarRectTransform != null)
                {
                    _timeRemainingBarRectTransform.localScale =
                        new Vector3((float)_remainingTime / _recipe.orderTime, 1f, 1f);
                }

                if (_remainingTime <= 0)
                {
                    DataManager.Instance?.AddRecipeFailed();
                    OnOrderExpired?.Invoke(this);
                    _orderManager.DeleteOrder(this);

                    yield break;
                }

                nextTime += 1f;
            }
        }





        
        /**
         * Sets whether this is a tutorial order (no expiration).
         * @param {bool} isTutorial - True if tutorial order.
         */
        public void SetTutorialMode(bool isTutorial)
        {
            _isTutorialOrder = isTutorial;
        }
    
        /**
         * Returns whether this is a tutorial order.
         * @returns {bool} True if tutorial order.
         */
        public bool IsTutorialOrder()
        {
            return _isTutorialOrder;
        }
    }
}
