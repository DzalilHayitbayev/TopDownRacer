using UnityEngine;

public class CarInputHandler : MonoBehaviour
{
    public int playerNumber = 1;
    public bool isUIInput = false;

    Vector2 inputVector = Vector2.zero;

    TopDownCarController topDownCarController;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    private void Update()
    {
        GetMovementVectorNormalized();
    }

    public Vector2 GetMovementVectorNormalized()
    {

        if (isUIInput)
        {

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

        inputVector = inputVector.normalized;

        return inputVector;
    }

    public void SetInput(Vector2 newInput)
    {
        inputVector = newInput;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Disable();
    }
}
