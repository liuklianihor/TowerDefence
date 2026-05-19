using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PathManager pathManager;
    [SerializeField] private BaseHealth baseHealth;

    [Header("Enemy Prefab")]
    [SerializeField] private EnemyMovement enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Wave Settings")]
    [SerializeField] private float spawnInterval = 1f;

    private Coroutine spawnRoutine;
    private readonly Queue<EnemySpawnEntry> queue = new();

    private void Start()
    {
        if (pathManager == null) pathManager = FindFirstObjectByType<PathManager>();
        if (baseHealth == null) baseHealth = FindFirstObjectByType<BaseHealth>();

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
        {
            return;
        }

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnInterval = interval;
        queue.Clear();

        for (int i = 0; i < wave.Count; i++)
        {
            if (wave[i] == null || wave[i].Enemy == null) continue;
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
        if (enemyPrefab == null || pathManager == null || baseHealth == null || spawnPoint == null)
        {
            return;
        }

        EnemySpawnEntry entry = queue.Dequeue();
        EnemyMovement enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        enemy.Initialize(pathManager, baseHealth, entry.Enemy);
    }
}
