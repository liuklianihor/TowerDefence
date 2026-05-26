using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerPlacementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameEconomy economy;
    [SerializeField] private GameStateManager gameStateManager;

    [Header("Tower")]
    [SerializeField] private TowerData defaultTower;
    [SerializeField] private Transform towerRoot;

    [Header("Input")]
    [SerializeField] private bool ignoreClicksOverUI = true;

    private struct PlacedTowerInfo
    {
        public Vector2Int cell;
        public TowerBase tower;
        public int cost;
    }

    private readonly Dictionary<Vector2Int, PlacedTowerInfo> placedTowers = new Dictionary<Vector2Int, PlacedTowerInfo>();
    private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    public TowerData SelectedTower { get; private set; }
    public TowerData DefaultTower => defaultTower;
    public TowerData ActiveTowerData => SelectedTower != null ? SelectedTower : defaultTower;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (towerRoot == null)
        {
            GameObject existingRoot = GameObject.Find("Towers");
            if (existingRoot == null)
                existingRoot = new GameObject("Towers");

            towerRoot = existingRoot.transform;
        }

        if (SelectedTower == null)
            SelectedTower = defaultTower;
    }

    private void Update()
    {
        if (gameStateManager != null && gameStateManager.CurrentPhase != GamePhase.Preparation)
            return;

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (ignoreClicksOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        TryPlaceTowerAtMouse();
    }

    public void SelectTower(TowerData towerData)
    {
        if (towerData == null)
            return;

        SelectedTower = towerData;
    }

    public void ClearSelection()
    {
        SelectedTower = defaultTower;
    }

    public bool TryPlaceTowerAtMouse()
    {
        TowerData towerData = ActiveTowerData;

        if (towerData == null || worldCamera == null || gridManager == null || towerData.towerPrefab == null)
            return false;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = worldCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, -worldCamera.transform.position.z)
        );
        worldPos.z = 0f;

        Vector2Int cell = gridManager.WorldToGrid(worldPos);
        return TryPlaceTower(cell, towerData);
    }

    public bool TryPlaceTower(Vector2Int cell, TowerData towerData)
    {
        if (gridManager == null || towerData == null || towerData.towerPrefab == null)
            return false;

        if (!gridManager.IsInsideGrid(cell))
            return false;

        if (gridManager.IsPathCell(cell))
            return false;

        if (occupiedCells.Contains(cell))
            return false;

        if (economy != null && !economy.SpendGold(towerData.cost))
            return false;

        Vector3 worldPos = gridManager.GridToWorld(cell);

        TowerBase tower = Instantiate(
            towerData.towerPrefab,
            worldPos,
            Quaternion.identity,
            towerRoot
        );

        tower.Initialize(towerData);

        occupiedCells.Add(cell);
        placedTowers[cell] = new PlacedTowerInfo
        {
            cell = cell,
            tower = tower,
            cost = towerData.cost
        };

        return true;
    }

    public bool CanPlaceTower(Vector2Int cell, TowerData towerData)
    {
        if (gridManager == null || towerData == null)
            return false;

        if (!gridManager.IsInsideGrid(cell))
            return false;

        if (gridManager.IsPathCell(cell))
            return false;

        if (occupiedCells.Contains(cell))
            return false;

        if (economy != null && !economy.CanSpendGold(towerData.cost))
            return false;

        return true;
    }

    public bool ClearPlacedTowersAndRefund()
    {
        return ClearPlacedTowersInternal(refund: true, ignorePhaseCheck: false);
    }

    public void ClearPlacedTowers()
    {
        ClearPlacedTowersInternal(refund: true, ignorePhaseCheck: false);
    }

    public void ResetForNewGame()
    {
        ClearPlacedTowersInternal(refund: false, ignorePhaseCheck: true);
    }

    private bool ClearPlacedTowersInternal(bool refund, bool ignorePhaseCheck)
    {
        if (!ignorePhaseCheck && gameStateManager != null && gameStateManager.CurrentPhase != GamePhase.Preparation)
        {
            return false;
        }

        int refundAmount = 0;

        foreach (PlacedTowerInfo info in placedTowers.Values)
        {
            if (info.tower != null)
            {
                Destroy(info.tower.gameObject);
            }

            if (refund)
            {
                refundAmount += Mathf.Max(0, info.cost);
            }
        }

        placedTowers.Clear();
        occupiedCells.Clear();
        SelectedTower = defaultTower;

        if (refund && economy != null && refundAmount > 0)
        {
            economy.AddGold(refundAmount);
        }

        return true;
    }
}