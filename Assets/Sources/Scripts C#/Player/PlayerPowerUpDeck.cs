using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPowerUpDeck
{
    private const string DeckKey = "SelectedPowerUps";

    private readonly List<PowerUpType> selectedPowerUps = new List<PowerUpType>();
    private readonly Dictionary<PowerUpType, int> powerUpPrices = new Dictionary<PowerUpType, int>();

    public event Action OnDeckUpdated;

    public PlayerPowerUpDeck()
    {
        Load();
    }

    public bool IsPowerUpPurchased(PowerUpType type) => selectedPowerUps.Contains(type);
    public bool IsPowerUpSelected(PowerUpType type) => selectedPowerUps.Contains(type);
    public List<PowerUpType> GetSelectedPowerUps() => new List<PowerUpType>(selectedPowerUps);

    public bool TogglePurchasePowerUp(PowerUpData data, PlayerWallet wallet)
    {
        if (data == null || wallet == null) return false;

        PowerUpType type = data.type;

        if (selectedPowerUps.Contains(type))
        {
            selectedPowerUps.Remove(type);

            if (powerUpPrices.TryGetValue(type, out int price))
            {
                wallet.AddMoney(price);
                powerUpPrices.Remove(type);
            }

            Save();
            OnDeckUpdated?.Invoke();
            return true;
        }

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
    /// Окончательное списание купленных зарядов.
    /// Вызывать ТОЛЬКО при возврате в Главное Меню / Гараж.
    /// </summary>
    public void ClearDeckAndSave()
    {
        selectedPowerUps.Clear();
        powerUpPrices.Clear();

        PlayerPrefs.DeleteKey(DeckKey);
        PlayerPrefs.Save();

        OnDeckUpdated?.Invoke();
        Debug.Log("[PowerUpDeck] Заряды сгорели после завершения заезда и выхода в меню.");
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