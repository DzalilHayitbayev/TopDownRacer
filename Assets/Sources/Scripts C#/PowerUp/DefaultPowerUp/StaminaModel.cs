using System;

public class StaminaModel
{
    public float MaxStamina { get; private set; }
    public float CurrentStamina { get; private set; }
    public float RegenRate { get; private set; }
    public float RegenDelay { get; private set; }

    public event Action<float, float> OnStaminaChanged;

    private float _timeSinceLastUse;

    public StaminaModel(float maxStamina, float regenRate, float regenDelay)
    {
        MaxStamina = maxStamina;
        CurrentStamina = maxStamina;
        RegenRate = regenRate;
        RegenDelay = regenDelay;
        _timeSinceLastUse = regenDelay; // Сразу готова к регену при старте
    }

    /// <summary>
    /// Вызывается в Update тике
    /// </summary>
    public void Tick(float deltaTime)
    {
        _timeSinceLastUse += deltaTime;

        if (_timeSinceLastUse >= RegenDelay && CurrentStamina < MaxStamina)
        {
            CurrentStamina = MathF.Min(MaxStamina, CurrentStamina + RegenRate * deltaTime);
            OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        }
    }

    public bool HasStamina(float amount)
    {
        return CurrentStamina >= amount;
    }

    public bool Consume(float amount)
    {
        if (CurrentStamina <= 0) return false;

        CurrentStamina = MathF.Max(0f, CurrentStamina - amount);
        _timeSinceLastUse = 0f;
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        return CurrentStamina > 0;
    }

    public void Reset()
    {
        CurrentStamina = MaxStamina;
        _timeSinceLastUse = RegenDelay;
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }
}