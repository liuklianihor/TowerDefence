using System;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 20;

    public int CurrentHP { get; private set; }
    public int MaxHP => maxHP;
    public bool IsDestroyed { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action OnBaseDestroyed;

    private void Awake()
    {
        ResetBase();
    }

    public void ResetBase()
    {
        CurrentHP = maxHP;
        IsDestroyed = false;
        NotifyHealthChanged();
    }

    public void TakeDamage(int damage)
    {
        if (IsDestroyed || damage <= 0)
        {
            return;
        }

        CurrentHP -= damage;
        if (CurrentHP < 0) CurrentHP = 0;
        NotifyHealthChanged();

        if (CurrentHP <= 0)
        {
            IsDestroyed = true;
            OnBaseDestroyed?.Invoke();
            Debug.Log("Base destroyed. Game Over.");
        }
    }

    public void Heal(int amount)
    {
        if (IsDestroyed || amount <= 0)
        {
            return;
        }

        CurrentHP += amount;
        if (CurrentHP > maxHP) CurrentHP = maxHP;
        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }
}
