using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile2D : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;
    private GameObject owner;
    private int damage;
    private float slowStrength;
    private bool isInitialized = false; // Флаг безопасности от ранних триггеров

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <param name="owner">Кто выстрелил</param>
    /// <param name="damage">Урон</param>
    /// <param name="slowStrength">Сила замедления</param>
    /// <param name="shootDirection">Вектор направления выстрела (Vector2)</param>
    /// <param name="ownerVelocity">Текущая скорость машины</param>
    public void Setup(GameObject owner, float damage, float slowStrength, Vector2 shootDirection, Vector2 ownerVelocity)
    {
        this.owner = owner;
        this.damage = (int)damage;
        this.slowStrength = slowStrength;

        IgnoreOwnerColliders(owner);

        Vector2 normalizedDirection = shootDirection.normalized;
        Vector2 finalVelocity = (normalizedDirection * speed) + ownerVelocity;

        rb.linearVelocity = finalVelocity;

        float angle = Mathf.Atan2(finalVelocity.y, finalVelocity.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;

        isInitialized = true; // Теперь снаряд готов регистрировать попадания

        StartCoroutine(DestroyAfterDelayCO(lifetime));
    }

    private void IgnoreOwnerColliders(GameObject ownerObj)
    {
        if (ownerObj == null) return;

        Collider2D bulletCollider = GetComponent<Collider2D>();
        Collider2D[] ownerColliders = ownerObj.GetComponentsInChildren<Collider2D>();

        foreach (var col in ownerColliders)
        {
            if (col != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, col, true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Игнорируем столкновения ДО инициализации
        if (!isInitialized) return;

        // 2. Игнорируем стрелка и его дочерние объекты
        if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        // Достаем IDamageable один раз
        bool hasDamageable = other.TryGetComponent<IDamageable>(out var damageable);

        // 3. Игнорируем триггеры без здоровья (чекпоинты, повер-апы и т.д.)
        if (other.isTrigger && !hasDamageable)
        {
            return;
        }

        // --- Нанесение урона ---
        if (hasDamageable)
        {
            damageable.TakeDamage(damage, owner);
        }

        if (other.TryGetComponent<CarDebuffHandler>(out var debuff))
        {
            debuff.ApplySlow(slowStrength, 1.5f);
        }

        // Уничтожаем снаряд при попадании
        Destroy(gameObject);
    }

    private IEnumerator DestroyAfterDelayCO(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}