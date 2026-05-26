using System;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private PathManager pathManager;
    private BaseHealth baseHealth;
    private EnemyHealth enemyHealth;
    private EnemyDefinition enemyDefinition;

    private int currentWaypointIndex;
    private bool reachedEnd;
    private bool isDespawning;
    private float slowMultiplier = 1f;
    private float slowTimer = 0f;

    public float ProgressNormalized { get; private set; }
    public int BaseDamage => enemyDefinition != null ? enemyDefinition.baseDamage : 1;
    public bool IgnoresFreezer => enemyDefinition != null && enemyDefinition.ignoresFreezer;

    public event Action<EnemyMovement> OnDespawned;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.OnDied += HandleDied;
        }
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDied -= HandleDied;
        }
    }

    public void Initialize(PathManager path, BaseHealth targetBase, EnemyDefinition definition = null)
    {
        pathManager = path;
        baseHealth = targetBase;
        enemyDefinition = definition;

        currentWaypointIndex = 0;
        reachedEnd = false;
        isDespawning = false;
        slowMultiplier = 1f;
        slowTimer = 0f;

        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();

        if (enemyDefinition != null)
        {
            moveSpeed = enemyDefinition.moveSpeed;

            if (spriteRenderer != null && enemyDefinition.sprite != null)
            {
                spriteRenderer.sprite = enemyDefinition.sprite;
            }

            if (enemyHealth != null)
            {
                enemyHealth.Initialize(enemyDefinition);
            }
        }
        else if (enemyHealth != null)
        {
            enemyHealth.ResetHealth();
        }

        if (pathManager != null && pathManager.WaypointCount > 0)
        {
            transform.position = pathManager.GetWaypointPosition(0);
            currentWaypointIndex = 1;
            UpdateProgress();
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (reachedEnd || pathManager == null) return;

        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f) slowMultiplier = 1f;
        }

        if (currentWaypointIndex >= pathManager.WaypointCount)
        {
            ReachBase();
            return;
        }

        float currentSpeed = moveSpeed * slowMultiplier;
        Vector3 targetPosition = pathManager.GetWaypointPosition(currentWaypointIndex);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            currentWaypointIndex++;
            UpdateProgress();
        }
    }

    public bool ApplySlow(float multiplier, float duration)
    {
        if (IgnoresFreezer) return false;

        slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowTimer = Mathf.Max(0f, duration);
        return true;
    }

    private void HandleDied()
    {
        RewardDefender();
        DespawnToPool();
    }

    private void ReachBase()
    {
        if (reachedEnd) return;

        reachedEnd = true;

        if (baseHealth != null)
        {
            baseHealth.TakeDamage(BaseDamage);
        }

        DespawnToPool();
    }

    public void DespawnToPool()
    {
        if (isDespawning) return;

        isDespawning = true;
        OnDespawned?.Invoke(this);

        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.Return(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateProgress()
    {
        if (pathManager == null || pathManager.WaypointCount <= 1)
        {
            ProgressNormalized = 0f;
            return;
        }

        ProgressNormalized = Mathf.Clamp01((float)currentWaypointIndex / (pathManager.WaypointCount - 1));
    }

    private void RewardDefender()
    {
        if (enemyDefinition == null) return;

        if (GameStateManager.Instance != null && GameStateManager.Instance.Economy != null)
        {
            GameStateManager.Instance.Economy.AddGold(enemyDefinition.goldReward);
        }
    }
}