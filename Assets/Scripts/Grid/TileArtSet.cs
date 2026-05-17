using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defence/Tile Art Set", fileName = "TileArtSet")]
public class TileArtSet : ScriptableObject
{
    [Header("Ground variants")]
    public List<Sprite> groundTiles = new();

    [Header("Optional path tile")]
    public Sprite pathTile;

    public Sprite GetGroundTile(Vector2Int cell)
    {
        if (groundTiles == null || groundTiles.Count == 0)
            return null;

        int hash = cell.x * 73856093 ^ cell.y * 19349663;
        int index = Mathf.Abs(hash) % groundTiles.Count;
        return groundTiles[index];
    }
}