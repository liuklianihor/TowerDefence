using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyMovement enemyPrefab;
    [SerializeField] private PathManager pathManager;
    [SerializeField] private BaseHealth baseHealth;

    [Header("Wave Settings")]
    [SerializeField] private int enemyCount = 10;
    [SerializeField] private float spawnInterval = 1f;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;

    private Coroutine spawnRoutine;

    private void Start()
    {
        if (spawnPoint == null && pathManager != null)
        {
            GameObject temp = new GameObject("SpawnPoint");
            temp.transform.position = pathManager.GetSpawnPosition();
            spawnPoint = temp.transform;
        }

        spawnRoutine = StartCoroutine(SpawnWave());
    }

    public void StartWave(int count, float interval)
    {
        enemyCount = count;
        spawnInterval = interval;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnRoutine = StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || pathManager == null || baseHealth == null)
            return;

        EnemyMovement enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        enemy.Initialize(pathManager, baseHealth);
    }
}