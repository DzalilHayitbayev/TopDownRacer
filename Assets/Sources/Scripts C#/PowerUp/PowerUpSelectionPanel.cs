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

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            _deck = GameManager.Instance.PowerUpDeck;
            _wallet = GameManager.Instance.Wallet;
        }

        InitSlots();
        RefreshUI();
    }

    private void OnEnable()
    {
        if (_deck != null)
            _deck.OnDeckUpdated += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (_deck != null)
            _deck.OnDeckUpdated -= RefreshUI;
    }

    public void ShowPowerUpPanel()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        else gameObject.SetActive(true);

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
        if (_deck == null || slot.Data == null || _wallet == null) return;

        // Переключаем покупку/отмену закупки на одну гонку
        _deck.TogglePurchasePowerUp(slot.Data, _wallet);
    }

    public void RefreshUI()
    {
        if (_deck == null) return;

        foreach (var slot in uiSlots)
        {
            if (slot.Data == null) continue;

            // В одноразовой системе "куплен на гонку" и "выбран" — это одно состояние
            bool isSelected = _deck.IsPowerUpSelected(slot.Data.type);

            slot.UpdateState(isSelected, isSelected);
        }
    }

    public void OnStartRaceButtonPressed()
    {
        int targetScene = selectLevelUIHandler.SelectedLevelIndex;
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
        selectLevelUIHandler.ShowLevelSelectionUI();
    }
}