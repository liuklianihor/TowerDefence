using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PathManager pathManager;
    [SerializeField] private BaseHealth baseHealth;

    [Header("Fallback Enemy Prefab")]
    [SerializeField] private EnemyMovement enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Wave Settings")]
    [SerializeField] private float spawnInterval = 1f;

    private Coroutine spawnRoutine;
    private readonly Queue<EnemySpawnEntry> queue = new();

    private void Start()
    {
        if (pathManager == null)
            pathManager = FindFirstObjectByType<PathManager>();

        if (baseHealth == null)
            baseHealth = FindFirstObjectByType<BaseHealth>();

        if (spawnPoint == null && pathManager != null)
        {
            GameObject temp = new GameObject("SpawnPoint");
            temp.transform.position = pathManager.GetSpawnPosition();
            spawnPoint = temp.transform;
        }
    }

    public void StartWave(List<EnemySpawnEntry> wave, float interval)
    {
        if (wave == null || wave.Count == 0)
            return;

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnInterval = interval;
        queue.Clear();

        for (int i = 0; i < wave.Count; i++)
        {
            if (wave[i] == null || wave[i].Enemy == null)
                continue;

            queue.Enqueue(wave[i]);
        }

        spawnRoutine = StartCoroutine(SpawnWave());
    }

    public bool IsSpawning => spawnRoutine != null;

    private IEnumerator SpawnWave()
    {
        while (queue.Count > 0)
        {
            SpawnNextFromQueue();
            yield return new WaitForSeconds(spawnInterval);
        }

        spawnRoutine = null;
    }

    private void SpawnNextFromQueue()
    {
        if (pathManager == null || baseHealth == null || spawnPoint == null || ObjectPool.Instance == null)
            return;

        EnemySpawnEntry entry = queue.Dequeue();

        if (entry == null || entry.Enemy == null)
            return;

        GameObject prefabToSpawn = null;

        if (entry.Enemy.enemyPrefab != null)
            prefabToSpawn = entry.Enemy.enemyPrefab;
        else if (enemyPrefab != null)
            prefabToSpawn = enemyPrefab.gameObject;

        if (prefabToSpawn == null)
        {
            Debug.LogError($"EnemySpawner: no prefab assigned for enemy '{entry.Enemy.name}'.");
            return;
        }

        GameObject enemyObject = ObjectPool.Instance.Get(
            prefabToSpawn,
            spawnPoint.position,
            Quaternion.identity
        );

        if (enemyObject == null)
            return;

        EnemyMovement enemy = enemyObject.GetComponent<EnemyMovement>();

        if (enemy == null)
            enemy = enemyObject.GetComponentInChildren<EnemyMovement>();

        if (enemy != null)
            enemy.Initialize(pathManager, baseHealth, entry.Enemy);
        else
            Debug.LogError($"EnemySpawner: prefab '{prefabToSpawn.name}' does not contain EnemyMovement.");
    }

    public void SetBaseHealth(BaseHealth targetBase)
    {
        baseHealth = targetBase;
    }
}