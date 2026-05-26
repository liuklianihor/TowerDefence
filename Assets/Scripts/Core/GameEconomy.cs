using System;
using UnityEngine;

public class GameEconomy : MonoBehaviour
{
    [Header("Starting Values")]
    [SerializeField] private int startingGold = 300;
    [SerializeField] private int startingAttackBudget = 300;

    [Header("Round Growth")]
    [SerializeField] private int attackBudgetIncreasePerRound = 30;

    public int Gold { get; private set; }
    public int AttackBudget { get; private set; }

    public event Action<int, int> OnGoldChanged;
    public event Action<int, int> OnAttackBudgetChanged;

    private void Awake()
    {
        ResetEconomy();
    }

    public void ResetEconomy()
    {
        Gold = startingGold;
        SetAttackBudgetForRound(1);
        NotifyGoldChanged();
    }

    public void PrepareForNextRound(int roundIndex)
    {
        SetAttackBudgetForRound(roundIndex);
    }

    public void BurnAttackBudget()
    {
        AttackBudget = 0;
        NotifyAttackBudgetChanged();
    }

    public bool CanSpendGold(int amount) => amount >= 0 && Gold >= amount;

    public bool SpendGold(int amount)
    {
        if (!CanSpendGold(amount))
        {
            return false;
        }

        Gold -= amount;
        NotifyGoldChanged();
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        Gold += amount;
        NotifyGoldChanged();
    }

    public bool CanSpendAttackBudget(int amount) => amount >= 0 && AttackBudget >= amount;

    public bool SpendAttackBudget(int amount)
    {
        if (!CanSpendAttackBudget(amount))
        {
            return false;
        }

        AttackBudget -= amount;
        NotifyAttackBudgetChanged();
        return true;
    }

    public void AddAttackBudget(int amount)
    {
        if (amount <= 0) return;

        AttackBudget += amount;
        NotifyAttackBudgetChanged();
    }

    private void SetAttackBudgetForRound(int roundIndex)
    {
        roundIndex = Mathf.Max(1, roundIndex);
        AttackBudget = startingAttackBudget + (roundIndex - 1) * attackBudgetIncreasePerRound;
        NotifyAttackBudgetChanged();
    }

    private void NotifyGoldChanged() => OnGoldChanged?.Invoke(Gold, startingGold);

    private void NotifyAttackBudgetChanged() => OnAttackBudgetChanged?.Invoke(AttackBudget, startingAttackBudget);
}