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

        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (IsDestroyed || amount <= 0)
            return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);

        OnHealthChanged?.Invoke(CurrentHP, maxHP);

        if (CurrentHP <= 0)
        {
            IsDestroyed = true;

            OnBaseDestroyed?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (IsDestroyed || amount <= 0)
            return;

        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);

        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }
}