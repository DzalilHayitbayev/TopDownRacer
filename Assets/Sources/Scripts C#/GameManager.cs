using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStates { countDown, running, raceOver };

// Причина окончания гонки для игрока
public enum RaceResult { Completed, Destroyed, Aborted }

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
    public RaceResult LastRaceResult { get; private set; } = RaceResult.Completed;
    private Health playerHealth;

    public event Action<GameManager> OnGameStateChanged;
    public event Action<int> OnPendingMoneyChanged;
    public event Action<RaceResult> OnRaceEndedWithResult;


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

    public void RaceCompleateStateChange(RaceResult raceResult)
    {
        LastRaceResult = raceResult;
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

    // 1. Успешный финиш гонки
    public void OnRaceCompleated()
    {
        if (gameState == GameStates.raceOver) return;

        Debug.Log("Race completed successfully!");

        LastRaceResult = RaceResult.Completed;
        raceCompletedTime = Time.time;

        if (PendingMoney > 0 && Wallet != null)
        {
            Wallet.SaveEarnedMoney(PendingMoney);
        }

        // Заряды больше НЕ сгорают здесь — игрок сможет перезапустить гонку с ними
        ChangeGameState(GameStates.raceOver);
        OnRaceEndedWithResult?.Invoke(LastRaceResult);
    }

    // 2. Уничтожение игрока во время гонки
    public void OnPlayerDestroyed()
    {
        if (gameState == GameStates.raceOver) return;

        Debug.Log("Player was destroyed! Race Over.");

        LastRaceResult = RaceResult.Destroyed;
        raceCompletedTime = Time.time;

        // При уничтожении заработанные деньги сгорают
        PendingMoney = 0;
        OnPendingMoneyChanged?.Invoke(PendingMoney);

        // Заряды больше НЕ сгорают здесь
        ChangeGameState(GameStates.raceOver);
        OnRaceEndedWithResult?.Invoke(LastRaceResult);
    }

    // 3. Выход из гонки через меню паузы
    public void OnPlayerQuitRace()
    {
        if (gameState == GameStates.raceOver) return;

        Debug.Log("Player quit the race via Pause Menu.");

        LastRaceResult = RaceResult.Aborted;
        raceCompletedTime = Time.time;

        // Накопленные деньги за заезд не начисляются
        PendingMoney = 0;
        OnPendingMoneyChanged?.Invoke(PendingMoney);

        UnregisterPlayerHealth();
        ChangeGameState(GameStates.raceOver);
        OnRaceEndedWithResult?.Invoke(LastRaceResult);
    }

    /// <summary>
    /// Вызывай этот метод при нажатии на кнопку "Выход в меню" в UI
    /// </summary>
    public void ClearDeckAndSave()
    {
        PowerUpDeck?.ClearDeckAndSave();;
    }

    #region Player Registration Logic
    public void RegisterPlayerHealth(Health health)
    {
        UnregisterPlayerHealth(); // Отписываемся от старого, если был

        playerHealth = health;
        if (playerHealth != null)
        {
            playerHealth.OnDied += HandlePlayerDied;
        }
    }

    public void UnregisterPlayerHealth()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDied -= HandlePlayerDied;
            playerHealth = null;
        }
    }

    private void HandlePlayerDied()
    {
        OnPlayerDestroyed();
    }

    #endregion

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnregisterPlayerHealth();
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