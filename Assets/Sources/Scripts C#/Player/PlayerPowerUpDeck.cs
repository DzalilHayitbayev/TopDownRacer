using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPowerUpDeck
{
    private const string DeckKey = "SelectedPowerUps";

    // Храним выбранные на ТЕКУЩУЮ гонку пауэрапы
    private readonly List<PowerUpType> selectedPowerUps = new List<PowerUpType>();

    // Кэшируем стоимости закупленных абилок для возврата средств при отмене выбора
    private readonly Dictionary<PowerUpType, int> powerUpPrices = new Dictionary<PowerUpType, int>();

    public event Action OnDeckUpdated;

    public PlayerPowerUpDeck()
    {
        Load();
    }

    /// <summary>
    /// В новой логике «куплен» равен «выбран в текущую колоду»
    /// </summary>
    public bool IsPowerUpPurchased(PowerUpType type) => selectedPowerUps.Contains(type);

    public bool IsPowerUpSelected(PowerUpType type) => selectedPowerUps.Contains(type);

    public List<PowerUpType> GetSelectedPowerUps() => new List<PowerUpType>(selectedPowerUps);

    /// <summary>
    /// Покупка / Выбор PowerUp на ОДНУ гонку.
    /// Если уже выбран — происходит отмена с возвратом денег.
    /// </summary>
    public bool TogglePurchasePowerUp(PowerUpData data, PlayerWallet wallet)
    {
        if (data == null || wallet == null) return false;

        PowerUpType type = data.type;

        // Если PowerUp УЖЕ куплен на эту гонку — отменяем покупку и возвращаем деньги
        if (selectedPowerUps.Contains(type))
        {
            selectedPowerUps.Remove(type);

            // Возвращаем средства обратно
            if (powerUpPrices.TryGetValue(type, out int price))
            {
                wallet.AddMoney(price);
                powerUpPrices.Remove(type);
            }

            Save();
            OnDeckUpdated?.Invoke();
            return true;
        }

        // Если PowerUp ЕЩЁ НЕ куплен — проверяем лимит и списываем средства
        if (selectedPowerUps.Count >= 3)
        {
            Debug.LogWarning("[PowerUpDeck] Нельзя выбрать больше 3 PowerUp'ов!");
            return false;
        }

        if (wallet.TrySpendMoney(data.price))
        {
            selectedPowerUps.Add(type);
            powerUpPrices[type] = data.price;

            Save();
            OnDeckUpdated?.Invoke();
            return true;
        }

        Debug.LogWarning("[PowerUpDeck] Недостаточно средств для покупки!");
        return false;
    }

    /// <summary>
    /// Вызывать этот метод ПОСЛЕ ЗАВЕРШЕНИЯ или СТАРТА гонки, чтобы сжечь одноразовые пауэрапы
    /// </summary>
    public void ConsumeSelectedPowerUps()
    {
        selectedPowerUps.Clear();
        powerUpPrices.Clear();
        PlayerPrefs.DeleteKey(DeckKey);
        PlayerPrefs.Save();

        OnDeckUpdated?.Invoke();
    }

    private void Save()
    {
        PlayerPrefs.SetString(DeckKey, string.Join(",", selectedPowerUps));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        selectedPowerUps.Clear();
        powerUpPrices.Clear();

        if (PlayerPrefs.HasKey(DeckKey))
        {
            string savedData = PlayerPrefs.GetString(DeckKey);
            if (string.IsNullOrEmpty(savedData)) return;

            foreach (var str in savedData.Split(','))
            {
                if (Enum.TryParse(str, out PowerUpType type))
                {
                    if (!selectedPowerUps.Contains(type) && selectedPowerUps.Count < 3)
                    {
                        selectedPowerUps.Add(type);
                    }
                }
            }
        }
    }
}