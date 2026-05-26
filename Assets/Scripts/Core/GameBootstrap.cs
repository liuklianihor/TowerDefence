using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PathManager pathManager;
    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private GameHUD gameHUD;

    [Header("Prefabs")]
    [SerializeField] private GameObject basePrefab;

    private GameObject spawnedBase;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        if (pathManager == null)
            pathManager = FindFirstObjectByType<PathManager>();

        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (gameHUD == null)
            gameHUD = FindFirstObjectByType<GameHUD>();
    }

    private void Start()
    {
        if (gridManager != null)
            gridManager.GenerateGrid();

        SpawnBase();

        if (gameHUD != null)
            gameHUD.BindBase(baseHealth);
    }

    private void SpawnBase()
    {
        if (pathManager == null)
            return;

        Vector3 basePosition = pathManager.GetBasePosition();

        if (basePrefab == null)
        {
            Debug.LogError("Base prefab is not assigned in GameBootstrap.");
            return;
        }

        spawnedBase = Instantiate(basePrefab, basePosition, Quaternion.identity);
        baseHealth = spawnedBase.GetComponent<BaseHealth>();

        if (enemySpawner != null)
            enemySpawner.SetBaseHealth(baseHealth);

        if (baseHealth == null)
            baseHealth = spawnedBase.GetComponentInChildren<BaseHealth>();

        if (baseHealth == null)
            Debug.LogError("Spawned base prefab does not contain BaseHealth component.");
    }
}