using System.Collections.Generic;
using UnityEngine;

public class EnemyTarget : MonoBehaviour
{
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyHealth health;

    public bool IsDead => health != null && health.IsDead;
    public float ProgressNormalized => movement != null ? movement.ProgressNormalized : 0f;
    public float HealthRatio => health != null ? (float)health.CurrentHP / Mathf.Max(1, health.MaxHP) : 1f;

    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<EnemyMovement>();

        if (health == null)
            health = GetComponent<EnemyHealth>();
    }

    public void TakeDamage(int amount)
    {
        if (health != null)
            health.TakeDamage(amount);
    }

    public bool TryApplySlow(float multiplier, float duration)
    {
        if (movement == null)
            return false;

        return movement.ApplySlow(multiplier, duration);
    }
}