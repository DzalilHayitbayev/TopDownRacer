using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpBox : MonoBehaviour
{
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private GameObject visualModel;

    private List<PowerUpData> activeMatchPowerUps = new List<PowerUpData>();
    private bool isAvailable = true;

    private void Start()
    {
        LoadMatchPowerUps();
    }

    private void LoadMatchPowerUps()
    {
        activeMatchPowerUps.Clear();

        // Загружаем только те PowerUpData, которые игрок купил/выбрал в меню
        PowerUpData[] allPowerUps = Resources.LoadAll<PowerUpData>("PowerUps/");
        
        List<PowerUpType> selectedTypes = null;
        if (GameManager.Instance != null && GameManager.Instance.PowerUpDeck != null)
        {
            selectedTypes = GameManager.Instance.PowerUpDeck.GetSelectedPowerUps();
        }

        if (selectedTypes != null && selectedTypes.Count > 0)
        {
            foreach (var p in allPowerUps)
            {
                if (selectedTypes.Contains(p.type))
                {
                    activeMatchPowerUps.Add(p);
                }
            }
        }

        // --- Главное условие ---
        // Если игрок ничего не взял на заезд, выключаем коробку
        if (activeMatchPowerUps.Count == 0)
        {
            isAvailable = false;
            
            if (visualModel != null)
                visualModel.SetActive(false);

            // Если на самом объекте есть коллайдер, отключаем его,
            // чтобы машина не врезалась / не триггерила пустой заезд
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.enabled = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Дополнительная защита: если ничего не выбрано или коробка неактивна — игнорируем
        if (!isAvailable || activeMatchPowerUps.Count == 0) return;

        if (other.TryGetComponent<CarPowerUpInventory>(out var inventory))
        {
            PowerUpData randomPowerUp = activeMatchPowerUps[Random.Range(0, activeMatchPowerUps.Count)];

            if (inventory.TryCollectPowerUp(randomPowerUp))
            {
                StartCoroutine(HideAndRespawnCO());
            }
        }
    }

    private IEnumerator HideAndRespawnCO()
    {
        isAvailable = false;
        if (visualModel != null) visualModel.SetActive(false);

        yield return new WaitForSeconds(respawnTime);

        // Включаем обратно только если в заезде ВПРИНЦИПЕ есть доступные PowerUp'ы
        if (activeMatchPowerUps.Count > 0)
        {
            isAvailable = true;
            if (visualModel != null) visualModel.SetActive(true);
        }
    }
}