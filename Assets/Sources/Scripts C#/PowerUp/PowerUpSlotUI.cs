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
    [SerializeField] private GameObject selectedOutline;   // Подсветка "Экипировано/Куплено"
    [SerializeField] private GameObject lockedOverlay;     // Затеняющий слой, если не куплено
    [SerializeField] private GameObject buyPriceContainer; // Контейнер с ценой (опционально)

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

        if (data == null) return;

        // 1. Установка иконки
        if (iconImage != null)
            iconImage.sprite = data.icon;

        // 2. Установка названия Power Up'а
        if (titleText != null)
            titleText.text = data.title;

        // 3. Начальная установка цены
        if (priceText != null)
            priceText.text = $"{data.price}$";
    }

    /// <summary>
    /// Обновляет визуальное состояние слота на основе данных из PowerUpDeck
    /// </summary>
    public void UpdateState(bool isPurchased, bool isSelected)
    {
        if (Data == null) return;

        // 1. Подсветка активного выбора
        if (selectedOutline != null)
            selectedOutline.SetActive(isSelected);

        // 2. Оверлей блокировки (если вы используете затемнение для некупленных)
        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isPurchased);

        // 3. Динамическая смена текста "Цена / Bought!"
        if (priceText != null)
        {
            if (isPurchased)
            {
                priceText.text = "Bought!";
            }
            else
            {
                priceText.text = $"{Data.price}$";
            }
        }

        // Если вы НЕ скрываете контейнер с ценой целиком, оставьте эту строку закомментированной 
        // или включенной, в зависимости от того, как сверстан UI в Unity
        if (buyPriceContainer != null)
        {
            buyPriceContainer.SetActive(true); // Контейнер всегда активен, меняется только текст priceText
        }
    }
}