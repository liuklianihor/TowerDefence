using System.Collections.Generic;
using UnityEngine;

public class TowerBase : MonoBehaviour
{
    [SerializeField] private TowerData towerData;

    private float nextAttackTime;

    public TowerData Data => towerData;

    public void Initialize(TowerData data)
    {
        towerData = data;
        nextAttackTime = 0f;
    }

    private void Update()
    {
        if (towerData == null) return;
        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentPhase != GamePhase.Battle)
        {
            return;
        }

        if (Time.time < nextAttackTime) return;

        EnemyTarget target = AcquireTarget();
        if (target == null)
        {
            return;
        }

        Attack(target);
        nextAttackTime = Time.time + towerData.cooldown;
    }

    private void Attack(EnemyTarget target)
    {
        if (towerData.attackMode == TowerAttackMode.Slow)
        {
            if (!target.TryApplySlow(towerData.slowMultiplier, towerData.slowDuration))
            {
                // Ghosts ignore slow; no damage fallthrough here.
            }

            if (towerData.projectilePrefab != null)
            {
                SpawnProjectile(target, towerData.damage, true);
            }
            return;
        }

        if (towerData.attackMode == TowerAttackMode.Splash)
        {
            if (towerData.projectilePrefab != null)
            {
                SpawnProjectile(target, towerData.damage, false, towerData.splashRadius);
            }
            else
            {
                ExplodeAt(target.transform.position, towerData.damage, towerData.splashRadius);
            }

            return;
        }

        if (towerData.projectilePrefab != null)
        {
            SpawnProjectile(target, towerData.damage, false);
        }
        else
        {
            target.TakeDamage(towerData.damage);
        }
    }

    private void SpawnProjectile(EnemyTarget target, int damage, bool applySlow, float splashRadius = 0f)
    {
        GameObject projectileObject = Instantiate(towerData.projectilePrefab, transform.position, Quaternion.identity);
        TowerProjectile projectile = projectileObject.GetComponent<TowerProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(
                target.transform,
                damage,
                towerData.projectileSpeed,
                splashRadius,
                applySlow ? ProjectileImpactMode.Slow : ProjectileImpactMode.Damage,
                towerData.slowMultiplier,
                towerData.slowDuration
            );
        }
    }

    private void ExplodeAt(Vector3 position, int damage, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius <= 0f ? towerData.range : radius);
        foreach (Collider2D hit in hits)
        {
            EnemyTarget enemy = hit.GetComponent<EnemyTarget>();
            if (enemy == null) continue;
            enemy.TakeDamage(damage);
        }
    }

    private EnemyTarget AcquireTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, towerData.range);
        if (hits == null || hits.Length == 0)
        {
            return null;
        }

        EnemyTarget bestTarget = null;
        float bestScore = towerData.targetMode == TowerTargetMode.Closest ? float.MaxValue : float.MinValue;
        float bestHealthRatio = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyTarget enemy = hits[i].GetComponent<EnemyTarget>();
            if (enemy == null || enemy.IsDead) continue;

            float progress = enemy.ProgressNormalized;
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            float hpRatio = enemy.HealthRatio;

            switch (towerData.targetMode)
            {
                case TowerTargetMode.Progress:
                    if (progress > bestScore)
                    {
                        bestScore = progress;
                        bestTarget = enemy;
                    }
                    break;

                case TowerTargetMode.Closest:
                    if (distance < bestScore)
                    {
                        bestScore = distance;
                        bestTarget = enemy;
                    }
                    break;

                case TowerTargetMode.Farthest:
                    if (distance > bestScore)
                    {
                        bestScore = distance;
                        bestTarget = enemy;
                    }
                    break;

                case TowerTargetMode.Strongest:
                    if (hpRatio < bestHealthRatio)
                    {
                        bestHealthRatio = hpRatio;
                        bestTarget = enemy;
                    }
                    break;

                case TowerTargetMode.Weakest:
                    if (hpRatio > bestHealthRatio)
                    {
                        bestHealthRatio = hpRatio;
                        bestTarget = enemy;
                    }
                    break;
            }
        }

        return bestTarget;
    }
}

public class EnemyTarget : MonoBehaviour
{
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyHealth health;

    public bool IsDead => health != null && health.IsDead;
    public float ProgressNormalized => movement != null ? movement.ProgressNormalized : 0f;
    public float HealthRatio => health != null ? (float)health.CurrentHP / Mathf.Max(1, health.MaxHP) : 1f;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<EnemyMovement>();
        if (health == null) health = GetComponent<EnemyHealth>();
    }

    public void TakeDamage(int amount)
    {
        if (health != null)
        {
            health.TakeDamage(amount);
        }
    }

    public bool TryApplySlow(float multiplier, float duration)
    {
        if (movement == null) return false;
        return movement.ApplySlow(multiplier, duration);
    }
}
