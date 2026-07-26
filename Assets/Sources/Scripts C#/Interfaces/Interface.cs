using UnityEngine;

public interface IDamageable
{
    bool IsAlive { get; }
    void TakeDamage(int damage, GameObject attacker = null);
    void Heal(int amount);
}

public interface ISpeedAttacker
{
    float CurrentSpeed { get; }
    bool CanDealSpeedDamage { get; }
}