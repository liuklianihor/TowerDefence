using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Flow")]
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private int currentRound = 1;

    [Header("References")]
    [SerializeField] private GameEconomy economy;
    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private WaveComposer waveComposer;

    [Header("Wave")]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int maxEnemiesPerWave = 50;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Menu;
    public int CurrentRound => currentRound;
    public int TotalRounds => totalRounds;

    public event Action<GamePhase> OnPhaseChanged;
    public event Action<int, int> OnRoundChanged;
    public event Action<bool> OnGameEnded; // true = defender wins

    private readonly List<EnemySpawnEntry> currentWave = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (economy == null) economy = FindFirstObjectByType<GameEconomy>();
        if (baseHealth == null) baseHealth = FindFirstObjectByType<BaseHealth>();
        if (enemySpawner == null) enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (waveComposer == null) waveComposer = FindFirstObjectByType<WaveComposer>();

        if (baseHealth != null)
        {
            baseHealth.OnBaseDestroyed += HandleBaseDestroyed;
        }

        StartNewGame();
    }

    private void OnDestroy()
    {
        if (baseHealth != null)
        {
            baseHealth.OnBaseDestroyed -= HandleBaseDestroyed;
        }
    }

    public void StartNewGame()
    {
        currentRound = 1;
        economy?.ResetEconomy();
        SetPhase(GamePhase.Preparation);
        OnRoundChanged?.Invoke(currentRound, totalRounds);
    }

    public void StartPreparation()
    {
        if (CurrentPhase == GamePhase.GameOver) return;
        SetPhase(GamePhase.Preparation);
    }

    public void StartBattle()
    {
        if (CurrentPhase == GamePhase.GameOver) return;
        if (enemySpawner == null || waveComposer == null || economy == null) return;

        currentWave.Clear();
        currentWave.AddRange(waveComposer.ComposeAutoWave(currentRound, economy.AttackBudget, maxEnemiesPerWave));

        if (currentWave.Count == 0)
        {
            Debug.LogWarning("WaveComposer returned an empty wave. Battle not started.");
            return;
        }

        economy.SpendAttackBudget(GetWaveCost(currentWave));
        SetPhase(GamePhase.Battle);
        enemySpawner.StartWave(currentWave, spawnInterval);
    }

    public void EndBattle(bool defenderWonRound)
    {
        if (CurrentPhase == GamePhase.GameOver) return;

        SetPhase(GamePhase.RoundEnd);

        if (defenderWonRound)
        {
            economy?.PrepareForNextRound(currentRound);
        }

        if (baseHealth != null && baseHealth.IsDestroyed)
        {
            GameOver(false);
            return;
        }

        if (currentRound >= totalRounds)
        {
            GameOver(true);
            return;
        }

        currentRound++;
        OnRoundChanged?.Invoke(currentRound, totalRounds);
        SetPhase(GamePhase.Preparation);
    }

    public void GameOver(bool defenderWon)
    {
        SetPhase(GamePhase.GameOver);
        OnGameEnded?.Invoke(defenderWon);
    }

    public void NotifyBattleFinished(bool defenderWonRound)
    {
        EndBattle(defenderWonRound);
    }

    private int GetWaveCost(List<EnemySpawnEntry> wave)
    {
        int total = 0;
        for (int i = 0; i < wave.Count; i++)
        {
            if (wave[i]?.Enemy == null) continue;
            total += wave[i].Enemy.cost;
        }
        return total;
    }

    private void HandleBaseDestroyed()
    {
        GameOver(false);
    }

    private void SetPhase(GamePhase phase)
    {
        if (CurrentPhase == phase) return;
        CurrentPhase = phase;
        OnPhaseChanged?.Invoke(CurrentPhase);
    }
}
