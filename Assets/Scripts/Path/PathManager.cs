using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    [Header("Path Cells on Grid")]
    [SerializeField] private List<Vector2Int> pathCells = new List<Vector2Int>();

    [Header("Waypoints")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    public IReadOnlyList<Vector2Int> PathCells => pathCells;
    public IReadOnlyList<Transform> Waypoints => waypoints;

    public int WaypointCount => waypoints.Count;

    public Vector3 GetWaypointPosition(int index)
    {
        if (index < 0 || index >= waypoints.Count)
            return Vector3.zero;

        return waypoints[index].position;
    }

    public Vector3 GetSpawnPosition()
    {
        if (waypoints.Count == 0)
            return Vector3.zero;

        return waypoints[0].position;
    }

    public Vector3 GetBasePosition()
    {
        if (waypoints.Count == 0)
            return Vector3.zero;

        return waypoints[waypoints.Count - 1].position;
    }

    public void SetPathCells(List<Vector2Int> cells)
    {
        pathCells = cells;
    }

    public void SetWaypoints(List<Transform> points)
    {
        waypoints = points;
    }
}