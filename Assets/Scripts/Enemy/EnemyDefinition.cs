using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defense/Enemy Definition", fileName = "EnemyDefinition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Identity")]
    public EnemyKind enemyKind = EnemyKind.Goblin;

    [Header("Stats")]
    [Min(1)] public int maxHP = 10;
    [Min(0.1f)] public float moveSpeed = 2f;
    [Min(1)] public int baseDamage = 1;
    [Min(0)] public int cost = 10;
    [Min(0)] public int goldReward = 5;

    [Header("Special Rules")]
    public bool ignoresFreezer = false;

    [Header("Visuals")]
    [SerializeField] public Sprite sprite;
    public GameObject enemyPrefab;

    [Header("Audio")]
    public AudioClip spawnClip;
}

[Serializable]
public class EnemySpawnEntry
{
    public EnemyDefinition Enemy;
    [Min(1)] public int Count = 1;

    public EnemySpawnEntry() { }

    public EnemySpawnEntry(EnemyDefinition enemy, int count = 1)
    {
        Enemy = enemy;
        Count = Mathf.Max(1, count);
    }
}