using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private List<Transform> waypoints = new();

    [Header("Grid Reference")]
    [SerializeField] private GridManager gridManager;

    [Header("Fallback")]
    [SerializeField] private float fallbackCellSize = 1f;

    private List<Vector2Int> pathCells = new();
    public IReadOnlyList<Vector2Int> PathCells => pathCells;
    public IReadOnlyList<Transform> Waypoints => waypoints;
    public int WaypointCount => waypoints != null ? waypoints.Count : 0;

    private void Awake()
    {
        ResolveGridManager();
        RebuildPathCells();
    }

    private void OnEnable()
    {
        ResolveGridManager();
    }

    private void OnValidate()
    {
        ResolveGridManager();

        if (!Application.isPlaying)
        {
            RebuildPathCells();
        }
    }

    public void RebuildPathCells()
    {
        if (pathCells == null)
            pathCells = new List<Vector2Int>();

        pathCells.Clear();

        if (waypoints == null || waypoints.Count < 2)
            return;

        bool hasPrevious = false;
        Vector2Int previousCell = default;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Transform waypoint = waypoints[i];
            if (waypoint == null)
                continue;

            Vector2Int currentCell = WorldToGrid(waypoint.position);

            if (!hasPrevious)
            {
                AddUniqueCell(currentCell);
                previousCell = currentCell;
                hasPrevious = true;
                continue;
            }

            AddLineCells(previousCell, currentCell);
            previousCell = currentCell;
        }
    }

    public void SetWaypoints(List<Transform> points)
    {
        waypoints = points;
        RebuildPathCells();
    }

    public void SetPathCells(List<Vector2Int> cells)
    {
        pathCells = cells;
    }

    public Vector3 GetWaypointPosition(int index)
    {
        if (index < 0 || waypoints == null || index >= waypoints.Count)
            return Vector3.zero;

        Transform waypoint = waypoints[index];
        return waypoint != null ? waypoint.position : Vector3.zero;
    }

    public Vector3 GetSpawnPosition()
    {
        if (waypoints == null)
            return Vector3.zero;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
                return waypoints[i].position;
        }

        return Vector3.zero;
    }

    public Vector3 GetBasePosition()
    {
        if (waypoints == null)
            return Vector3.zero;

        for (int i = waypoints.Count - 1; i >= 0; i--)
        {
            if (waypoints[i] != null)
                return waypoints[i].position;
        }

        return Vector3.zero;
    }

    private void ResolveGridManager()
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        if (gridManager != null)
            return gridManager.WorldToGrid(worldPos);

        int x = Mathf.RoundToInt(worldPos.x / Mathf.Max(0.0001f, fallbackCellSize));
        int y = Mathf.RoundToInt(worldPos.y / Mathf.Max(0.0001f, fallbackCellSize));
        return new Vector2Int(x, y);
    }

    private void AddLineCells(Vector2Int from, Vector2Int to)
    {
        int x0 = from.x;
        int y0 = from.y;
        int x1 = to.x;
        int y1 = to.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = -Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            AddUniqueCell(new Vector2Int(x0, y0));

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;

            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private void AddUniqueCell(Vector2Int cell)
    {
        if (pathCells.Count == 0 || pathCells[pathCells.Count - 1] != cell)
            pathCells.Add(cell);
    }
}