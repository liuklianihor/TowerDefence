using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameEconomy economy;
    [SerializeField] private GameStateManager gameStateManager;

    [Header("Tower")]
    [SerializeField] private TowerBase towerPrefab;
    [SerializeField] private TowerData defaultTower;
    [SerializeField] private Transform towerRoot;

    private readonly HashSet<Vector2Int> occupiedCells = new();

    private void Awake()
    {
        if (worldCamera == null) worldCamera = Camera.main;
        if (towerRoot == null)
        {
            var root = new GameObject("Towers");
            towerRoot = root.transform;
        }
    }

    private void Update()
    {
        if (gameStateManager != null && gameStateManager.CurrentPhase != GamePhase.Preparation)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlaceTowerAtMouse();
        }
    }

    public bool TryPlaceTowerAtMouse()
    {
        if (worldCamera == null || gridManager == null || towerPrefab == null || defaultTower == null)
        {
            return false;
        }

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -worldCamera.transform.position.z));
        worldPos.z = 0f;

        Vector2Int cell = gridManager.WorldToGrid(worldPos);
        return TryPlaceTower(cell, defaultTower);
    }

    public bool TryPlaceTower(Vector2Int cell, TowerData towerData)
    {
        if (gridManager == null || towerPrefab == null || towerData == null)
        {
            return false;
        }

        if (!gridManager.IsInsideGrid(cell) || gridManager.IsPathCell(cell) || occupiedCells.Contains(cell))
        {
            return false;
        }

        if (economy != null && !economy.SpendGold(towerData.cost))
        {
            return false;
        }

        Vector3 worldPos = gridManager.GridToWorld(cell);
        TowerBase tower = Instantiate(towerPrefab, worldPos, Quaternion.identity, towerRoot);
        tower.Initialize(towerData);
        occupiedCells.Add(cell);
        return true;
    }

    public void ClearPlacedTowers()
    {
        occupiedCells.Clear();
        for (int i = towerRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(towerRoot.GetChild(i).gameObject);
        }
    }
}
