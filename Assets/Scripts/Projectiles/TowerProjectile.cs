using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    [SerializeField] private float hitDistance = 0.1f;
    [SerializeField] private float rotationOffsetDegrees = 0f;

    private Transform target;
    private int damage;
    private float speed;
    private float splashRadius;
    private ProjectileImpactMode impactMode;
    private float slowMultiplier;
    private float slowDuration;
    private bool despawning;

    private void OnEnable()
    {
        despawning = false;
    }

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
        speed = Mathf.Max(0.01f, projectileSpeed);
        splashRadius = impactRadius;
        impactMode = mode;
        slowMultiplier = slowMultiplierValue;
        slowDuration = slowDurationValue;
        despawning = false;

        UpdateRotationToTarget();
    }

    private void Update()
    {
        if (despawning)
            return;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Despawn();
            return;
        }

        UpdateRotationToTarget();
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= hitDistance)
            Impact();
    }

    private void UpdateRotationToTarget()
    {
        if (target == null)
            return;

        Vector2 direction = target.position - transform.position;
        if (direction.sqrMagnitude < 0.000001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffsetDegrees;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Impact()
    {
        if (despawning)
            return;

        despawning = true;

        if (CombatFeedbackManager.Instance != null)
            CombatFeedbackManager.Instance.PlayProjectileHit(transform.position);

        if (impactMode == ProjectileImpactMode.Slow)
        {
            EnemyTarget targetEnemy = target != null ? target.GetComponent<EnemyTarget>() : null;
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
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null) continue;

                EnemyTarget enemy = hits[i].GetComponent<EnemyTarget>();
                if (enemy == null) continue;

                enemy.TakeDamage(damage);
            }

            Despawn();
            return;
        }

        EnemyTarget directTarget = target != null ? target.GetComponent<EnemyTarget>() : null;
        if (directTarget != null)
            directTarget.TakeDamage(damage);

        Despawn();
    }

    private void Despawn()
    {
        if (despawning == false)
            despawning = true;

        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(gameObject);
        else
            gameObject.SetActive(false);
    }
}