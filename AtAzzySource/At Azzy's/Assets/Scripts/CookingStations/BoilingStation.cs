using System;
using System.Collections;
using System.Collections.Generic;
using CookingStations;
using NUnit.Framework;
using PixelAdventures.Scripts.Completed_Scripts;
using UnityEngine;
using UnityEngine.UI;

public class BoilingStation: AbstractCookingStation
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

    /*
     * Called when a spell hits the cooking station
     * @param spellType The type of spell that hit the cooking station
     */
    public override void OnSpellHit(SpellEnum spellType)
    {
        if (spellsNeededToWork.Contains(spellType))
        {
            if ((CurrentState == CookingStationStateEnum.RESTING || CurrentState == CookingStationStateEnum.READY) && spellType == SpellEnum.WATER)
            {
                OnSpellReceived?.Invoke(spellType);
                CurrentState = CookingStationStateEnum.READY;
                // change the sprite
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.sprite = readySprite;
                }
                
                _spellHistory.Add(spellType);
            }
            if ((CurrentState == CookingStationStateEnum.READY || CurrentState == CookingStationStateEnum.COOKING) && spellType == SpellEnum.FIRE && CookingSlots.Count > 0 && _spellHistory.Contains(SpellEnum.WATER))
            {
                OnSpellReceived?.Invoke(spellType);
                audioManager.PlayBoilingSound();
                CurrentState = CookingStationStateEnum.COOKING;
                
                _spellHistory.Add(spellType);
                
                // call the discrete cooking bar update if needed
                UpdateCookingBarDiscrete();
                
                if (cookingBarSlider != null) 
                {
                    cookingBarSlider.gameObject.SetActive(true);
                }
                
                // update the velocity to start cooking
                CurrentVelocity = cookingVelocity;
                
                if (_isAnimating)
                    return;

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
                    audioManager.PlayBoilingSound(true);
                    CurrentState = CookingStationStateEnum.READY;
                    // change the sprite
                    if (_spriteRenderer != null)
                    {
                        _spriteRenderer.sprite = readySprite;
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
            audioManager.PlayBoilingSound(true);
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
                    OutputSlot = acceptedType;
                    // invoke the recipe completed event for the tutorial
                    OnRecipeCompleted?.Invoke(acceptedType);
                    
                    if (DataManager.Instance != null)
                    {
                        DataManager.Instance.AddRecipeDone();
                    }
                    
                    audioManager.PlayCookingReadySound();
                }
                else
                {
                    if (DataManager.Instance != null)
                    {
                        DataManager.Instance.AddRecipeFailed();
                    }
                    // invoke the cooking failed event for the tutorial
                    OnCookingFailed?.Invoke();
                    Debug.Log("Boiling Station: No recipe found for ingredients.");
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
