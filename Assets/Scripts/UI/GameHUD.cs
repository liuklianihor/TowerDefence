using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private GameEconomy economy;

    [Tooltip("BaseHealth")]
    [SerializeField] private Component baseHealthComponent;

    [Header("UI Text")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text attackBudgetText;
    [SerializeField] private TMP_Text baseHpText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text gameOverText;

    [Header("Buttons")]
    [SerializeField] private Button startBattleButton;
    [SerializeField] private Button restartButton;

    [Header("Optional Panels")]
    [SerializeField] private GameObject gameOverPanel;

    private object baseHealthInstance;
    private PropertyInfo baseCurrentHpProp;
    private PropertyInfo baseMaxHpProp;
    private PropertyInfo baseIsDestroyedProp;

    private void Awake()
    {
        ResolveReferences();
        CacheBaseHealthReflection();

        WireButtons();
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshAll();
    }

    private void Start()
    {
        RefreshAll();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        RefreshBaseHealth();
    }

    private void ResolveReferences()
    {
        if (gameStateManager == null)
            gameStateManager = FindFirstObjectByType<GameStateManager>();

        if (economy == null)
            economy = FindFirstObjectByType<GameEconomy>();

        if (baseHealthComponent == null)
            baseHealthComponent = FindFirstObjectByType<MonoBehaviour>(FindObjectsInactive.Include);

        baseHealthInstance = baseHealthComponent;
    }

    private void CacheBaseHealthReflection()
    {
        if (baseHealthInstance == null)
            return;

        Type t = baseHealthInstance.GetType();

        baseCurrentHpProp = GetFirstProperty(t,
            "CurrentHP", "CurrentHealth", "HP", "Health");

        baseMaxHpProp = GetFirstProperty(t,
            "MaxHP", "MaxHealth", "TotalHP", "HealthMax", "HPMax");

        baseIsDestroyedProp = GetFirstProperty(t,
            "IsDestroyed", "Destroyed", "IsDead");
    }

    private PropertyInfo GetFirstProperty(Type type, params string[] names)
    {
        foreach (string name in names)
        {
            PropertyInfo prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (prop != null)
                return prop;
        }

        return null;
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

        if (restartButton != null)
            restartButton.gameObject.SetActive(phase == GamePhase.GameOver);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(phase == GamePhase.GameOver);
    }

    private void OnGameEnded(bool defenderWon)
    {
        if (gameOverText != null)
            gameOverText.text = defenderWon ? "VICTORY" : "DEFEAT";

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private void RefreshBaseHealth()
    {
        if (baseHealthInstance == null || baseHpText == null)
            return;

        int currentHp = ReadIntProperty(baseCurrentHpProp, baseHealthInstance, -1);
        int maxHp = ReadIntProperty(baseMaxHpProp, baseHealthInstance, -1);
        bool destroyed = ReadBoolProperty(baseIsDestroyedProp, baseHealthInstance, false);

        if (destroyed)
        {
            if (maxHp > 0)
                baseHpText.text = $"Base HP: 0/{maxHp}";
            else
                baseHpText.text = "Base HP: 0";
            return;
        }

        if (currentHp >= 0 && maxHp > 0)
            baseHpText.text = $"Base HP: {currentHp}/{maxHp}";
        else if (currentHp >= 0)
            baseHpText.text = $"Base HP: {currentHp}";
    }

    private int ReadIntProperty(PropertyInfo prop, object target, int fallback)
    {
        if (prop == null || target == null)
            return fallback;

        object value = prop.GetValue(target);
        if (value is int intValue)
            return intValue;

        return fallback;
    }

    private bool ReadBoolProperty(PropertyInfo prop, object target, bool fallback)
    {
        if (prop == null || target == null)
            return fallback;

        object value = prop.GetValue(target);
        if (value is bool boolValue)
            return boolValue;

        return fallback;
    }

    public void OnStartBattlePressed()
    {
        if (gameStateManager != null)
            gameStateManager.StartBattle();
    }

    public void OnRestartPressed()
    {
        if (gameStateManager != null)
            gameStateManager.StartNewGame();
    }
}