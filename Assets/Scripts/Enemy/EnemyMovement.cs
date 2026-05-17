using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    private PathManager pathManager;
    private BaseHealth baseHealth;
    private EnemyHealth enemyHealth;

    private int currentWaypointIndex;
    private bool reachedEnd;

    public void Initialize(PathManager path, BaseHealth targetBase)
    {
        pathManager = path;
        baseHealth = targetBase;

        currentWaypointIndex = 0;
        reachedEnd = false;

        enemyHealth = GetComponent<EnemyHealth>();

        if (pathManager != null && pathManager.WaypointCount > 0)
        {
            transform.position = pathManager.GetWaypointPosition(0);
            currentWaypointIndex = 1;
        }
    }

    private void Update()
    {
        if (reachedEnd || pathManager == null)
            return;

        if (currentWaypointIndex >= pathManager.WaypointCount)
        {
            ReachBase();
            return;
        }

        Vector3 targetPosition = pathManager.GetWaypointPosition(currentWaypointIndex);
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            currentWaypointIndex++;
        }
    }

    private void ReachBase()
    {
        if (reachedEnd)
            return;

        reachedEnd = true;

        if (baseHealth != null)
        {
            baseHealth.TakeDamage(1);
        }

        Destroy(gameObject);
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }
}