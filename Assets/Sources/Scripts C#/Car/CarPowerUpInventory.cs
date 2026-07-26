using System;
using System.Collections;
using UnityEngine;

public class CarPowerUpInventory : MonoBehaviour
{
    [Header("Current State")]
    public PowerUpData currentPowerUp;
    public bool isActive = false;
    public int currentAmmo = 0;
    public event Action<PowerUpData> OnPowerUpChanged;

    [Header("Spawn Points for Mine/Shoot")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform minePoint;

    [Header("Visual Indicators (Optional)")]
    [SerializeField] private GameObject shieldVisual;

    private Rigidbody2D carRigidbody;
    private Health health;
    private void Awake()
    {
        carRigidbody = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    public bool TryCollectPowerUp(PowerUpData data)
    {
        if (currentPowerUp != null || isActive) return false;

        currentPowerUp = data;
        currentAmmo = data.type == PowerUpType.Shoot ? data.ammo : 0;

        OnPowerUpChanged?.Invoke(currentPowerUp);

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
        OnPowerUpChanged?.Invoke(null);

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
        OnPowerUpChanged?.Invoke(null);

        health.IsUnderSheild(true);
        yield return new WaitForSeconds(data.duration);

        if (shieldVisual != null) shieldVisual.SetActive(false);
        health.IsUnderSheild(false);
        isActive = false;
    }
    private void ApplyRepair()
    {
        if (health != null && currentPowerUp != null)
        {
            health.Heal((int)currentPowerUp.value);
        }
    }
    private void ApplyShockWave()
    {
        float radius = currentPowerUp.value/4f;
        int damage = (int)currentPowerUp.value;
        float force = 12f;

        if (currentPowerUp.prefab != null)
        {
            Instantiate(currentPowerUp.prefab, transform.position, transform.rotation);
        }

        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in targets)
        {
            if (hit.gameObject == gameObject) continue;

            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage, gameObject);
            }

            if (hit.TryGetComponent<Rigidbody2D>(out var targetRb))
            {
                Vector2 direction = (hit.transform.position - transform.position).normalized;
                targetRb.AddForce(direction * force, ForceMode2D.Impulse);
            }
        }
    }

    private void ExecuteShoot()
    {
        if (currentAmmo <= 0) return;

        Transform spawnLocation = shootPoint != null ? shootPoint : transform;

        if (currentPowerUp.prefab != null)
        {
            GameObject projectileObj = Instantiate(currentPowerUp.prefab, spawnLocation.position, spawnLocation.rotation);

            if (projectileObj.TryGetComponent<Projectile2D>(out var projectile))
            {
                Vector2 shootDirection = spawnLocation.up;

                float slowFactor = 0.4f;

                projectile.Setup(
                    gameObject,
                    currentPowerUp.value, 
                    slowFactor,           
                    shootDirection,
                    carRigidbody.linearVelocity
                );
            }
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
            GameObject mineObj = Instantiate(currentPowerUp.prefab, spawnLocation.position, spawnLocation.rotation);

            if (mineObj.TryGetComponent<Mine2D>(out var mine))
            {
                mine.Setup(currentPowerUp.value);
            }
        }
    }
    private void ClearInventory()
    {
        currentPowerUp = null;
        OnPowerUpChanged?.Invoke(null);
        currentAmmo = 0;
    }

    #endregion
}