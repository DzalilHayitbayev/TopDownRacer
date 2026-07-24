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
        // «агружаем только те 3 PowerUpData, которые игрок выбрал в меню
        PowerUpData[] allPowerUps = Resources.LoadAll<PowerUpData>("PowerUps/");
        List<PowerUpType> selectedTypes = GameManager.Instance.PowerUpDeck.GetSelectedPowerUps();

        foreach (var p in allPowerUps)
        {
            if (selectedTypes.Contains(p.type))
            {
                activeMatchPowerUps.Add(p);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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

        isAvailable = true;
        if (visualModel != null) visualModel.SetActive(true);
    }
}