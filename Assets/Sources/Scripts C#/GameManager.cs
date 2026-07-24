using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStates { countDown, running, raceOver };

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    GameStates gameState = GameStates.countDown;

    float raceStartedTime = 0;
    float raceCompletedTime = 0;

    List<DriverInfo> driverInfoList = new List<DriverInfo>();

    [Header("Zombie Rewards System")]
    [SerializeField] private int baseZombieReward = 10;

    public PlayerWallet Wallet { get; private set; }
    public PlayerGarage Garage { get; private set; }
    public PlayerPowerUpDeck PowerUpDeck { get; private set; }

    public int PendingMoney { get; private set; }
    public int CurrentLapMultiplier { get; private set; } = 1;

    public event Action<GameManager> OnGameStateChanged;
    public event Action<int> OnPendingMoneyChanged;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Wallet = new PlayerWallet();
            Garage = new PlayerGarage();
            PowerUpDeck = new PlayerPowerUpDeck();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        driverInfoList.Add(new DriverInfo(1, "P1", 0, UnityEngine.Random.Range(0, 1), false));
    }

    void LevelStart()
    {
        gameState = GameStates.countDown;

        ResetRaceMoney();

        Debug.Log("Level started");
    }

    public GameStates GetGameState()
    {
        return gameState;
    }

    void ChangeGameState(GameStates newGameState)
    {
        if (gameState != newGameState)
        {
            gameState = newGameState;

            OnGameStateChanged?.Invoke(this);
        }
    }

    public float GetRaceTime()
    {
        if (gameState == GameStates.countDown)
            return 0;
        else if (gameState == GameStates.raceOver)
            return raceCompletedTime - raceStartedTime;
        else return Time.time - raceStartedTime;
    }

    public void ClearDriversList()
    {
        driverInfoList.Clear();
    }

    public void AddDriverToList(int playerName, string name, int carUniqueID, int carColorIndex, bool isAI)
    {
        driverInfoList.Add(new DriverInfo(playerName, name, carUniqueID, carColorIndex, isAI));
    }

    public void SetDriversLastRacePosition(int playerNumber, int position)
    {
        DriverInfo driverInfo = FindDriverInfo(playerNumber);
        if (driverInfo != null) driverInfo.lastRacePosition = position;
    }

    public void AddPointsToChampionship(int playerNumber, int points)
    {
        DriverInfo driverInfo = FindDriverInfo(playerNumber);
        if (driverInfo != null) driverInfo.championshipPoints += points;
    }

    DriverInfo FindDriverInfo(int playerNumber)
    {
        foreach (DriverInfo driverInfo in driverInfoList)
        {
            if (playerNumber == driverInfo.playerNumber)
                return driverInfo;
        }

        Debug.LogError($"FindDriverInfoBasedOnDriverNumber failed for player number: {playerNumber}");
        return null;
    }

    public List<DriverInfo> GetDriverList()
    {
        return driverInfoList;
    }

    public void OnRaceStart()
    {
        Debug.Log("Race started");

        raceStartedTime = Time.time;

        ChangeGameState(GameStates.running);
    }

    public void OnRaceCompleated()
    {
        Debug.Log("Race compleated");

        raceCompletedTime = Time.time;

        if (PendingMoney > 0 && Wallet != null)
        {
            Wallet.SaveEarnedMoney(PendingMoney);
        }

        ChangeGameState(GameStates.raceOver);
    }

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LevelStart();
    }

    #region Zombie Reward System Logic

    public void SetCurrentLapMultiplier(int lap)
    {
        CurrentLapMultiplier = Mathf.Max(1, lap);
    }

    public void AddZombieKillReward()
    {
        if (gameState != GameStates.running) return;

        int reward = baseZombieReward * CurrentLapMultiplier;
        PendingMoney += reward;

        OnPendingMoneyChanged?.Invoke(PendingMoney);
        Debug.Log($"[GameManager] Зомби уничтожен! +{reward}$ (Множитель круга x{CurrentLapMultiplier}). Временный счет: {PendingMoney}$");
    }

    private void ResetRaceMoney()
    {
        PendingMoney = 0;
        CurrentLapMultiplier = 1;
        OnPendingMoneyChanged?.Invoke(PendingMoney);
    }

    #endregion
}