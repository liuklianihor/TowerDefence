using System.Collections.Generic;
using UnityEngine;

public class WaveComposer : MonoBehaviour
{
    [Header("Enemy Catalog")]
    [SerializeField] private List<EnemyDefinition> enemyCatalog = new();

    public List<EnemySpawnEntry> ComposeAutoWave(int roundIndex, int attackBudget, int maxEnemies = 50)
    {
        var wave = new List<EnemySpawnEntry>();

        if (enemyCatalog == null || enemyCatalog.Count == 0 || attackBudget <= 0)
            return wave;

        List<EnemyDefinition> orderedCatalog = new List<EnemyDefinition>();

        for (int i = 0; i < enemyCatalog.Count; i++)
        {
            if (enemyCatalog[i] != null)
                orderedCatalog.Add(enemyCatalog[i]);
        }

        orderedCatalog.Sort((a, b) => a.enemyKind.CompareTo(b.enemyKind));

        if (orderedCatalog.Count == 0)
            return wave;

        int remainingBudget = attackBudget;
        int enemyCount = 0;
        int cycleIndex = 0;

        while (remainingBudget > 0 && enemyCount < maxEnemies)
        {
            bool foundAffordableEnemy = false;
            EnemyDefinition picked = null;

            for (int attempts = 0; attempts < orderedCatalog.Count; attempts++)
            {
                EnemyDefinition candidate = orderedCatalog[cycleIndex % orderedCatalog.Count];
                cycleIndex++;

                if (candidate == null || candidate.cost > remainingBudget)
                    continue;

                picked = candidate;
                foundAffordableEnemy = true;
                break;
            }

            if (!foundAffordableEnemy || picked == null)
                break;

            wave.Add(new EnemySpawnEntry(picked, 1));

            remainingBudget -= picked.cost;
            enemyCount++;
        }

        return wave;
    }
}