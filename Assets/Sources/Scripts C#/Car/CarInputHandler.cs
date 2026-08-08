using UnityEngine;
using UnityEngine.InputSystem;

public class CarInputHandler : MonoBehaviour
{
    public int playerNumber = 1;
    public bool isUIInput = false;

    private Vector2 inputVector = Vector2.zero;

    private TopDownCarController topDownCarController;
    private CarPowerUpInventory powerUpInventory;
    private DefaultPowerUpController defaultPowerUpController;
    private PlayerInputActions playerInputActions;

    public PlayerInputActions InputActions => playerInputActions;

    private void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
        powerUpInventory = GetComponent<CarPowerUpInventory>();
        defaultPowerUpController = GetComponentInChildren<DefaultPowerUpController>();

        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();

        // Подписываемся на .started (момент физического нажатия) и .canceled (отпускание)
        playerInputActions.Player.UseDefaultPowerUp.started += OnDefaultPowerUpStarted;
        playerInputActions.Player.UseDefaultPowerUp.canceled += OnDefaultPowerUpCanceled;

        playerInputActions.Player.UsePowerUp.performed += OnUsePowerUpPerformed;
    }

    private void OnDisable()
    {
        playerInputActions.Player.UseDefaultPowerUp.started -= OnDefaultPowerUpStarted;
        playerInputActions.Player.UseDefaultPowerUp.canceled -= OnDefaultPowerUpCanceled;
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
            // Управление через UI
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

        if (topDownCarController != null)
        {
            topDownCarController.SetInputVector(inputVector);
        }

        return inputVector.normalized;
    }

    public void SetInput(Vector2 newInput)
    {
        inputVector = newInput;
    }

    #region Default PowerUp (Saw Blades / Left Shift)

    private void OnDefaultPowerUpStarted(InputAction.CallbackContext context)
    {
        if (isUIInput) return;

        if (defaultPowerUpController != null)
        {
            defaultPowerUpController.ActivatePowerUp();
        }
    }

    private void OnDefaultPowerUpCanceled(InputAction.CallbackContext context)
    {
        if (isUIInput) return;

        if (defaultPowerUpController != null)
        {
            defaultPowerUpController.DeactivatePowerUp();
        }
    }

    public void OnDefaultPowerUpButtonDown()
    {
        if (defaultPowerUpController != null)
            defaultPowerUpController.ActivatePowerUp();
    }

    public void OnDefaultPowerUpButtonUp()
    {
        if (defaultPowerUpController != null)
            defaultPowerUpController.DeactivatePowerUp();
    }

    #endregion

    #region Inventory PowerUp

    public void OnUsePowerUpPerformed(InputAction.CallbackContext context)
    {
        if (isUIInput) return;

        if (powerUpInventory != null)
        {
            powerUpInventory.ActivatePowerUp();
        }
    }

    #endregion
}