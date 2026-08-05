public class SawPowerUpModel
{
    private readonly StaminaModel _stamina;
    private readonly float _drainRate;
    private readonly float _minStaminaToActivate;

    public bool IsActive { get; private set; }

    public SawPowerUpModel(StaminaModel stamina, float drainRate, float minStaminaToActivate = 5f)
    {
        _stamina = stamina;
        _drainRate = drainRate;
        _minStaminaToActivate = minStaminaToActivate;
    }

    public void TryActivate()
    {
        if (_stamina.HasStamina(_minStaminaToActivate))
        {
            IsActive = true;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Вызывать каждый кадр. Возвращает true, если PowerUp продолжает работать.
    /// </summary>
    public bool Tick(float deltaTime)
    {
        if (!IsActive) return false;

        bool hasStamina = _stamina.Consume(_drainRate * deltaTime);
        if (!hasStamina)
        {
            IsActive = false;
        }

        return IsActive;
    }
}