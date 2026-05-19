using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerSelectionUI : MonoBehaviour
{
    [Serializable]
    public class TowerOption
    {
        public string label;
        public TowerData towerData;
        public Button button;
    }

    [Header("References")]
    [SerializeField] private TowerPlacementController placementController;

    [Header("UI")]
    [SerializeField] private TMP_Text selectedTowerText;
    [SerializeField] private Button defaultTowerButton;
    [SerializeField] private List<TowerOption> options = new List<TowerOption>();

    private void Awake()
    {
        WireButtons();
        RefreshSelectionText();
    }

    private void OnEnable()
    {
        RefreshSelectionText();
    }

    private void OnDestroy()
    {
        UnwireButtons();
    }

    private void WireButtons()
    {
        if (defaultTowerButton != null)
        {
            defaultTowerButton.onClick.RemoveListener(SelectDefaultTower);
            defaultTowerButton.onClick.AddListener(SelectDefaultTower);
        }

        for (int i = 0; i < options.Count; i++)
        {
            TowerOption option = options[i];
            if (option == null || option.button == null)
                continue;

            TowerData towerData = option.towerData;
            option.button.onClick.RemoveAllListeners();
            option.button.onClick.AddListener(() => SelectTower(towerData));
        }
    }

    private void UnwireButtons()
    {
        if (defaultTowerButton != null)
            defaultTowerButton.onClick.RemoveListener(SelectDefaultTower);
    }

    public void SelectTower(TowerData towerData)
    {
        if (placementController != null)
            placementController.SelectTower(towerData);

        RefreshSelectionText();
    }

    public void SelectDefaultTower()
    {
        if (placementController != null)
            placementController.ClearSelection();

        RefreshSelectionText();
    }

    private void RefreshSelectionText()
    {
        if (selectedTowerText == null)
            return;

        TowerData active = placementController != null ? placementController.ActiveTowerData : null;

        if (active == null)
        {
            selectedTowerText.text = "Selected: none";
            return;
        }

        selectedTowerText.text = $"Selected: {active.towerName}  |  Cost: {active.cost}";
    }
}