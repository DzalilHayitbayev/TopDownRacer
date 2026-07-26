using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PowerUpSelectionPanel : MonoBehaviour
{
    [Header("Panel Root Canvas/GameObject")]
    [SerializeField] private GameObject panelRoot;

    [Header("Flow References")]
    [SerializeField] private SelectLevelUIHandler selectLevelUIHandler;

    [Header("All Available PowerUps (6 items)")]
    [SerializeField] private List<PowerUpData> allPowerUps = new List<PowerUpData>();

    [Header("UI Slots (6 buttons)")]
    [SerializeField] private List<PowerUpSlotUI> uiSlots = new List<PowerUpSlotUI>();

    private PlayerPowerUpDeck _deck;
    private PlayerWallet _wallet;

    private void Awake()
    {
        // Инициализируем слоты при старте объекта
        InitSlots();
    }

    private void OnEnable()
    {
        // Гарантируем получение актуальных ссылок из GameManager до подписки
        EnsureReferences();

        if (_deck != null)
        {
            _deck.OnDeckUpdated -= RefreshUI; // Защита от дублирования подписки
            _deck.OnDeckUpdated += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (_deck != null)
        {
            _deck.OnDeckUpdated -= RefreshUI;
        }
    }

    private void Start()
    {
        EnsureReferences();
        RefreshUI();
    }

    private void EnsureReferences()
    {
        if (GameManager.Instance != null)
        {
            if (_deck == null) _deck = GameManager.Instance.PowerUpDeck;
            if (_wallet == null) _wallet = GameManager.Instance.Wallet;
        }
    }

    public void ShowPowerUpPanel()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        else gameObject.SetActive(true);

        EnsureReferences();
        RefreshUI();
    }

    public void HidePowerUpPanel()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        else gameObject.SetActive(false);
    }

    private void InitSlots()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < allPowerUps.Count)
            {
                uiSlots[i].gameObject.SetActive(true);
                uiSlots[i].Setup(allPowerUps[i], OnSlotClicked);
            }
            else
            {
                uiSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnSlotClicked(PowerUpSlotUI slot)
    {
        EnsureReferences();

        if (_deck == null || slot.Data == null || _wallet == null)
        {
            Debug.LogError("[PowerUpSelectionPanel] _deck или _wallet не найдены в GameManager!");
            return;
        }

        // Переключаем покупку/продажу
        bool success = _deck.TogglePurchasePowerUp(slot.Data, _wallet);

        // Явный вызов обновления UI на случай, если событие где-то потерялось
        if (success)
        {
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        EnsureReferences();

        if (_deck == null) return;

        foreach (var slot in uiSlots)
        {
            if (slot.Data == null) continue;

            bool isSelected = _deck.IsPowerUpSelected(slot.Data.type);

            // Передаем статус покупки/выбора в слот
            slot.UpdateState(isSelected, isSelected);
        }
    }

    public void OnStartRaceButtonPressed()
    {
        int targetScene = selectLevelUIHandler != null ? selectLevelUIHandler.SelectedLevelIndex : -1;
        if (targetScene >= 0)
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogError("Уровень не выбран!");
        }
    }

    public void OnBackToLevelSelectionPressed()
    {
        HidePowerUpPanel();
        if (selectLevelUIHandler != null)
        {
            selectLevelUIHandler.ShowLevelSelectionUI();
        }
    }
}