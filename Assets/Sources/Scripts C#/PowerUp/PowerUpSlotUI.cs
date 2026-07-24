using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button actionButton;

    [Header("Visual Indicators")]
    [SerializeField] private GameObject selectedOutline;  // Подсветка "Экипировано"
    [SerializeField] private GameObject lockedOverlay;    // Затеняющий слой, если не куплено
    [SerializeField] private GameObject buyPriceContainer;// Контейнер с ценой (скрываем, если куплено)

    public PowerUpData Data { get; private set; }

    private Action<PowerUpSlotUI> _onSlotClicked;

    private void Awake()
    {
        if (actionButton != null)
            actionButton.onClick.AddListener(() => _onSlotClicked?.Invoke(this));
    }

    public void Setup(PowerUpData data, Action<PowerUpSlotUI> onClickCallback)
    {
        Data = data;
        _onSlotClicked = onClickCallback;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (titleText != null) titleText.text = data.title;
        if (priceText != null) priceText.text = $"{data.price}$";
    }

    /// <summary>
    /// Обновляет визуальное состояние слота на основе данных из PowerUpDeck
    /// </summary>
    public void UpdateState(bool isPurchased, bool isSelected)
    {
        // 1. Индикатор выбора
        if (selectedOutline != null)
            selectedOutline.SetActive(isSelected);

        // 2. Отображение покупки
        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isPurchased);

        if (buyPriceContainer != null)
            buyPriceContainer.SetActive(!isPurchased);
    }
}