using System;
using System.Collections;
using UnityEngine;

namespace PixelAdventures.Scripts.Completed_Scripts 
{
    /*
     * This class represents an object that can be targeted by spells.
     * It implements the ISpellInteractable interface to handle spell interactions.
     */
    public class SpellTargetObject : MonoBehaviour, ISpellInteractable
    {
        // Example of internal state to track spell interactions
        private ArrayList _spellHistory = new ArrayList();

        /*
         * Start is called before the first frame update
         */
        public void Start()
        {
            // make sure that highlight child is inactive at start
            Transform highlight = transform.Find("Highlight");
            if (highlight != null)
            {
                // Disable the highlight
                highlight.gameObject.SetActive(false);
            }
        }
        
        /* Implement the OnSpellHit method from ISpellInteractable
         * This method is called when the spell projectile hits this object
         * spellType indicates which spell was used
         * @param spellType The type of spell that hit the object
         */
        public void OnSpellHit(SpellEnum spellType)
        {
            // put here the code to handle the spell hit
            switch (spellType)
            {
                case SpellEnum.FIRE:
                    // Add fire effect here
                    Destroy(gameObject);
                    break;
                case SpellEnum.WATER:
                    // Add water effect here
                    Destroy(gameObject);
                    break;
                case SpellEnum.GROWTH:
                    // Add growth effect here
                    Destroy(gameObject);
                    break;
                case SpellEnum.CUT:
                    // Add cut effect here
                    Destroy(gameObject);
                    break;
                default:
                    Destroy(gameObject);
                    break;
            }
        }
    }
}