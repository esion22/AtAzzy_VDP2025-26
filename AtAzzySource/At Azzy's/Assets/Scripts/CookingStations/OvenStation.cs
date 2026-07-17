using System;
using System.Collections;
using System.Collections.Generic;
using CookingStations;
using NUnit.Framework;
using PixelAdventures.Scripts.Completed_Scripts;
using UnityEngine;
using UnityEngine.UI;

public class OvenStation: AbstractCookingStation
{
    [SerializeField] private Sprite restingSprite;
    [SerializeField] private List<Sprite> cookingSprites;
    [SerializeField] private float startGoodInterval;
    [SerializeField] private float endGoodInterval;
    [SerializeField] private RectTransform goodIntervalIndicator;
    [SerializeField] private RectTransform burntIntervalIndicator;
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
        
        if ((goodIntervalIndicator != null || burntIntervalIndicator != null) && cookingBarSlider != null)
        {
            // start setting the indicator rects
            SetIndicatorRect();
        }
    }
    
    private void SetIndicatorRect()
    {
        float sliderRange = cookingBarSlider.maxValue - cookingBarSlider.minValue;
        if (sliderRange <= 0) return;

        if (goodIntervalIndicator != null)
        {
            // Normalize the start and end values to a 0-1 range based on the slider's min and max values
            float normalizedStart = (startGoodInterval - cookingBarSlider.minValue) / sliderRange;
            float normalizedEnd = (endGoodInterval - cookingBarSlider.minValue) / sliderRange;

            // set only the horizontal anchors to position and scale the indicator
            goodIntervalIndicator.anchorMin = new Vector2(normalizedStart, goodIntervalIndicator.anchorMin.y);
            goodIntervalIndicator.anchorMax = new Vector2(normalizedEnd, goodIntervalIndicator.anchorMax.y);
        }

        if (burntIntervalIndicator != null)
        {
            // the "burnt" indicator starts at the end of the good interval
            float normalizedStart = (endGoodInterval - cookingBarSlider.minValue) / sliderRange;
            // the "burnt" indicator ends at the max value of the slider
            float normalizedEnd = 1f;

            // set only the horizontal anchors to position and scale the indicator
            burntIntervalIndicator.anchorMin = new Vector2(normalizedStart, burntIntervalIndicator.anchorMin.y);
            burntIntervalIndicator.anchorMax = new Vector2(normalizedEnd, burntIntervalIndicator.anchorMax.y);
        }
    }
    
    // Update is called once per frame
    protected override void Update()
    {
        // update the cooking bar if in cooking state
        if (CurrentState == CookingStationStateEnum.COOKING)
        {
            UpdateCookingBar();
            if (cookingBarSlider != null)
            {
                cookingBarSlider.value = CookingBarValue;
            }
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
            if ((CurrentState == CookingStationStateEnum.READY || CurrentState == CookingStationStateEnum.COOKING) &&
                spellType == SpellEnum.FIRE && CookingSlots.Count > 0)
            {
                OnSpellReceived?.Invoke(spellType);
                audioManager.PlayOvenSound();
                CurrentState = CookingStationStateEnum.COOKING;
                _spellHistory.Add(spellType);

                if (cookingBarSlider != null)
                {
                    cookingBarSlider.gameObject.SetActive(true);
                }

                if (CurrentVelocity == 0f)
                {
                    CurrentVelocity = cookingVelocity;
                }
                
                if (_isAnimating)
                    return;
                
                if (_spriteRenderer != null && cookingSprites != null && cookingSprites.Count > 0)
                {
                    _spriteRenderer.sprite = cookingSprites[0];
                }
                
                _animationCoroutine = StartCoroutine(AnimateCookingSprites());
            }

            if (CurrentState == CookingStationStateEnum.COOKING && spellType == SpellEnum.WATER &&
                CookingSlots.Count > 0)
            {
                OnSpellReceived?.Invoke(spellType);
                audioManager.PlayOvenSound(true);
                CurrentVelocity = 0f;
                
                // if the player stopped the cooking at the correct time
                if (CookingBarValue >= startGoodInterval && CookingBarValue <= endGoodInterval)
                {
                    CurrentState = CookingStationStateEnum.DONE;
                    CookingReady();
                }
                else
                {
                    if (CookingBarValue < startGoodInterval)
                    {
                        CurrentState = CookingStationStateEnum.READY;
                    }
                    else if (CookingBarValue > endGoodInterval)
                    {
                        // change the cooking slots in order to produce a burnt recipe
                        CookingSlots.Clear();
                        // add the burnt CollectableType
                        CookingSlots.Add(CollectableType.BURNT);
                        CookingReady();
                    }
                }
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

        // Continue animating while in COOKING state
        while (CurrentState == CookingStationStateEnum.COOKING)
        {
            for (int i = 0; i < cookingSprites.Count; i++)
            {
                if (CurrentState != CookingStationStateEnum.COOKING)
                    break;

                if (_spriteRenderer != null)
                    _spriteRenderer.sprite = cookingSprites[i];

                yield return new WaitForSeconds(0.2f);
            }

            // After finishing the animation cycle, loop back to first sprite
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
            
            // if it is greater than max, clamp it to max
            if (CookingBarValue > doneThreshold)
            {
                CookingBarValue = doneThreshold;
                CurrentVelocity = 0f;
            }

            // if it is 0, change the state to READY
            if (CookingBarValue == 0)
            {
                audioManager.PlayOvenSound(true);
                CurrentState = CookingStationStateEnum.READY;
                // change the sprite
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.sprite = restingSprite;
                }

                if (cookingBarSlider != null)
                {
                    cookingBarSlider.gameObject.SetActive(false);
                }
            }

            if (cookingBarSlider != null)
            {
                cookingBarSlider.value = CookingBarValue;
            }
        }
    }
    

    /*
     * Checks if the cooking is ready and updates the state accordingly
     */
    protected override void CookingReady()
    {
        // if there is a BURNT into the CookingSlot then put it into the OutputSlot
        if (CookingSlots.Contains(CollectableType.BURNT))
        {
            audioManager.PlayOvenSound(true);
            CurrentState = CookingStationStateEnum.DONE;
            if (DataManager.Instance != null)
            {
                DataManager.Instance.AddRecipeFailed();
            }
            
            // change the sprite
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = restingSprite;
            }
            
            if (cookingBarSlider != null) 
            {
                cookingBarSlider.gameObject.SetActive(false);
            }
            
            OutputSlot = CollectableType.BURNT;
            Debug.Log("Oven Station: HAHA, you burnt the food!");
            // invoke the cooking failed event
            OnRecipeCompleted?.Invoke(CollectableType.BURNT);
            audioManager.PlayCookingFailedSound();
            // reset the input slots and put the cooking station back to resting state
            UpdateVisualFeedback();
        }
        // if the cooking bar value is in the correct interval
        if (CookingBarValue >= startGoodInterval &&  CookingBarValue <= endGoodInterval)
        {
            audioManager.PlayOvenSound(true);
            CurrentState = CookingStationStateEnum.DONE;
            
            // change the sprite
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = restingSprite;
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
                    Debug.Log("Oven Station: No recipe found for ingredients.");
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
    
}
