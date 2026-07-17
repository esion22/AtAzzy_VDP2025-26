using System;
using PixelAdventures.Scripts.Completed_Scripts;
using UnityEngine;

/*
 * Interface for objects that can interact with spells
 */
public interface ISpellInteractable
{
    /*
     * Called when a spell hits the object
     * @param spellType The type of spell that hit the object
     */
    void OnSpellHit(SpellEnum spellType);
}
