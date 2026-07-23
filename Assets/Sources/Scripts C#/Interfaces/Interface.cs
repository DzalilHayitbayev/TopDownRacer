public interface IDamageable
{
    void TakeDamage(int damage);
    bool IsAlive { get; }
}

public interface ISpeedAttacker
{
    float CurrentSpeed { get; }
    bool CanDealSpeedDamage { get; }
}