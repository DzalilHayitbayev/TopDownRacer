using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectCarUIHandler : MonoBehaviour
{
    [Header("Car prefab")]
    public GameObject carPrefab;

    [Header("Spawn on")]
    public Transform spawnOnTransform;

    [Header("UI Controls")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private TMP_Text statusInfoText; // Текст состояния (например: "Car: 500$" или "Color: 100$")

    private bool isChangingCar = false;
    private CarUIHandler carUIHandler = null;
    private CarData[] carDatas;

    private int selectedCarIndex = 0;
    private int selectedCarColorIndex = 0;

    private void Start()
    {
        carDatas = Resources.LoadAll<CarData>("CarData/").OrderBy(c => c.CarUniqueID).ToArray();
        StartCoroutine(SpawnCarCO(true));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) OnPreviousCar();
        else if (Input.GetKeyDown(KeyCode.RightArrow)) OnNextCar();

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            OnMainActionButtonPressed();
        }
    }

    public void OnPreviousCar()
    {
        if (isChangingCar) return;
        selectedCarIndex--;
        if (selectedCarIndex < 0) selectedCarIndex = carDatas.Length - 1;

        StartCoroutine(SpawnCarCO(true));
    }

    public void OnNextCar()
    {
        if (isChangingCar) return;
        selectedCarIndex++;
        if (selectedCarIndex > carDatas.Length - 1) selectedCarIndex = 0;

        StartCoroutine(SpawnCarCO(false));
    }

    public void OnPreviousCarColor()
    {
        if (isChangingCar) return;

        CarData currentCar = carDatas[selectedCarIndex];
        selectedCarColorIndex--;
        if (selectedCarColorIndex < 0)
            selectedCarColorIndex = currentCar.CarColorSchemes.Length - 1;

        carUIHandler.ChangeCarSprite(selectedCarColorIndex, currentCar);
        UpdateUIState();
    }

    public void OnNextCarColor()
    {
        if (isChangingCar) return;

        CarData currentCar = carDatas[selectedCarIndex];
        selectedCarColorIndex++;
        if (selectedCarColorIndex > currentCar.CarColorSchemes.Length - 1)
            selectedCarColorIndex = 0;

        carUIHandler.ChangeCarSprite(selectedCarColorIndex, currentCar);
        UpdateUIState();
    }

    /// <summary>
    /// Нажатие на главную кнопку (RACE / BUY CAR / BUY COLOR)
    /// </summary>
    public void OnMainActionButtonPressed()
    {
        if (isChangingCar) return;

        CarData currentCar = carDatas[selectedCarIndex];
        PlayerGarage garage = GameManager.Instance.Garage;
        PlayerWallet wallet = GameManager.Instance.Wallet;

        bool isCarUnlocked = garage.IsCarUnlocked(currentCar.CarUniqueID);
        bool isColorUnlocked = garage.IsColorUnlocked(currentCar.CarUniqueID, selectedCarColorIndex);

        // 1. Если не куплена сама машина — покупаем машину
        if (!isCarUnlocked)
        {
            if (garage.TryPurchaseCar(currentCar, wallet))
            {
                UpdateUIState();
            }
            else
            {
                Debug.Log("Недостаточно денег для покупки машины!");
            }
            return;
        }

        // 2. Если машина куплена, но выбранный цвет не куплен — покупаем цвет
        if (!isColorUnlocked)
        {
            if (garage.TryPurchaseColor(currentCar, selectedCarColorIndex, wallet))
            {
                UpdateUIState();
            }
            else
            {
                Debug.Log("Недостаточно денег для покупки цвета!");
            }
            return;
        }

        // 3. Если и машина, и цвет куплены — запускаем гонку!
        StartRaceWithSelectedCar();
    }

    private void UpdateUIState()
    {
        CarData currentCar = carDatas[selectedCarIndex];
        PlayerGarage garage = GameManager.Instance.Garage;

        bool isCarUnlocked = garage.IsCarUnlocked(currentCar.CarUniqueID);
        bool isColorUnlocked = garage.IsColorUnlocked(currentCar.CarUniqueID, selectedCarColorIndex);

        if (!isCarUnlocked)
        {
            // Машина заблокирована
            if (actionButtonText != null) actionButtonText.text = $"BUY CAR ({currentCar.Price}$)";
            if (statusInfoText != null) statusInfoText.text = "Car Locked";
        }
        else if (!isColorUnlocked)
        {
            // Машина есть, но выбранный цвет заблокирован
            int colorPrice = currentCar.CarColorSchemes[selectedCarColorIndex].Price;
            if (actionButtonText != null) actionButtonText.text = $"BUY COLOR ({colorPrice}$)";
            if (statusInfoText != null) statusInfoText.text = "Color Locked";
        }
        else
        {
            // Всё куплено
            if (actionButtonText != null) actionButtonText.text = "RACE";
            if (statusInfoText != null) statusInfoText.text = "Ready!";
        }
    }

    private void StartRaceWithSelectedCar()
    {
        GameManager.Instance.ClearDriversList();
        GameManager.Instance.AddDriverToList(1, "P1", carDatas[selectedCarIndex].CarUniqueID, selectedCarColorIndex, false);

        List<CarData> uniqueCars = new List<CarData>(carDatas);
        uniqueCars.Remove(carDatas[selectedCarIndex]);

        string[] names = { "Osama", "P.Diddy", "Ye", "Adolf.H", "Max" };
        List<string> uniqueNames = names.ToList();

        for (int i = 2; i < 5; i++)
        {
            if (uniqueNames.Count == 0 || uniqueCars.Count == 0) break;

            string driverName = uniqueNames[Random.Range(0, uniqueNames.Count)];
            uniqueNames.Remove(driverName);

            CarData carData = uniqueCars[Random.Range(0, uniqueCars.Count)];

            GameManager.Instance.AddDriverToList(i, driverName, carData.CarUniqueID, selectedCarColorIndex, true);
        }

        SceneManager.LoadScene(1);
    }

    private IEnumerator SpawnCarCO(bool isCarAppearingOnRightSide)
    {
        isChangingCar = true;

        if (carUIHandler != null)
            carUIHandler.StartCarExitAnimation(!isCarAppearingOnRightSide);

        GameObject instantiatedCar = Instantiate(carPrefab, spawnOnTransform);

        carUIHandler = instantiatedCar.GetComponent<CarUIHandler>();
        carUIHandler.SetUpCar(carDatas[selectedCarIndex]);
        carUIHandler.StartCarEnteranceAnimation(isCarAppearingOnRightSide);

        selectedCarColorIndex = 0;
        UpdateUIState();

        yield return new WaitForSeconds(0.4f);

        isChangingCar = false;
    }
}