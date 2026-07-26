using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicleSpeedAttacker : MonoBehaviour, ISpeedAttacker
{
    [Header("Damage Settings")]
    [SerializeField] private float minImpactSpeed = 6f;      // Мин. относительная скорость для урона
    [SerializeField] private int baseRamDamage = 20;         // Адекватный базовый урон
    [SerializeField] private float damageMultiplier = 3f;    // Множитель урона за каждые единицы скорости выше порога
    [SerializeField] private float hitCooldown = 0.5f;       // Пауза между повторными ударами по той же машине

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb2d;


    private Dictionary<GameObject, float> lastHitTimes = new Dictionary<GameObject, float>();

    public float CurrentSpeed => rb2d != null ? rb2d.linearVelocity.magnitude : 0f;
    public bool CanDealSpeedDamage => CurrentSpeed >= minImpactSpeed;

    private void Awake()
    {
        if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < minImpactSpeed) return;

        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();
        if (damageable == null || !damageable.IsAlive) return;

        GameObject targetObj = (damageable as Component)?.gameObject ?? collision.gameObject;

        if (lastHitTimes.TryGetValue(targetObj, out float lastTime))
        {
            if (Time.time - lastTime < hitCooldown) return;
        }

        
        Vector2 myVelocity = rb2d.linearVelocity;
        Vector2 directionToTarget = (collision.transform.position - transform.position).normalized;

        float dot = Vector2.Dot(myVelocity.normalized, directionToTarget);

        if (dot < 0.3f)
        {
            return;
        }

        float excessSpeed = impactSpeed - minImpactSpeed;
        int calculatedDamage = baseRamDamage + Mathf.RoundToInt(excessSpeed * damageMultiplier);

        damageable.TakeDamage(calculatedDamage, gameObject);
        lastHitTimes[targetObj] = Time.time;

        Debug.Log($"[{gameObject.name}] Таран по [{targetObj.name}]! Сила удара: {impactSpeed:F1}, Урон: {calculatedDamage}");
    }
}