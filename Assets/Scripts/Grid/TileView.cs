using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TileView : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector2Int gridPosition;
    private bool isPath;

    public Vector2Int GridPosition => gridPosition;
    public bool IsPath => isPath;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2Int position, Sprite sprite, bool path)
    {
        gridPosition = position;
        spriteRenderer.sprite = sprite;
        isPath = path;
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void SetPath(bool path)
    {
        isPath = path;
    }
}