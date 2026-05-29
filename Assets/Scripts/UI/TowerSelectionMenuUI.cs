using UnityEngine;
using UnityEngine.UI;

public class TowerSelectionMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TowerPlacementController towerPlacementController;

    [Header("Tower Data")]
    [SerializeField] private TowerData archerTower;
    [SerializeField] private TowerData cannonTower;
    [SerializeField] private TowerData freezerTower;
    [SerializeField] private TowerData mageTower;

    [Header("Buttons")]
    [SerializeField] private Button archerButton;
    [SerializeField] private Button cannonButton;
    [SerializeField] private Button freezerButton;
    [SerializeField] private Button mageButton;
    [SerializeField] private Button clearSelectionButton;

    private void Awake()
    {
        if (towerPlacementController == null)
            towerPlacementController = FindFirstObjectByType<TowerPlacementController>();
    }

    private void OnEnable()
    {
        WireButtons();
    }

    private void OnDisable()
    {
        UnwireButtons();
    }

    private void WireButtons()
    {
        if (archerButton != null)
        {
            archerButton.onClick.RemoveListener(SelectArcher);
            archerButton.onClick.AddListener(SelectArcher);
        }

        if (cannonButton != null)
        {
            cannonButton.onClick.RemoveListener(SelectCannon);
            cannonButton.onClick.AddListener(SelectCannon);
        }

        if (freezerButton != null)
        {
            freezerButton.onClick.RemoveListener(SelectFreezer);
            freezerButton.onClick.AddListener(SelectFreezer);
        }

        if (mageButton != null)
        {
            mageButton.onClick.RemoveListener(SelectMage);
            mageButton.onClick.AddListener(SelectMage);
        }

        if (clearSelectionButton != null)
        {
            clearSelectionButton.onClick.RemoveListener(ClearSelection);
            clearSelectionButton.onClick.AddListener(ClearSelection);
        }
    }

    private void UnwireButtons()
    {
        if (archerButton != null) archerButton.onClick.RemoveListener(SelectArcher);
        if (cannonButton != null) cannonButton.onClick.RemoveListener(SelectCannon);
        if (freezerButton != null) freezerButton.onClick.RemoveListener(SelectFreezer);
        if (mageButton != null) mageButton.onClick.RemoveListener(SelectMage);
        if (clearSelectionButton != null) clearSelectionButton.onClick.RemoveListener(ClearSelection);
    }

    private void SelectArcher() => SelectTower(archerTower);
    private void SelectCannon() => SelectTower(cannonTower);
    private void SelectFreezer() => SelectTower(freezerTower);
    private void SelectMage() => SelectTower(mageTower);

    private void SelectTower(TowerData towerData)
    {
        if (towerPlacementController != null && towerData != null)
            towerPlacementController.SelectTower(towerData);
    }

    private void ClearSelection()
    {
        towerPlacementController?.ClearSelection();
    }
}