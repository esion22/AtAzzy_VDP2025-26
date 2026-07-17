using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum representing different shopptypes of spells.
/// </summary>
public enum SpellType { Water, Fire, Growing, Cut }

/// <summary>
/// Enum representing different types of cooking stations.
/// </summary>
public enum StationType { CuttingBoard, Keg, Oven, Pot, Stove }

/// <summary>
/// Represents a cooking formula consisting of a station type and a list of spells.
/// </summary>
[System.Serializable]
public class CookingFormula
{
    public StationType station;
    public List<SpellType> spells = new ();
}
