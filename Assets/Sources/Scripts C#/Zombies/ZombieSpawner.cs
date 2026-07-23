using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WaveData
    {
        [Tooltip("Название волны для удобства в инспекторе")]
        public string waveName = "Wave 1";

        [Tooltip("Список префабов зомби, которые могут спавниться на этом круге")]
        public List<GameObject> zombiePrefabs = new List<GameObject>();

        [Tooltip("Общее количество зомби на этом круге")]
        public int totalZombiesToSpawn = 15;

        [Tooltip("Задержка между спавном отдельных зомби (в секундах)")]
        public float spawnInterval = 0.3f;
    }

    [Header("Wave Settings")]
    [SerializeField] private List<WaveData> wavesConfig = new List<WaveData>();

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints; // Точки спавна на карте
    [SerializeField] private float spawnRadiusOffset = 2f; // Случайный разброс вокруг точки

    [Header("References")]
    [SerializeField] private CarLapCounter playerLapCounter;

    private int currentWaveIndex = 0;
    private Coroutine currentSpawnCoroutine;
    private List<GameObject> activeZombies = new List<GameObject>();

    public void SetPlayerCarLapCounter(CarLapCounter lapCounter)
    {
        playerLapCounter = lapCounter;
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        if (playerLapCounter != null)
        {
            playerLapCounter.OnLapCompleted += HandleLapCompleted;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        if (playerLapCounter != null)
        {
            playerLapCounter.OnLapCompleted -= HandleLapCompleted;
        }
    }

    private void HandleGameStateChanged(GameManager manager)
    {
        if (manager.GetGameState() == GameStates.running)
        {
            // Старт 1-го круга / 1-й волны
            currentWaveIndex = 0;
            StartWave(currentWaveIndex);
        }
        else if (manager.GetGameState() == GameStates.raceOver)
        {
            // Останавливаем спавн при завершении гонки
            if (currentSpawnCoroutine != null)
            {
                StopCoroutine(currentSpawnCoroutine);
            }
        }
    }

    private void HandleLapCompleted(CarLapCounter lapCounter)
    {
        // Проверяем, пересёк ли игрок именно финишную линию (и не окончена ли гонка)
        // Новую волну запускаем при начале НОВОГО круга
        if (!lapCounter.IsRaceCompleated())
        {
            // Увеличиваем индекс волны (каждый новый круг)
            currentWaveIndex++;

            if (currentWaveIndex < wavesConfig.Count)
            {
                StartWave(currentWaveIndex);
            }
        }
    }

    public void StartWave(int waveIndex)
    {
        if (waveIndex >= wavesConfig.Count)
        {
            Debug.LogWarning($"[ZombieSpawner] Волна с индексом {waveIndex} не настроена в Inspector!");
            return;
        }

        if (currentSpawnCoroutine != null)
        {
            StopCoroutine(currentSpawnCoroutine);
        }

        currentSpawnCoroutine = StartCoroutine(SpawnWaveRoutine(wavesConfig[waveIndex]));
    }

    private IEnumerator SpawnWaveRoutine(WaveData wave)
    {
        Debug.Log($"[ZombieSpawner] Запуск волны: {wave.waveName} (Зомби: {wave.totalZombiesToSpawn})");

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[ZombieSpawner] Не заданы Spawn Points в Inspector!");
            yield break;
        }

        if (wave.zombiePrefabs == null || wave.zombiePrefabs.Count == 0)
        {
            Debug.LogError($"[ZombieSpawner] У волны {wave.waveName} нет префабов зомби!");
            yield break;
        }

        for (int i = 0; i < wave.totalZombiesToSpawn; i++)
        {
            // 1. Выбираем случайный префаб зомби для текущей волны
            GameObject selectedPrefab = wave.zombiePrefabs[UnityEngine.Random.Range(0, wave.zombiePrefabs.Count)];

            // 2. Выбираем случайную точку спавна
            Transform selectedSpawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

            // 3. Добавляем небольшой случайный смещение вокруг точки (чтобы они не спавнились в одной точке)
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadiusOffset;
            Vector3 spawnPosition = selectedSpawnPoint.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            // 4. Спавним зомби
            GameObject zombie = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            activeZombies.Add(zombie);

            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    // Вспомогательный метод: если нужно очистить старых зомби с прошлого круга
    public void ClearAllActiveZombies()
    {
        foreach (var zombie in activeZombies)
        {
            if (zombie != null)
            {
                Destroy(zombie);
            }
        }
        activeZombies.Clear();
    }
}