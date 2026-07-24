using UnityEngine;

public class PlayerWallet
{
    private const string SaveKey = "PlayerBalance";

    public int Balance { get; private set; }

    public event System.Action<int> OnMoneyChanged;

    public PlayerWallet()
    {
        Load();
    }

    public void SaveEarnedMoney(int amount)
    {
        if (amount <= 0) return;

        Balance += amount;
        OnMoneyChanged?.Invoke(Balance);
        PlayerPrefs.SetInt(SaveKey, Balance);
        PlayerPrefs.Save();
        Debug.Log($"[PlayerWallet] Сохранено: +{amount}$. Новый итоговый баланс: {Balance}$");
    }

    /// <summary>
    /// Пополнение счета или возврат средств за отмененный PowerUp
    /// </summary>
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        Balance += amount;
        OnMoneyChanged?.Invoke(Balance);
        PlayerPrefs.SetInt(SaveKey, Balance);
        PlayerPrefs.Save();
    }

    public bool TrySpendMoney(int amount)
    {
        if (Balance >= amount)
        {
            Balance -= amount;
            OnMoneyChanged?.Invoke(Balance);
            PlayerPrefs.SetInt(SaveKey, Balance);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    private void Load()
    {
        Balance = PlayerPrefs.GetInt(SaveKey, 0);
    }
}