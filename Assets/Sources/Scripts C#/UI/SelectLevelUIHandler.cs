using UnityEngine;

public class SelectLevelUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject _levelCanvas;
    [SerializeField] private SelectCarUIHandler _selectCarUIHandler;
    [SerializeField] private PowerUpSelectionPanel _powerUpSelectionPanel;

    private int _selectedLevelIndex = -1;

    public int SelectedLevelIndex => _selectedLevelIndex;

    public void ShowLevelSelectionUI()
    {
        _levelCanvas.SetActive(true);
    }

    public void HideLevelSelectionUI()
    {
        _levelCanvas.SetActive(false);
    }

    /// <summary>
    /// Вызывается при клике на кнопку уровня
    /// </summary>
    public void OnLevelSelected(int levelIndex)
    {
        _selectedLevelIndex = levelIndex;

        // Скрываем выбор уровня и переходим к PowerUp'ам
        HideLevelSelectionUI();
        _powerUpSelectionPanel.ShowPowerUpPanel();
    }

    /// <summary>
    /// Повесь этот метод на UI-кнопку "Назад" на экране выбора уровня
    /// </summary>
    public void OnBackToCarSelectionPressed()
    {
        HideLevelSelectionUI();
        _selectCarUIHandler.ShowCarSelectionUI();
    }
}