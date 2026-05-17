using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 10;

    private int currentHP;
    private bool isDead;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsDead => isDead;

    public event Action<int, int> OnHealthChanged;
    public event Action<EnemyHealth> OnDied;

    private void Awake()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        currentHP = maxHP;
        isDead = false;
        NotifyHealthChanged();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHP -= damage;
        if (currentHP < 0)
            currentHP = 0;

        NotifyHealthChanged();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        OnDied?.Invoke(this);
        Destroy(gameObject);
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
}