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
        if (_deck == null || slot.Data == null) return;

        PowerUpType type = slot.Data.type;

        if (!_deck.IsPowerUpPurchased(type))
        {
            bool purchased = _deck.TryPurchasePowerUp(slot.Data, _wallet);
            if (!purchased)
            {
                Debug.LogWarning("[UI] Недостаточно денег для покупки!");
            }
            return;
        }

        bool success = _deck.ToggleSelectPowerUp(type);
        if (!success)
        {
            Debug.LogWarning("[UI] Нельзя выбрать больше 3 PowerUp'ов!");
        }
    }

    public void RefreshUI()
    {
        if (_deck == null) return;

        foreach (var slot in uiSlots)
        {
            if (slot.Data == null) continue;

            bool isPurchased = _deck.IsPowerUpPurchased(slot.Data.type);
            bool isSelected = _deck.IsPowerUpSelected(slot.Data.type);

            slot.UpdateState(isPurchased, isSelected);
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