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

    [Header("UI Text")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text attackBudgetText;
    [SerializeField] private TMP_Text baseHpText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text phaseText;

    [Header("Game Over Texts")]
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private TMP_Text defeatText;

    [Header("Buttons")]
    [SerializeField] private Button startBattleButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button clearSelectionButton;

    [Header("Optional Panels")]
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        ResolveReferences();
        AutoBindButtons();
        WireButtons();
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

    private void AutoBindButtons()
    {
        if (restartButton == null)
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = buttons[i];
                if (btn == null) continue;

                TMP_Text label = btn.GetComponentInChildren<TMP_Text>(true);
                if (label == null) continue;

                string text = label.text.Trim().ToLowerInvariant();
                if (text.Contains("restart") || text.Contains("рестарт") || text.Contains("replay"))
                {
                    restartButton = btn;
                    break;
                }
            }
        }
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
            restartButton.interactable = true;
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
        if (goldText != null)
            goldText.text = $"Gold: {gold}";
    }

    private void OnAttackBudgetChanged(int budget, int startingBudget)
    {
        if (attackBudgetText != null)
            attackBudgetText.text = $"Attack: {budget}";
    }

    private void OnRoundChanged(int currentRound, int totalRounds)
    {
        if (roundText != null)
            roundText.text = $"Round: {currentRound}/{totalRounds}";
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        if (phaseText != null)
            phaseText.text = $"Phase: {phase}";

        if (startBattleButton != null)
            startBattleButton.interactable = phase == GamePhase.Preparation;

        bool showGameOver = phase == GamePhase.GameOver;

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(showGameOver);
            restartButton.interactable = showGameOver;
            restartButton.transform.SetAsLastSibling();
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(showGameOver);
    }

    private void OnGameEnded(bool defenderWon)
    {
        if (victoryText != null)
            victoryText.gameObject.SetActive(defenderWon);

        if (defeatText != null)
            defeatText.gameObject.SetActive(!defenderWon);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
            restartButton.interactable = true;
            restartButton.transform.SetAsLastSibling();
        }
    }

    private void OnBaseHealthChanged(int currentHp, int maxHp)
    {
        UpdateBaseHpText(currentHp, maxHp);

        if (currentHp <= 0 &&
            gameStateManager != null &&
            gameStateManager.CurrentPhase != GamePhase.GameOver)
        {
            gameStateManager.GameOver(false);
        }
    }

    private void RefreshBaseHealth()
    {
        if (baseHealth == null || baseHpText == null) return;
        UpdateBaseHpText(baseHealth.CurrentHP, baseHealth.MaxHP);
    }

    private void UpdateBaseHpText(int currentHp, int maxHp)
    {
        if (baseHpText == null) return;
        baseHpText.text = $"HP: {Mathf.Max(0, currentHp)}/{Mathf.Max(1, maxHp)}";
    }

    public void OnStartBattlePressed()
    {
        if (gameStateManager != null)
            gameStateManager.StartBattle();
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