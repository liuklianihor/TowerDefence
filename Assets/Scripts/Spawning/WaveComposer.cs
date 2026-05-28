using System.Collections.Generic;
using UnityEngine;

public class WaveComposer : MonoBehaviour
{
    [Header("Enemy Catalog")]
    [SerializeField] private List<EnemyDefinition> enemyCatalog = new();

    public List<EnemySpawnEntry> ComposeAutoWave(int roundIndex, int attackBudget, int maxEnemies = 50)
    {
        var wave = new List<EnemySpawnEntry>();

        if (enemyCatalog == null || enemyCatalog.Count == 0 || attackBudget <= 0 || maxEnemies <= 0)
            return wave;

        var orderedCatalog = new List<EnemyDefinition>();
        for (int i = 0; i < enemyCatalog.Count; i++)
        {
            EnemyDefinition enemy = enemyCatalog[i];
            if (enemy != null && enemy.cost > 0)
                orderedCatalog.Add(enemy);
        }

        orderedCatalog.Sort((a, b) => a.enemyKind.CompareTo(b.enemyKind));

        if (orderedCatalog.Count == 0)
            return wave;

        int remainingBudget = attackBudget;
        int enemyIndex = 0;

        while (wave.Count < maxEnemies)
        {
            EnemyDefinition enemy = orderedCatalog[enemyIndex];
            if (enemy.cost > remainingBudget)
                break;

            wave.Add(new EnemySpawnEntry(enemy, 1));
            remainingBudget -= enemy.cost;

            enemyIndex++;
            if (enemyIndex >= orderedCatalog.Count)
                enemyIndex = 0;
        }

        return wave;
    }
}