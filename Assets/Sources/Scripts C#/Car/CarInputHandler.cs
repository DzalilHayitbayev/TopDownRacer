using UnityEngine;
using UnityEngine.InputSystem;

public class CarInputHandler : MonoBehaviour
{
    public int playerNumber = 1;
    public bool isUIInput = false;

    private Vector2 inputVector = Vector2.zero;

    private TopDownCarController topDownCarController;
    private CarPowerUpInventory powerUpInventory;
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
        powerUpInventory = GetComponent<CarPowerUpInventory>();

        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();

        // Подписываемся на нажатие кнопки активации PowerUp
        playerInputActions.Player.UsePowerUp.performed += OnUsePowerUpPerformed;
    }

    private void OnDisable()
    {
        // Отписываемся, чтобы избежать утечек памяти
        playerInputActions.Player.UsePowerUp.performed -= OnUsePowerUpPerformed;
        playerInputActions.Player.Disable();
    }

    private void Update()
    {
        GetMovementVectorNormalized();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        if (isUIInput)
        {
            // Управление через UI кнопки/джойстик (если применяется)
        }
        else
        {
            switch (playerNumber)
            {
                case 1:
                    inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
                    break;
                case 2:
                    inputVector = playerInputActions.Player.Move2.ReadValue<Vector2>();
                    break;
                case 3:
                    inputVector = playerInputActions.Player.Move3.ReadValue<Vector2>();
                    break;
                case 4:
                    inputVector = playerInputActions.Player.Move4.ReadValue<Vector2>();
                    break;
            }
        }

        topDownCarController.SetInputVector(inputVector);

        return inputVector.normalized;
    }

    public void SetInput(Vector2 newInput)
    {
        inputVector = newInput;
    }

    private void OnUsePowerUpPerformed(InputAction.CallbackContext context)
    {
        if (isUIInput) return;

        if (powerUpInventory != null)
        {
            powerUpInventory.ActivatePowerUp();
        }
    }
    public void OnPowerUpUIButtonClick()
    {
        if (powerUpInventory != null)
        {
            powerUpInventory.ActivatePowerUp();
        }
    }
}