using System.Collections;
using System.Collections.Generic;
using PixelAdventures.Scripts.Completed_Scripts;
using UnityEngine;

namespace CookingStations
{
    public class StoveStation: AbstractCookingStation
    {
        [SerializeField] private Sprite restingSprite;
        [SerializeField] private Sprite readySprite;
        [SerializeField] private List<Sprite> cookingSprites;
        private SpriteRenderer _spriteRenderer;

        private bool _isAnimating = false;
        private Coroutine _animationCoroutine;
        
        /*
         * Start is called once before the first execution of Update after the MonoBehaviour is created
         */
        protected override void Start()
        {
            base.Start();
            
            // initialize the sprite, get the sprite renderer component
            SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                _spriteRenderer = spriteRenderer;
                _spriteRenderer.sprite = restingSprite;
            }
        }

        public override void TryToAddIngredient(PlayerItemController player)
        {
            base.TryToAddIngredient(player);
            // if the ingredient was added successfully, update the sprite
            if (_spriteRenderer != null && CookingSlots.Count > 0)
            {
                _spriteRenderer.sprite = readySprite;
            }
        }

        /*
         * Called when a spell hits the cooking station
         * @param spellType The type of spell that hit the cooking station
         */
        public override void OnSpellHit(SpellEnum spellType)
        {
            if (spellsNeededToWork.Contains(spellType))
            {
                if ((CurrentState == CookingStationStateEnum.READY || CurrentState == CookingStationStateEnum.COOKING) && spellType == SpellEnum.FIRE && CookingSlots.Count > 0)
                {
                    OnSpellReceived?.Invoke(spellType);
                    audioManager.PlayStoveSound();
                    if (CurrentState == CookingStationStateEnum.READY)
                    {
                        CurrentState = CookingStationStateEnum.COOKING;
                    }
                    
                    _spellHistory.Add(spellType);
                    
                    if (cookingBarSlider != null)
                    {
                        cookingBarSlider.gameObject.SetActive(true);
                    }

                    CurrentVelocity = cookingVelocity;

                    if (_isAnimating)
                        return;
                    
                    // change to the first cooking sprite
                    if (_spriteRenderer != null && cookingSprites.Count > 0)
                    {
                        _spriteRenderer.sprite = cookingSprites[0];
                    }
                    
                    _animationCoroutine = StartCoroutine(AnimateCookingSprites());
                }
            }
        }
        
        /*
         * Central coroutine to animate the cooking sprites while in COOKING state
         */
        private IEnumerator AnimateCookingSprites()
        {
            if (cookingSprites == null || cookingSprites.Count == 0)
                yield break;

            _isAnimating = true;

            // continue animating while in COOKING state
            while (CurrentState == CookingStationStateEnum.COOKING)
            {
                for (int i = 0; i < cookingSprites.Count; i++)
                {
                    if (CurrentState != CookingStationStateEnum.COOKING)
                        break;

                    if (_spriteRenderer != null)
                        _spriteRenderer.sprite = cookingSprites[i];

                    yield return new WaitForSeconds(0.1f);
                }

                // after finishing the animation cycle, check if still cooking
                if (CurrentState == CookingStationStateEnum.COOKING && _spriteRenderer != null)
                {
                    _spriteRenderer.sprite = cookingSprites[0];
                }
            }

            _isAnimating = false;
            _animationCoroutine = null;
        }
        
        /*
         * Updates the cooking bar based on the current state and velocity
         */
        public override void UpdateCookingBar()
        {
            if (isDiscrete == false)
            {
                // update the cooking bar based on the current state
                if (CurrentState == CookingStationStateEnum.COOKING)
                {
                    // update the cooking bar value based on the current velocity
                    CookingBarValue += CurrentVelocity * Time.deltaTime;

                    // if it is less than 0, clamp it to 0
                    if (CookingBarValue < 0)
                    {
                        CookingBarValue = 0;
                    }

                    // if it is 0, change the state to READY
                    if (CookingBarValue == 0)
                    {
                        audioManager.PlayStoveSound(true);
                        CurrentState = CookingStationStateEnum.READY;
                        // change the sprite
                        if (_spriteRenderer != null)
                        {
                            _spriteRenderer.sprite = cookingSprites[0];
                        }

                        if (cookingBarSlider != null)
                        {
                            cookingBarSlider.gameObject.SetActive(false);
                        }
                    }

                    // decelerate the cooking velocity
                    CurrentVelocity -= cookingDeceleration * Time.deltaTime;
                    // clamp the cooking velocity to the minimum negative velocity
                    if (CurrentVelocity <= minNegativeVelocity)
                    {
                        CurrentVelocity = minNegativeVelocity;
                    }

                    if (cookingBarSlider != null)
                    {
                        cookingBarSlider.value = CookingBarValue;
                    }
                }
            }
        }
        
        /*
         * Checks if the cooking is ready and updates the state accordingly
         */
        protected override void CookingReady()
        {
            // if the cooking bar value is greater than the done threshold
            if (CookingBarValue >= doneThreshold)
            {
                audioManager.PlayStoveSound(true);
                CurrentState = CookingStationStateEnum.DONE;
            
                // change the sprite
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.sprite = readySprite;
                }
            
                if (cookingBarSlider != null) 
                {
                    cookingBarSlider.gameObject.SetActive(false);
                }
            
                // create the food item based on the cooking slots
                if (CookingSlots.Count > 0)
                {
                    // get all the ingredient types from the cooking slots
                    ArrayList ingredientType = new ArrayList();
                    foreach (CollectableType ct in CookingSlots)
                    {
                        // add the ingredient type to the list
                        ingredientType.Add(ct);
                    }
                
                    // check if the ingredient types match any recipe
                    CollectableType acceptedType = CheckRecipes(ingredientType);
                
                    // if a recipe is found, set the output slot to the resulting food type
                    if (acceptedType != CollectableType.TRASH)
                    {
                        if (DataManager.Instance != null)
                        {
                            DataManager.Instance.AddRecipeDone();
                        }
                        // invoke the recipe completed event for the tutorial
                        OnRecipeCompleted?.Invoke(acceptedType);
                        
                        OutputSlot = acceptedType;
                        
                        audioManager.PlayCookingReadySound();
                    }
                    else
                    {
                        if (DataManager.Instance != null)
                        {
                            DataManager.Instance.AddRecipeFailed();
                        }
                        OnCookingFailed?.Invoke();
                        Debug.Log("Cutting Board: No recipe found for ingredients.");
                        // reset the input slots and put the cooking station back to resting state
                        OutputSlot = acceptedType;
                        
                        audioManager.PlayCookingFailedSound();
                    }
                
                    // clear the cooking slots
                    CookingSlots.Clear();
                    UpdateVisualFeedback();
                }
            }
        }
        
        /*
         * Resets the cooking station to its initial state
         */
        protected override void ResetStation()
        {
            base.ResetStation();
            // reset the sprite to resting sprite
            if (_spriteRenderer != null)
            {            
                _spriteRenderer.sprite = restingSprite;
            }
        }
        
        private static IEnumerator WaitAndExecute(float seconds, System.Action callback)
        {
            if (seconds > 0f)
                yield return new WaitForSeconds(seconds);
            else
                yield return null;

            callback?.Invoke();
        }
    }
}