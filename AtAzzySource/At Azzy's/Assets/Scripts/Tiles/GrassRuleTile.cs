using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Grass Rule Tile")]
public class GrassRuleTile : RuleTile<GrassRuleTile.Neighbor>
{
    public TileBase[] ignoredTiles;

    public class Neighbor : RuleTile.TilingRule.Neighbor { }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case TilingRule.Neighbor.This:
                if (tile == this) return true;
                if (ignoredTiles != null)
                {
                    foreach (var ignoredTile in ignoredTiles)
                        if (tile == ignoredTile)
                            return true;
                }
                return false;

            case TilingRule.Neighbor.NotThis:
                if (tile == this) return false;
                if (ignoredTiles != null)
                {
                    foreach (var ignoredTile in ignoredTiles)
                        if (tile == ignoredTile)
                            return false;
                }
                return true;

            default:
                return base.RuleMatch(neighbor, tile);
        }
    }
}