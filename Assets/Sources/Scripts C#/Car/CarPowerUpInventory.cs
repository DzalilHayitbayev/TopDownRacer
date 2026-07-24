using System.Collections;
using UnityEngine;

public class CarPowerUpInventory : MonoBehaviour
{
    [Header("Current State")]
    public PowerUpData currentPowerUp;
    public bool isActive = false;
    public int currentAmmo = 0;

    [Header("Spawn Points for Mine/Shoot")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform minePoint;

    [Header("Visual Indicators (Optional)")]
    [SerializeField] private GameObject shieldVisual;

    private Rigidbody2D carRigidbody;

    private void Awake()
    {
        carRigidbody = GetComponent<Rigidbody2D>();
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    public bool TryCollectPowerUp(PowerUpData data)
    {
        if (currentPowerUp != null || isActive) return false;

        currentPowerUp = data;
        currentAmmo = data.type == PowerUpType.Shoot ? data.ammo : 0;

        Debug.Log($"[{gameObject.name}] Поднял PowerUp: {data.powerUpName}");
        return true;
    }

    public void ActivatePowerUp()
    {
        if (currentPowerUp == null) return;

        switch (currentPowerUp.type)
        {
            case PowerUpType.Boost:
                StartCoroutine(ApplyBoostCO());
                break;

            case PowerUpType.Shield:
                StartCoroutine(ApplyShieldCO());
                break;

            case PowerUpType.Repair:
                ApplyRepair();
                ClearInventory();
                break;

            case PowerUpType.ShockWave:
                ApplyShockWave();
                ClearInventory();
                break;

            case PowerUpType.Shoot:
                ExecuteShoot();
                break;

            case PowerUpType.Mine:
                ExecuteDropMine();
                ClearInventory();
                break;
        }
    }

    #region PowerUp Effects Logic

    private IEnumerator ApplyBoostCO()
    {
        isActive = true;
        PowerUpData data = currentPowerUp;
        currentPowerUp = null;

        float timer = 0f;
        while (timer < data.duration)
        {
            timer += Time.deltaTime;
            carRigidbody.AddForce(transform.up * data.value, ForceMode2D.Force);
            yield return null;
        }

        isActive = false;
    }

    private IEnumerator ApplyShieldCO()
    {
        isActive = true;
        if (shieldVisual != null) shieldVisual.SetActive(true);

        PowerUpData data = currentPowerUp;
        currentPowerUp = null;

        yield return new WaitForSeconds(data.duration);

        if (shieldVisual != null) shieldVisual.SetActive(false);
        isActive = false;
    }

    private void ApplyRepair()
    {
        Debug.Log($"[{gameObject.name}] Машина отремонтирована!");
    }

    private void ApplyShockWave()
    {
        if (currentPowerUp.prefab != null)
        {
            Instantiate(currentPowerUp.prefab, transform.position, transform.rotation);
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, currentPowerUp.value);
            foreach (var hit in colliders)
            {
                if (hit.gameObject != gameObject && hit.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.AddExplosionForce(1000f, transform.position, currentPowerUp.value, 1f, ForceMode.Impulse);
                }
            }
        }
    }

    private void ExecuteShoot()
    {
        if (currentAmmo <= 0) return;

        Transform spawnLocation = shootPoint != null ? shootPoint : transform;
        if (currentPowerUp.prefab != null)
        {
            Instantiate(currentPowerUp.prefab, spawnLocation.position, spawnLocation.rotation);
        }

        currentAmmo--;

        if (currentAmmo <= 0)
        {
            ClearInventory();
        }
    }

    private void ExecuteDropMine()
    {
        Transform spawnLocation = minePoint != null ? minePoint : transform;
        if (currentPowerUp.prefab != null)
        {
            Instantiate(currentPowerUp.prefab, spawnLocation.position, spawnLocation.rotation);
        }
    }

    private void ClearInventory()
    {
        currentPowerUp = null;
        currentAmmo = 0;
    }

    #endregion
}