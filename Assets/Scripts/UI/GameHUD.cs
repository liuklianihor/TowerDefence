using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private GameEconomy economy;
    [SerializeField] private TowerPlacementController towerPlacementController;
    [SerializeField] private BaseHealth baseHealth;

    [Header("Value Texts")]
    [SerializeField] private TMP_Text goldValueText;
    [SerializeField] private TMP_Text attackBudgetValueText;
    [SerializeField] private TMP_Text baseHpValueText;
    [SerializeField] private TMP_Text roundValueText;
    [SerializeField] private TMP_Text phaseValueText;

    [Header("Label Images")]
    [SerializeField] private Image goldLabelImage;
    [SerializeField] private Image attackBudgetLabelImage;
    [SerializeField] private Image hpLabelImage;
    [SerializeField] private Image roundLabelImage;
    [SerializeField] private Image phaseLabelImage;

    [Header("Game Over Images")]
    [SerializeField] private Image victoryImage;
    [SerializeField] private Image defeatImage;

    [Header("Buttons")]
    [SerializeField] private Button startBattleButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button clearSelectionButton;

    [Header("Optional Panels")]
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        ResolveReferences();
        WireButtons();
        HideGameOverImages();
    }

    private void OnEnable()
    {
        Subscribe();
        TryBindBase();
        RefreshAll();
    }

    private void Start()
    {
        TryBindBase();
        RefreshAll();
    }

    private void OnDisable()
    {
        Unsubscribe();
        UnbindBase();
    }

    private void Update()
    {
        if (baseHealth == null)
            TryBindBase();

        RefreshBaseHealth();
    }

    private void ResolveReferences()
    {
        if (gameStateManager == null) gameStateManager = FindFirstObjectByType<GameStateManager>();
        if (economy == null) economy = FindFirstObjectByType<GameEconomy>();
        if (towerPlacementController == null) towerPlacementController = FindFirstObjectByType<TowerPlacementController>();
        if (baseHealth == null) baseHealth = FindFirstObjectByType<BaseHealth>();
    }

    private void WireButtons()
    {
        if (startBattleButton != null)
        {
            startBattleButton.onClick.RemoveListener(OnStartBattlePressed);
            startBattleButton.onClick.AddListener(OnStartBattlePressed);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartPressed);
            restartButton.onClick.AddListener(OnRestartPressed);
        }

        if (clearSelectionButton != null)
        {
            clearSelectionButton.onClick.RemoveListener(OnClearSelectionPressed);
            clearSelectionButton.onClick.AddListener(OnClearSelectionPressed);
        }
    }

    private void Subscribe()
    {
        if (economy != null)
        {
            economy.OnGoldChanged += OnGoldChanged;
            economy.OnAttackBudgetChanged += OnAttackBudgetChanged;
        }

        if (gameStateManager != null)
        {
            gameStateManager.OnRoundChanged += OnRoundChanged;
            gameStateManager.OnPhaseChanged += OnPhaseChanged;
            gameStateManager.OnGameEnded += OnGameEnded;
        }

        if (baseHealth != null)
            baseHealth.OnHealthChanged += OnBaseHealthChanged;
    }

    private void Unsubscribe()
    {
        if (economy != null)
        {
            economy.OnGoldChanged -= OnGoldChanged;
            economy.OnAttackBudgetChanged -= OnAttackBudgetChanged;
        }

        if (gameStateManager != null)
        {
            gameStateManager.OnRoundChanged -= OnRoundChanged;
            gameStateManager.OnPhaseChanged -= OnPhaseChanged;
            gameStateManager.OnGameEnded -= OnGameEnded;
        }

        if (baseHealth != null)
            baseHealth.OnHealthChanged -= OnBaseHealthChanged;
    }

    public void BindBase(BaseHealth newBaseHealth)
    {
        if (baseHealth == newBaseHealth)
        {
            RefreshBaseHealth();
            return;
        }

        UnbindBase();
        baseHealth = newBaseHealth;

        if (baseHealth != null)
            baseHealth.OnHealthChanged += OnBaseHealthChanged;

        RefreshBaseHealth();
    }

    private void UnbindBase()
    {
        if (baseHealth != null)
            baseHealth.OnHealthChanged -= OnBaseHealthChanged;
    }

    private void TryBindBase()
    {
        if (baseHealth != null) return;

        baseHealth = FindFirstObjectByType<BaseHealth>();
        if (baseHealth != null)
            baseHealth.OnHealthChanged += OnBaseHealthChanged;
    }

    private void RefreshAll()
    {
        if (economy != null)
        {
            OnGoldChanged(economy.Gold, 0);
            OnAttackBudgetChanged(economy.AttackBudget, 0);
        }

        if (gameStateManager != null)
        {
            OnRoundChanged(gameStateManager.CurrentRound, gameStateManager.TotalRounds);
            OnPhaseChanged(gameStateManager.CurrentPhase);
        }

        RefreshBaseHealth();
    }

    private void OnGoldChanged(int gold, int startingGold)
    {
        if (goldValueText != null)
            goldValueText.text = gold.ToString();
    }

    private void OnAttackBudgetChanged(int budget, int startingBudget)
    {
        if (attackBudgetValueText != null)
            attackBudgetValueText.text = budget.ToString();
    }

    private void OnRoundChanged(int currentRound, int totalRounds)
    {
        if (roundValueText != null)
            roundValueText.text = $"{currentRound}/{totalRounds}";
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        if (phaseValueText != null)
            phaseValueText.text = phase.ToString();

        if (startBattleButton != null)
            startBattleButton.interactable = phase == GamePhase.Preparation;

        bool showGameOver = phase == GamePhase.GameOver;

        if (restartButton != null)
            restartButton.gameObject.SetActive(showGameOver);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(showGameOver);

        if (!showGameOver)
            HideGameOverImages();
    }

    private void OnGameEnded(bool defenderWon)
    {
        if (victoryImage != null)
            victoryImage.gameObject.SetActive(defenderWon);

        if (defeatImage != null)
            defeatImage.gameObject.SetActive(!defenderWon);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);
    }

    private void HideGameOverImages()
    {
        if (victoryImage != null)
            victoryImage.gameObject.SetActive(false);

        if (defeatImage != null)
            defeatImage.gameObject.SetActive(false);
    }

    private void OnBaseHealthChanged(int currentHp, int maxHp)
    {
        UpdateBaseHpText(currentHp, maxHp);

        if (currentHp <= 0 && gameStateManager != null && gameStateManager.CurrentPhase != GamePhase.GameOver)
            gameStateManager.GameOver(false);
    }

    private void RefreshBaseHealth()
    {
        if (baseHealth == null || baseHpValueText == null)
            return;

        UpdateBaseHpText(baseHealth.CurrentHP, baseHealth.MaxHP);
    }

    private void UpdateBaseHpText(int currentHp, int maxHp)
    {
        if (baseHpValueText == null)
            return;

        baseHpValueText.text = $"{Mathf.Max(0, currentHp)}/{Mathf.Max(1, maxHp)}";
    }

    public void OnStartBattlePressed()
    {
        gameStateManager?.StartBattle();
    }

    public void OnRestartPressed()
    {
        Time.timeScale = 1f;
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }

    public void OnClearSelectionPressed()
    {
        if (towerPlacementController != null)
            towerPlacementController.ClearPlacedTowersAndRefund();

        RefreshAll();
    }
}