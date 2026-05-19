using System.Collections.Generic;
using UnityEngine;

public class WaveComposer : MonoBehaviour
{
    [Header("Enemy Catalog")]
    [SerializeField] private List<EnemyDefinition> enemyCatalog = new();

    [Header("Difficulty Growth")]
    [SerializeField] private int cheapEnemyBiasUntilRound = 3;
    [SerializeField] private int mixedEnemyBiasUntilRound = 6;

    public List<EnemySpawnEntry> ComposeAutoWave(int roundIndex, int attackBudget, int maxEnemies = 50)
    {
        var wave = new List<EnemySpawnEntry>();
        if (enemyCatalog == null || enemyCatalog.Count == 0 || attackBudget <= 0)
        {
            return wave;
        }

        int remainingBudget = attackBudget;
        int enemyCount = 0;

        while (remainingBudget > 0 && enemyCount < maxEnemies)
        {
            EnemyDefinition picked = PickEnemy(roundIndex, remainingBudget, enemyCount);
            if (picked == null)
            {
                break;
            }

            wave.Add(new EnemySpawnEntry(picked, 1));
            remainingBudget -= picked.cost;
            enemyCount++;
        }

        return wave;
    }

    private EnemyDefinition PickEnemy(int roundIndex, int remainingBudget, int waveIndex)
    {
        EnemyDefinition fallback = null;

        for (int i = 0; i < enemyCatalog.Count; i++)
        {
            EnemyDefinition candidate = enemyCatalog[i];
            if (candidate == null || candidate.cost > remainingBudget)
            {
                continue;
            }

            fallback ??= candidate;

            if (roundIndex <= cheapEnemyBiasUntilRound)
            {
                if (candidate.enemyKind == EnemyKind.Goblin)
                    return candidate;
            }
            else if (roundIndex <= mixedEnemyBiasUntilRound)
            {
                if (candidate.enemyKind == EnemyKind.Orc && waveIndex % 3 == 1)
                    return candidate;

                if (candidate.enemyKind == EnemyKind.Goblin)
                    fallback = candidate;
            }
            else
            {
                if (candidate.enemyKind == EnemyKind.Ghost && waveIndex % 4 == 0)
                    return candidate;

                if (candidate.enemyKind == EnemyKind.Orc && waveIndex % 2 == 0)
                    return candidate;
            }
        }

        return fallback;
    }
}
