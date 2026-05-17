using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField] private int gridWidth = 12;
    [SerializeField] private int gridHeight = 8;
    [SerializeField] private float cellSize = 1f;

    [Header("Prefabs")]
    [SerializeField] private TileView tilePrefab;

    [Header("References")]
    [SerializeField] private Transform gridRoot;
    [SerializeField] private PathManager pathManager;

    private readonly Dictionary<Vector2Int, TileView> tiles = new Dictionary<Vector2Int, TileView>();

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;

    private void Awake()
    {
        if (gridRoot == null)
        {
            GameObject root = new GameObject("GridRoot");
            root.transform.SetParent(transform);
            gridRoot = root.transform;
        }
    }

    private void Start()
    {
        GenerateGrid();
        ApplyPathToGrid();
    }

    public void GenerateGrid()
    {
        ClearGrid();
        tiles.Clear();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = GridToWorld(gridPos);

                TileView tile = Instantiate(tilePrefab, worldPos, Quaternion.identity, gridRoot);
                tile.name = $"Tile_{x}_{y}";
                tile.Initialize(gridPos, false);

                tiles.Add(gridPos, tile);
            }
        }
    }

    public void ApplyPathToGrid()
    {
        if (pathManager == null)
            return;

        foreach (Vector2Int pathCell in pathManager.PathCells)
        {
            if (tiles.TryGetValue(pathCell, out TileView tile))
            {
                tile.SetPath(true);
            }
        }
    }

    public bool IsInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight;
    }

    public bool IsPathCell(Vector2Int cell)
    {
        return pathManager != null && pathManager.PathCells.Contains(cell);
    }

    public Vector3 GridToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * cellSize, cell.y * cellSize, 0f);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / cellSize);
        int y = Mathf.RoundToInt(worldPos.y / cellSize);
        return new Vector2Int(x, y);
    }

    public TileView GetTile(Vector2Int cell)
    {
        tiles.TryGetValue(cell, out TileView tile);
        return tile;
    }

    private void ClearGrid()
    {
        for (int i = gridRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(gridRoot.GetChild(i).gameObject);
        }
    }
}