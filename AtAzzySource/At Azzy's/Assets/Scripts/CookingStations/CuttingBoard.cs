using System.Collections;
using System.Collections.Generic;
using PixelAdventures.Scripts.Completed_Scripts;
using UnityEngine;

namespace CookingStations
{
    public class CuttingBoard: AbstractCookingStation
    {
        [SerializeField] private Sprite restingSprite;
        [SerializeField] private List<Sprite> cutSprites;
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
                if ((CurrentState == CookingStationStateEnum.READY || CurrentState == CookingStationStateEnum.COOKING) && spellType == SpellEnum.CUT)
                {
                    OnSpellReceived?.Invoke(spellType);
                    audioManager.PlayCuttingSound();
                    if (CurrentState == CookingStationStateEnum.READY)
                    {
                        CurrentState = CookingStationStateEnum.COOKING;
                    }
                    
                    if (cookingBarSlider != null)
                    {
                        cookingBarSlider.gameObject.SetActive(true);
                    }
                    
                    UpdateCookingBarDiscrete();
                    
                    UpdateVisualFeedback();
                    
                    _spellHistory.Add(spellType);

                    if (_isAnimating)
                        return;

                    _animationCoroutine = StartCoroutine(AnimateCutSprites());
                }
            }
        }
        
        /*
         * Coroutine to animate the cutting sprites
         */
        private IEnumerator AnimateCutSprites()
        {
            if (cutSprites == null || cutSprites.Count == 0)
                yield break;

            _isAnimating = true;

            // Animate through each sprite in the list
            for (int i = 0; i < cutSprites.Count; i++)
            {
                if (_spriteRenderer != null)
                    _spriteRenderer.sprite = cutSprites[i];

                yield return new WaitForSeconds(0.05f);
            }

            // Return to resting sprite after animation completes
            if (_spriteRenderer != null)
                _spriteRenderer.sprite = restingSprite;

            _isAnimating = false;
            _animationCoroutine = null;
        }
        
        /*
         * Checks if the cooking is ready and updates the state accordingly
         */
        protected override void CookingReady()
        {
            // if the cooking bar value is greater than the done threshold
            if (CookingBarValue >= doneThreshold)
            {
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