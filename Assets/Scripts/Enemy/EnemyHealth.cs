using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 10;

    public int CurrentHP { get; private set; }
    public int MaxHP => maxHP;
    public bool IsDead { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action<EnemyHealth> OnDied;

    private void Awake()
    {
        ResetHealth();
    }

    public void Initialize(int newMaxHP)
    {
        maxHP = Mathf.Max(1, newMaxHP);
        ResetHealth();
    }

    public void ResetHealth()
    {
        CurrentHP = maxHP;
        IsDead = false;
        NotifyHealthChanged();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0)
        {
            return;
        }

        CurrentHP -= damage;
        if (CurrentHP < 0) CurrentHP = 0;
        NotifyHealthChanged();

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsDead) return;

        IsDead = true;
        OnDied?.Invoke(this);
        Destroy(gameObject);
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }
}
