using UnityEngine;
using UnityEngine.UI;

public class PowerUpDisplayUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject emptyStateVisual; // Опционально: визуал когда пустая ячейка

    [Header("Settings")]
    [SerializeField] private bool hideIconWhenEmpty = true;

    private CarPowerUpInventory _playerInventory;


    private void OnEnable()
    {
        if (_playerInventory != null)
            _playerInventory.OnPowerUpChanged += UpdateDisplay;
    }

    private void OnDisable()
    {
        if (_playerInventory != null)
            _playerInventory.OnPowerUpChanged -= UpdateDisplay;
    }

    public void InjectPlayerInventory(CarPowerUpInventory inventory)
    {
        SetInventory(inventory);
        UpdateDisplay(_playerInventory != null ? _playerInventory.currentPowerUp : null);
    }

    public void SetInventory(CarPowerUpInventory inventory)
    {
        if (_playerInventory != null)
            _playerInventory.OnPowerUpChanged -= UpdateDisplay;

        _playerInventory = inventory;

        if (_playerInventory != null)
        {
            _playerInventory.OnPowerUpChanged += UpdateDisplay;
            UpdateDisplay(_playerInventory.currentPowerUp);
        }
    }

    private void UpdateDisplay(PowerUpData data)
    {
        if (data != null && data.icon != null)
        {
            // Показывыаем иконку закупленного/поднятого пауэрапа
            if (iconImage != null)
            {
                iconImage.sprite = data.icon;
                iconImage.gameObject.SetActive(true);
            }

            if (emptyStateVisual != null)
                emptyStateVisual.SetActive(false);
        }
        else
        {
            // Ячейка пуста
            if (iconImage != null && hideIconWhenEmpty)
            {
                iconImage.gameObject.SetActive(false);
            }

            if (emptyStateVisual != null)
                emptyStateVisual.SetActive(true);
        }
    }
}