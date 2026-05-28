using System;
using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Hit / Death Feedback")]
    [SerializeField] private float damageFlashDuration = 0.12f;
    [SerializeField] private float deathFadeDuration = 0.5f;
    [SerializeField] private Color damageTint = new Color(1f, 0.35f, 0.35f, 1f);

    private PathManager pathManager;
    private BaseHealth baseHealth;
    private EnemyHealth enemyHealth;
    private EnemyDefinition enemyDefinition;

    private int currentWaypointIndex;
    private bool reachedEnd;
    private bool isDespawning;

    private float slowMultiplier = 1f;
    private float slowTimer = 0f;

    private Color originalColor;
    private Sprite originalSprite;

    private int lastHp;
    private Coroutine flashRoutine;
    private Coroutine fadeRoutine;

    public float ProgressNormalized { get; private set; }
    public int BaseDamage => enemyDefinition != null ? enemyDefinition.baseDamage : 1;
    public bool IgnoresFreezer => enemyDefinition != null && enemyDefinition.ignoresFreezer;

    public event Action<EnemyMovement> OnDespawned;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalSprite = spriteRenderer.sprite;
        }

        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDied += HandleDied;
            enemyHealth.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnEnable()
    {
        ResetReusableState();
    }

    private void OnDisable()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        ResetVisualState();
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDied -= HandleDied;
            enemyHealth.OnHealthChanged -= HandleHealthChanged;
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

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        if (enemyDefinition != null)
        {
            moveSpeed = enemyDefinition.moveSpeed;

            if (spriteRenderer != null)
            {
                if (enemyDefinition.sprite != null)
                    spriteRenderer.sprite = enemyDefinition.sprite;
                else if (originalSprite != null)
                    spriteRenderer.sprite = originalSprite;
            }

            if (enemyHealth != null)
                enemyHealth.Initialize(enemyDefinition);
        }
        else if (enemyHealth != null)
        {
            enemyHealth.ResetHealth();
        }

        if (pathManager != null && pathManager.WaypointCount > 0)
        {
            transform.position = pathManager.GetWaypointPosition(0);
            currentWaypointIndex = 1;
        }

        lastHp = enemyHealth != null ? enemyHealth.CurrentHP : 0;
        UpdateProgress();
        ResetVisualState();
        UpdateFacingFromNextWaypoint();
    }

    private void Update()
    {
        if (reachedEnd || pathManager == null)
            return;

        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
                slowMultiplier = 1f;
        }

        if (currentWaypointIndex >= pathManager.WaypointCount)
        {
            ReachBase();
            return;
        }

        float currentSpeed = moveSpeed * slowMultiplier;
        Vector3 previousPosition = transform.position;
        Vector3 targetPosition = pathManager.GetWaypointPosition(currentWaypointIndex);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

        UpdateFacingFromMovement(previousPosition, targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            currentWaypointIndex++;
            UpdateProgress();
            UpdateFacingFromNextWaypoint();
        }
    }

    public bool ApplySlow(float multiplier, float duration)
    {
        if (IgnoresFreezer || isDespawning)
            return false;

        slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowTimer = Mathf.Max(0f, duration);
        return true;
    }

    private void HandleHealthChanged(int currentHp, int maxHp)
    {
        if (isDespawning)
        {
            lastHp = currentHp;
            return;
        }

        if (currentHp < lastHp && currentHp > 0)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(FlashDamageRoutine());
        }

        lastHp = currentHp;
    }

    private void HandleDied()
    {
        if (isDespawning)
            return;

        RewardDefender();
        BeginFadeOut(true);
    }

    private void ReachBase()
    {
        if (reachedEnd || isDespawning)
            return;

        reachedEnd = true;

        if (baseHealth != null)
            baseHealth.TakeDamage(BaseDamage);

        BeginFadeOut(false);
    }

    private void BeginFadeOut(bool playDeathEffect)
    {
        if (isDespawning)
            return;

        isDespawning = true;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (playDeathEffect && CombatFeedbackManager.Instance != null)
            CombatFeedbackManager.Instance.PlayEnemyDeath(transform.position);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOutThenDespawn());
    }

    private IEnumerator FlashDamageRoutine()
    {
        if (spriteRenderer == null)
            yield break;

        Color startColor = spriteRenderer.color;
        spriteRenderer.color = damageTint;

        yield return new WaitForSecondsRealtime(damageFlashDuration);

        if (spriteRenderer != null && !isDespawning)
            spriteRenderer.color = startColor;

        flashRoutine = null;
    }

    private IEnumerator FadeOutThenDespawn()
    {
        if (spriteRenderer == null)
        {
            DespawnToPool();
            yield break;
        }

        float elapsed = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsed < deathFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / deathFadeDuration);

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = c;

            yield return null;
        }

        DespawnToPool();
        fadeRoutine = null;
    }

    public void DespawnToPool()
    {
        if (isDespawning == false)
            isDespawning = true;

        OnDespawned?.Invoke(this);

        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void ResetReusableState()
    {
        reachedEnd = false;
        isDespawning = false;
        slowMultiplier = 1f;
        slowTimer = 0f;
        currentWaypointIndex = 0;

        lastHp = enemyHealth != null ? enemyHealth.CurrentHP : 0;

        ResetVisualState();
    }

    private void ResetVisualState()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = originalColor;
        Color c = spriteRenderer.color;
        c.a = 1f;
        spriteRenderer.color = c;

        if (originalSprite != null && (enemyDefinition == null || enemyDefinition.sprite == null))
            spriteRenderer.sprite = originalSprite;
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

    private void UpdateFacingFromNextWaypoint()
    {
        if (spriteRenderer == null || pathManager == null)
            return;

        if (currentWaypointIndex >= pathManager.WaypointCount)
            return;

        Vector3 current = transform.position;
        Vector3 next = pathManager.GetWaypointPosition(currentWaypointIndex);
        ApplyFacingFromDelta(next.x - current.x);
    }

    private void UpdateFacingFromMovement(Vector3 from, Vector3 to)
    {
        if (spriteRenderer == null)
            return;

        ApplyFacingFromDelta(to.x - from.x);
    }

    private void ApplyFacingFromDelta(float deltaX)
    {
        if (Mathf.Abs(deltaX) < 0.0001f)
            return;

        bool faceRight = deltaX > 0f;
        spriteRenderer.flipX = !faceRight;
    }

    private void RewardDefender()
    {
        if (enemyDefinition == null)
            return;

        if (GameStateManager.Instance != null && GameStateManager.Instance.Economy != null)
            GameStateManager.Instance.Economy.AddGold(enemyDefinition.goldReward);
    }
}