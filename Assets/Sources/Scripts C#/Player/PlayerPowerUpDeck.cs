using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPowerUpDeck
{
    private const string DeckKey = "SelectedPowerUps";
    private const string PurchasedPowerUpsKey = "PurchasedPowerUps";

    private readonly List<PowerUpType> selectedPowerUps = new List<PowerUpType>();
    private readonly HashSet<PowerUpType> purchasedPowerUps = new HashSet<PowerUpType>();

    public event Action OnDeckUpdated;

    public PlayerPowerUpDeck()
    {
        Load();
    }

    public bool IsPowerUpPurchased(PowerUpType type) => purchasedPowerUps.Contains(type);
    public bool IsPowerUpSelected(PowerUpType type) => selectedPowerUps.Contains(type);
    public List<PowerUpType> GetSelectedPowerUps() => new List<PowerUpType>(selectedPowerUps);

    /// <summary>
    /// Покупка PowerUp в магазине
    /// </summary>
    public bool TryPurchasePowerUp(PowerUpData data, PlayerWallet wallet)
    {
        if (data == null || wallet == null) return false;
        if (IsPowerUpPurchased(data.type)) return true;

        if (wallet.TrySpendMoney(data.price))
        {
            purchasedPowerUps.Add(data.type);
            Save();
            OnDeckUpdated?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Выбор/снятие PowerUp в селекте (Максимум 3)
    /// </summary>
    public bool ToggleSelectPowerUp(PowerUpType type)
    {
        if (!IsPowerUpPurchased(type)) return false;

        if (selectedPowerUps.Contains(type))
        {
            selectedPowerUps.Remove(type);
        }
        else
        {
            if (selectedPowerUps.Count >= 3) return false; // Максимум 3
            selectedPowerUps.Add(type);
        }

        Save();
        OnDeckUpdated?.Invoke();
        return true;
    }

    private void Save()
    {
        PlayerPrefs.SetString(DeckKey, string.Join(",", selectedPowerUps));
        PlayerPrefs.SetString(PurchasedPowerUpsKey, string.Join(",", purchasedPowerUps));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        selectedPowerUps.Clear();
        purchasedPowerUps.Clear();

        purchasedPowerUps.Add(PowerUpType.Boost);
        selectedPowerUps.Add(PowerUpType.Boost);

        if (PlayerPrefs.HasKey(PurchasedPowerUpsKey))
        {
            foreach (var str in PlayerPrefs.GetString(PurchasedPowerUpsKey).Split(','))
            {
                if (Enum.TryParse(str, out PowerUpType type))
                    purchasedPowerUps.Add(type);
            }
        }

        if (PlayerPrefs.HasKey(DeckKey))
        {
            foreach (var str in PlayerPrefs.GetString(DeckKey).Split(','))
            {
                if (Enum.TryParse(str, out PowerUpType type) && purchasedPowerUps.Contains(type))
                {
                    if (!selectedPowerUps.Contains(type) && selectedPowerUps.Count < 3)
                        selectedPowerUps.Add(type);
                }
            }
        }
    }
}