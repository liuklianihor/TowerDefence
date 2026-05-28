using System.Collections;
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

        if (GameStateManager.Instance != null &&
            GameStateManager.Instance.CurrentPhase != GamePhase.Battle)
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
        if (target == null)
            return;

        if (CombatFeedbackManager.Instance != null)
            CombatFeedbackManager.Instance.PlayTowerShot(transform.position);

        float projectileSpeed = Mathf.Max(0.01f, towerData.range * 2f);
        float distance = Vector3.Distance(transform.position, target.transform.position);
        float travelTime = distance / projectileSpeed;

        ProjectileImpactMode impactMode = towerData.attackMode == TowerAttackMode.Slow
            ? ProjectileImpactMode.Slow
            : ProjectileImpactMode.Damage;

        if (towerData.projectilePrefab != null)
        {
            SpawnProjectile(target, projectileSpeed, impactMode);
            return;
        }

        StartCoroutine(DelayedFallbackImpact(
            target,
            towerData.damage,
            travelTime,
            impactMode,
            towerData.slowMultiplier,
            towerData.slowDuration,
            towerData.splashRadius
        ));
    }

    private void SpawnProjectile(EnemyTarget target, float projectileSpeed, ProjectileImpactMode impactMode)
    {
        GameObject projectileObject = null;

        if (ObjectPool.Instance != null)
            projectileObject = ObjectPool.Instance.Get(towerData.projectilePrefab, transform.position, Quaternion.identity);
        else
            projectileObject = Instantiate(towerData.projectilePrefab, transform.position, Quaternion.identity);

        if (projectileObject == null)
            return;

        TowerProjectile projectile = projectileObject.GetComponent<TowerProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(
                target.transform,
                towerData.damage,
                projectileSpeed,
                towerData.splashRadius,
                impactMode,
                towerData.slowMultiplier,
                towerData.slowDuration
            );
        }
    }

    private IEnumerator DelayedFallbackImpact(
        EnemyTarget target,
        int damage,
        float travelTime,
        ProjectileImpactMode impactMode,
        float slowMultiplier,
        float slowDuration,
        float splashRadius)
    {
        yield return new WaitForSecondsRealtime(travelTime);

        if (target == null || !target.gameObject.activeInHierarchy)
            yield break;

        if (CombatFeedbackManager.Instance != null)
            CombatFeedbackManager.Instance.PlayProjectileHit(target.transform.position);

        if (impactMode == ProjectileImpactMode.Slow)
        {
            target.TryApplySlow(slowMultiplier, slowDuration);
            target.TakeDamage(damage);
            yield break;
        }

        if (splashRadius > 0f)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, splashRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null)
                    continue;

                EnemyTarget enemy = hits[i].GetComponent<EnemyTarget>();
                if (enemy == null)
                    continue;

                enemy.TakeDamage(damage);
            }

            yield break;
        }

        target.TakeDamage(damage);
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