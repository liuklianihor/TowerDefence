using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PathManager pathManager;
    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] private EnemySpawner enemySpawner;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        if (pathManager == null)
            pathManager = FindFirstObjectByType<PathManager>();

        if (baseHealth == null)
            baseHealth = FindFirstObjectByType<BaseHealth>();

        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
    }

    private void Start()
    {
        if (gridManager != null)
            gridManager.GenerateGrid();
    }
}