using System;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHP = 20;

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
            return;

        CurrentHP = Mathf.Max(0, CurrentHP - damage);
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
            return;

        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }
}