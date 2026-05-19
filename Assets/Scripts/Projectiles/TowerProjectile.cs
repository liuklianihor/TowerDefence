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

    private bool despawning;

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
        despawning = false;
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Despawn();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= hitDistance)
            Impact();
    }

    private void Impact()
    {
        if (despawning)
            return;

        if (impactMode == ProjectileImpactMode.Slow)
        {
            EnemyTarget targetEnemy = target.GetComponent<EnemyTarget>();
            if (targetEnemy != null)
            {
                targetEnemy.TryApplySlow(slowMultiplier, slowDuration);
                targetEnemy.TakeDamage(damage);
            }

            Despawn();
            return;
        }

        if (splashRadius > 0f)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius);
            foreach (Collider2D hit in hits)
            {
                EnemyTarget enemy = hit.GetComponent<EnemyTarget>();
                if (enemy == null)
                    continue;

                enemy.TakeDamage(damage);
            }
        }
        else
        {
            EnemyTarget enemy = target.GetComponent<EnemyTarget>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }

        Despawn();
    }

    private void Despawn()
    {
        if (despawning)
            return;

        despawning = true;

        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(gameObject);
        else
            gameObject.SetActive(false);
    }
}