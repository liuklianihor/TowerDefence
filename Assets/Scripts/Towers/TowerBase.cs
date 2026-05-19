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
        if (towerData == null)
            return;

        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentPhase != GamePhase.Battle)
            return;

        if (Time.time < nextAttackTime)
            return;

        EnemyTarget target = AcquireTarget();
        if (target == null)
            return;

        Attack(target);
        nextAttackTime = Time.time + towerData.cooldown;
    }

    private void Attack(EnemyTarget target)
    {
        if (towerData.attackMode == TowerAttackMode.Slow)
        {
            if (target.TryApplySlow(towerData.slowMultiplier, towerData.slowDuration))
            {
                if (towerData.projectilePrefab != null)
                    SpawnProjectile(target, towerData.damage, true);
            }
            else
            {
                if (towerData.projectilePrefab != null)
                    SpawnProjectile(target, towerData.damage, true);
            }

            return;
        }

        if (towerData.attackMode == TowerAttackMode.Splash)
        {
            if (towerData.projectilePrefab != null)
                SpawnProjectile(target, towerData.damage, false, towerData.splashRadius);
            else
                ExplodeAt(target.transform.position, towerData.damage, towerData.splashRadius);

            return;
        }

        if (towerData.projectilePrefab != null)
            SpawnProjectile(target, towerData.damage, false);
        else
            target.TakeDamage(towerData.damage);
    }

    private void SpawnProjectile(EnemyTarget target, int damage, bool applySlow, float splashRadius = 0f)
    {
        GameObject projectileObject = ObjectPool.Instance.Get(
            towerData.projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        if (projectileObject == null)
            return;

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
            if (enemy == null)
                continue;

            enemy.TakeDamage(damage);
        }
    }

    private EnemyTarget AcquireTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, towerData.range);
        if (hits == null || hits.Length == 0)
            return null;

        EnemyTarget bestTarget = null;

        float bestProgress = float.MinValue;
        float bestDistance = float.MaxValue;
        float farthestDistance = float.MinValue;
        float strongestHealth = float.MinValue;
        float weakestHealth = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyTarget enemy = hits[i].GetComponent<EnemyTarget>();
            if (enemy == null || enemy.IsDead)
                continue;

            float progress = enemy.ProgressNormalized;
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            float hpRatio = enemy.HealthRatio;

            switch (towerData.targetMode)
            {
                case TowerTargetMode.Progress:
                    if (progress > bestProgress)
                    {
                        bestProgress = progress;
                        bestTarget = enemy;
                    }
                    break;

                case TowerTargetMode.Closest:
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestTarget = enemy;
                    }
                    break;

                case TowerTargetMode.Farthest:
                    if (distance > farthestDistance)
                    {
                        farthestDistance = distance;
                        bestTarget = enemy;
                    }
                    break;

                case TowerTargetMode.Strongest:
                    if (hpRatio > strongestHealth)
                    {
                        strongestHealth = hpRatio;
                        bestTarget = enemy;
                    }
                    break;

                case TowerTargetMode.Weakest:
                    if (hpRatio < weakestHealth)
                    {
                        weakestHealth = hpRatio;
                        bestTarget = enemy;
                    }
                    break;
            }
        }

        return bestTarget;
    }
}