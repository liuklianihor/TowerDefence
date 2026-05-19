using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Runtime / Default Stats")]
    [SerializeField] private EnemyDefinition enemyDefinition;
    [SerializeField] private int maxHP = 10;

    public int CurrentHP { get; private set; }
    public int MaxHP => maxHP;
    public bool IsDead { get; private set; }

    public EnemyDefinition Definition => enemyDefinition;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private void Awake()
    {
        ApplyDefinition(enemyDefinition);
        ResetHealth();
    }

    public void Initialize(EnemyDefinition definition)
    {
        ApplyDefinition(definition);
        ResetHealth();
    }

    public void Initialize(int newMaxHP)
    {
        enemyDefinition = null;
        maxHP = Mathf.Max(1, newMaxHP);
        ResetHealth();
    }

    public void ApplyDefinition(EnemyDefinition definition)
    {
        enemyDefinition = definition;

        if (enemyDefinition != null)
            maxHP = Mathf.Max(1, enemyDefinition.maxHP);
    }

    public void ResetHealth()
    {
        CurrentHP = maxHP;
        IsDead = false;
        NotifyHealthChanged();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        CurrentHP -= Mathf.Max(0, damage);

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            IsDead = true;

            NotifyHealthChanged();
            OnDied?.Invoke();
            return;
        }

        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }
}