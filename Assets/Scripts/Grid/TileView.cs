using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TileView : MonoBehaviour
{
    [SerializeField] private Color normalColor = new Color(0.85f, 0.85f, 0.85f);
    [SerializeField] private Color pathColor = new Color(0.65f, 0.45f, 0.2f);

    private SpriteRenderer spriteRenderer;
    private Vector2Int gridPosition;
    private bool isPath;

    public Vector2Int GridPosition => gridPosition;
    public bool IsPath => isPath;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2Int position, bool path)
    {
        gridPosition = position;
        SetPath(path);
    }

    public void SetPath(bool path)
    {
        isPath = path;
        spriteRenderer.color = isPath ? pathColor : normalColor;
    }
}