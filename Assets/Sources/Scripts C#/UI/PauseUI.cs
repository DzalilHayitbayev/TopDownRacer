using UnityEngine;
using UnityEngine.InputSystem;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;

    private PlayerInputActions playerInputActions;
    private bool isPaused = false;

    private void Awake()
    {
        // Скрываем паузу на старте
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    private void OnDisable()
    {
        UnsubscribeInput();
    }

    /// <summary>
    /// Вызывается из SpawnCars при спавне машины игрока
    /// </summary>
    public void Setup(PlayerInputActions inputActions)
    {
        UnsubscribeInput();

        playerInputActions = inputActions;

        if (playerInputActions != null)
        {
            // Карта UI должна быть включена ВСЕГДА (и в игре, и в паузе)
            playerInputActions.UI.Enable();
            playerInputActions.UI.Pause.started += OnPauseStarted;
        }
    }

    private void UnsubscribeInput()
    {
        if (playerInputActions != null)
        {
            playerInputActions.UI.Pause.started -= OnPauseStarted;
        }
    }

    private void OnPauseStarted(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        // 1. Показываем/скрываем меню паузы
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(isPaused);
        }

        // 2. Останавливаем/возобновляем время
        Time.timeScale = isPaused ? 0f : 1f;

        // 3. Отключаем/включаем только PLAYER карту (управление машиной)
        if (playerInputActions != null)
        {
            if (isPaused)
            {
                playerInputActions.Player.Disable();
            }
            else
            {
                playerInputActions.Player.Enable();
            }
        }
    }

    // --- UI BUTTON EVENTS ---

    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerQuitRace();
        }
        pauseMenuUI.SetActive(false);
    }
}