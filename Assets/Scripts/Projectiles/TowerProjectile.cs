using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    [SerializeField] private float hitDistance = 0.1f;

    private Transform target;
    private int damage;
    private float speed;
    private float splashRadius;
    private ProjectileImpactMode impactMode;
    private float slowMultiplier;
    private float slowDuration;

    public void Initialize(
        Transform targetTransform,
        int projectileDamage,
        float projectileSpeed,
        float impactRadius,
        ProjectileImpactMode mode,
        float slowMultiplierValue,
        float slowDurationValue)
    {
        target = targetTransform;
        damage = projectileDamage;
        speed = projectileSpeed;
        splashRadius = impactRadius;
        impactMode = mode;
        slowMultiplier = slowMultiplierValue;
        slowDuration = slowDurationValue;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= hitDistance)
        {
            Impact();
        }
    }

    private void Impact()
    {
        if (impactMode == ProjectileImpactMode.Slow)
        {
            var targetEnemy = target.GetComponent<EnemyTarget>();
            if (targetEnemy != null)
            {
                targetEnemy.TryApplySlow(slowMultiplier, slowDuration);
                targetEnemy.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        if (splashRadius > 0f)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius);
            foreach (Collider2D hit in hits)
            {
                EnemyTarget enemy = hit.GetComponent<EnemyTarget>();
                if (enemy == null) continue;
                enemy.TakeDamage(damage);
            }
        }
        else
        {
            EnemyTarget enemy = target.GetComponent<EnemyTarget>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}
