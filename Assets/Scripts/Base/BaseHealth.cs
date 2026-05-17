using System;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 20;

    private int currentHP;
    private bool isDestroyed;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsDestroyed => isDestroyed;

    public event Action<int, int> OnHealthChanged;
    public event Action OnBaseDestroyed;

    private void Awake()
    {
        currentHP = maxHP;
        isDestroyed = false;
        NotifyHealthChanged();
    }

    public void TakeDamage(int damage)
    {
        if (isDestroyed)
            return;

        currentHP -= damage;
        if (currentHP < 0)
            currentHP = 0;

        NotifyHealthChanged();

        if (currentHP <= 0)
        {
            isDestroyed = true;
            OnBaseDestroyed?.Invoke();
            Debug.Log("Base destroyed. Game Over.");
        }
    }

    public void Heal(int amount)
    {
        if (isDestroyed)
            return;

        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;

        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
}