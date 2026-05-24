using UnityEngine;

public enum TileKind
{
    Grass,
    Road,
    BelowRoad,
    AboveRoad,
    RightOfRoad,
    LeftOfRoad,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

[CreateAssetMenu(menuName = "Tower Defence/Tile Art Set", fileName = "TileArtSet")]
public class TileArtSet : ScriptableObject
{
    [Header("Core")]
    [SerializeField] private Sprite roadTile;
    [SerializeField] private Sprite grassTile;

    [Header("Below road (1, 2, 3)")]
    [SerializeField] private Sprite[] belowRoad = new Sprite[3];

    [Header("Above road (33, 34, 35)")]
    [SerializeField] private Sprite[] aboveRoad = new Sprite[3];

    [Header("Right of road (8, 16, 24)")]
    [SerializeField] private Sprite[] rightOfRoad = new Sprite[3];

    [Header("Left of road (12, 20, 28)")]
    [SerializeField] private Sprite[] leftOfRoad = new Sprite[3];

    [Header("Top-left (5, 9)")]
    [SerializeField] private Sprite[] topLeft = new Sprite[2];

    [Header("Top-right (7, 11)")]
    [SerializeField] private Sprite[] topRight = new Sprite[2];

    [Header("Bottom-left (21, 25)")]
    [SerializeField] private Sprite[] bottomLeft = new Sprite[2];

    [Header("Bottom-right (23, 27)")]
    [SerializeField] private Sprite[] bottomRight = new Sprite[2];

    public Sprite GetTile(TileKind kind)
    {
        return kind switch
        {
            TileKind.Road => roadTile,
            TileKind.BelowRoad => PickRandom(belowRoad),
            TileKind.AboveRoad => PickRandom(aboveRoad),
            TileKind.RightOfRoad => PickRandom(rightOfRoad),
            TileKind.LeftOfRoad => PickRandom(leftOfRoad),
            TileKind.TopLeft => PickRandom(topLeft),
            TileKind.TopRight => PickRandom(topRight),
            TileKind.BottomLeft => PickRandom(bottomLeft),
            TileKind.BottomRight => PickRandom(bottomRight),
            _ => grassTile
        };
    }

    private Sprite PickRandom(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0)
            return null;

        return sprites[Random.Range(0, sprites.Length)];
    }
}