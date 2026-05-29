using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BaseHealth))]
public class BaseVisualFeedback : MonoBehaviour
{
    [SerializeField] private SpriteRenderer mainSprite;
    [SerializeField] private Sprite destroyedSprite;
    [SerializeField] private Color hitTint = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private float flashDuration = 0.15f;

    private BaseHealth baseHealth;
    private Color originalColor;
    private Sprite originalSprite;
    private int lastHp;
    private bool destroyedShown;
    private Coroutine flashRoutine;
    private bool isHitFlashActive;

    private void Awake()
    {
        baseHealth = GetComponent<BaseHealth>();

        if (mainSprite == null)
            mainSprite = GetComponentInChildren<SpriteRenderer>(true);

        if (mainSprite != null)
        {
            originalColor = mainSprite.color;
            originalSprite = mainSprite.sprite;
        }
    }

    private void OnEnable()
    {
        if (baseHealth != null)
        {
            baseHealth.OnHealthChanged += HandleHealthChanged;
            baseHealth.OnBaseDestroyed += HandleDestroyed;
            lastHp = baseHealth.CurrentHP;
        }

        destroyedShown = false;
        isHitFlashActive = false;
        ResetVisualState();
    }

    private void OnDisable()
    {
        if (baseHealth != null)
        {
            baseHealth.OnHealthChanged -= HandleHealthChanged;
            baseHealth.OnBaseDestroyed -= HandleDestroyed;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        isHitFlashActive = false;
    }

    private void HandleHealthChanged(int currentHp, int maxHp)
    {
        if (mainSprite == null)
        {
            lastHp = currentHp;
            return;
        }

        if (currentHp >= maxHp && !baseHealth.IsDestroyed)
        {
            destroyedShown = false;
            ResetVisualState();
            lastHp = currentHp;
            return;
        }

        if (currentHp < lastHp && currentHp > 0)
        {
            if (CombatFeedbackManager.Instance != null)
                CombatFeedbackManager.Instance.PlayBaseHit(transform.position);

            // Захист тільки від повторного flash-сигналу
            if (!isHitFlashActive)
                flashRoutine = StartCoroutine(FlashRoutine());
        }

        lastHp = currentHp;
    }

    private void HandleDestroyed()
    {
        if (destroyedShown)
            return;

        destroyedShown = true;

        if (CombatFeedbackManager.Instance != null)
            CombatFeedbackManager.Instance.PlayBaseDestroyed(transform.position);

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        isHitFlashActive = false;

        if (mainSprite != null && destroyedSprite != null)
            mainSprite.sprite = destroyedSprite;
    }

    private IEnumerator FlashRoutine()
    {
        if (mainSprite == null)
            yield break;

        isHitFlashActive = true;
        mainSprite.color = hitTint;

        yield return new WaitForSecondsRealtime(flashDuration);

        if (mainSprite != null)
            mainSprite.color = originalColor;

        isHitFlashActive = false;
        flashRoutine = null;
    }

    private void ResetVisualState()
    {
        if (mainSprite == null)
            return;

        mainSprite.color = originalColor;

        if (!baseHealth.IsDestroyed && originalSprite != null)
            mainSprite.sprite = originalSprite;
    }
}