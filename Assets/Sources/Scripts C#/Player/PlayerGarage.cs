using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGarage
{
    private const string PurchasedCarsKey = "PurchasedCars";
    private const string PurchasedColorsKey = "PurchasedColors";

    private readonly List<int> purchasedCarIDs = new List<int>();
    // Храним купленные цвета в формате: "CarID_ColorIndex" (например, "0_1", "2_3")
    private readonly HashSet<string> purchasedColorKeys = new HashSet<string>();

    public event Action OnGarageUpdated;

    public PlayerGarage()
    {
        Load();
    }

    #region Car Purchase Logic

    public bool IsCarUnlocked(int carUniqueID)
    {
        return purchasedCarIDs.Contains(carUniqueID);
    }

    public bool TryPurchaseCar(CarData carData, PlayerWallet wallet)
    {
        if (carData == null || wallet == null) return false;

        if (IsCarUnlocked(carData.CarUniqueID)) return true;

        if (wallet.TrySpendMoney(carData.Price))
        {
            purchasedCarIDs.Add(carData.CarUniqueID);
            // При покупке машины автоматически разблокируем ее дефолтный цвет (0)
            UnlockColorInternal(carData.CarUniqueID, 0);

            Save();
            OnGarageUpdated?.Invoke();
            Debug.Log($"[PlayerGarage] Куплена машина ID: {carData.CarUniqueID}");
            return true;
        }

        return false;
    }

    #endregion

    #region Color Purchase Logic

    /// <summary>
    /// Проверяет, куплен ли указанный цвет для конкретной машины
    /// </summary>
    public bool IsColorUnlocked(int carUniqueID, int colorIndex)
    {
        // Индекс 0 всегда разблокирован по умолчанию
        if (colorIndex == 0) return true;

        string key = GetColorKey(carUniqueID, colorIndex);
        return purchasedColorKeys.Contains(key);
    }

    /// <summary>
    /// Покупка цвета для машины
    /// </summary>
    public bool TryPurchaseColor(CarData carData, int colorIndex, PlayerWallet wallet)
    {
        if (carData == null || wallet == null) return false;

        // Машина должна быть сначала куплена
        if (!IsCarUnlocked(carData.CarUniqueID)) return false;

        if (IsColorUnlocked(carData.CarUniqueID, colorIndex)) return true;

        if (colorIndex < 0 || colorIndex >= carData.CarColorSchemes.Length) return false;

        int colorPrice = carData.CarColorSchemes[colorIndex].Price;

        if (wallet.TrySpendMoney(colorPrice))
        {
            UnlockColorInternal(carData.CarUniqueID, colorIndex);
            Save();
            OnGarageUpdated?.Invoke();
            Debug.Log($"[PlayerGarage] Куплен цвет {colorIndex} для машины ID: {carData.CarUniqueID} за {colorPrice}$");
            return true;
        }

        return false;
    }

    private void UnlockColorInternal(int carUniqueID, int colorIndex)
    {
        string key = GetColorKey(carUniqueID, colorIndex);
        if (!purchasedColorKeys.Contains(key))
        {
            purchasedColorKeys.Add(key);
        }
    }

    private string GetColorKey(int carUniqueID, int colorIndex)
    {
        return $"{carUniqueID}_{colorIndex}";
    }

    #endregion

    #region Save / Load

    private void Save()
    {
        // Сохраняем машины
        string carsData = string.Join(",", purchasedCarIDs);
        PlayerPrefs.SetString(PurchasedCarsKey, carsData);

        // Сохраняем цвета
        string colorsData = string.Join(",", purchasedColorKeys);
        PlayerPrefs.SetString(PurchasedColorsKey, colorsData);

        PlayerPrefs.Save();
    }

    private void Load()
    {
        purchasedCarIDs.Clear();
        purchasedColorKeys.Clear();

        // По умолчанию разблокирована машина 0 и ее 0-й цвет
        purchasedCarIDs.Add(0);
        purchasedColorKeys.Add(GetColorKey(0, 0));

        // Загрузка машин
        if (PlayerPrefs.HasKey(PurchasedCarsKey))
        {
            string savedCars = PlayerPrefs.GetString(PurchasedCarsKey);
            if (!string.IsNullOrEmpty(savedCars))
            {
                foreach (string idStr in savedCars.Split(','))
                {
                    if (int.TryParse(idStr, out int id) && !purchasedCarIDs.Contains(id))
                    {
                        purchasedCarIDs.Add(id);
                    }
                }
            }
        }

        // Загрузка цветов
        if (PlayerPrefs.HasKey(PurchasedColorsKey))
        {
            string savedColors = PlayerPrefs.GetString(PurchasedColorsKey);
            if (!string.IsNullOrEmpty(savedColors))
            {
                foreach (string key in savedColors.Split(','))
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        purchasedColorKeys.Add(key);
                    }
                }
            }
        }
    }

    #endregion
}