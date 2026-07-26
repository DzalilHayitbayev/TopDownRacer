using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CarPowerUpInventory))]
public class EnemyPowerUpController : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Слои целей: машины игроков, другие боты, зомби")]
    [SerializeField] private LayerMask targetLayers;

    [Tooltip("Слои препятствий: стены, барьеры (для проверки нитро)")]
    [SerializeField] private LayerMask obstacleLayers;

    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float checkInterval = 0.2f; // Проверка 5 раз в секунду

    [Header("Combat Conditions")]
    [Tooltip("Максимальная дистанция для прицельного выстрела спереди")]
    [SerializeField] private float maxShootDistance = 12f;

    [Tooltip("Угол сектора атаки спереди (в градусах) для выстрела")]
    [SerializeField] private float minForwardAngleForShoot = 25f;

    [Tooltip("Максимальная дистанция до преследователя сзади для сброса мины")]
    [SerializeField] private float minBehindDistanceForMine = 4f;

    [Tooltip("Угол сектора защиты сзади (в градусах) для мины")]
    [SerializeField] private float minBehindAngleForMine = 45f;

    [Header("Support PowerUps")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float repairHealthThreshold = 0.6f; // Ремонт при HP < 60%
    [SerializeField] private float boostCheckDistance = 12f;     // Дистанция проверки чистой дороги

    private CarPowerUpInventory inventory;
    private Health health;

    // Оптимизированный буфер для поиска целей без GC Alloc
    private readonly Collider2D[] overlapResults = new Collider2D[16];

    private void Awake()
    {
        inventory = GetComponent<CarPowerUpInventory>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        StartCoroutine(AIEvaluationRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator AIEvaluationRoutine()
    {
        var wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            yield return wait;

            if (inventory.currentPowerUp == null || inventory.isActive)
                continue;

            EvaluateAndUsePowerUp();
        }
    }

    private void EvaluateAndUsePowerUp()
    {
        PowerUpType currentType = inventory.currentPowerUp.type;

        // 1. Вспомогательные способнойсти (Без сканирования врагов)
        if (currentType == PowerUpType.Repair)
        {
            if (health != null && health.CurrentHealth < health.MaxHealth * repairHealthThreshold)
            {
                inventory.ActivatePowerUp();
            }
            return;
        }

        if (currentType == PowerUpType.Boost)
        {
            // Проверяем свободна ли дорога спереди
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, boostCheckDistance, obstacleLayers);
            if (hit.collider == null)
            {
                inventory.ActivatePowerUp();
            }
            return;
        }

        // 2. Атакующие / Защитные способности (Поиск целей вокруг)
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, detectionRadius, overlapResults, targetLayers);
        if (count == 0) return;

        bool shouldUse = false;

        // Предварительный расчёт квадратов дистанций для оптимизации
        float maxShootDistSqr = maxShootDistance * maxShootDistance;
        float mineDistSqr = minBehindDistanceForMine * minBehindDistanceForMine;

        for (int i = 0; i < count; i++)
        {
            Collider2D target = overlapResults[i];

            // Пропускаем сам бот и его дочерние объекты
            if (target == null || target.transform.IsChildOf(transform)) continue;

            Vector2 dirToTarget = target.transform.position - transform.position;
            float sqrDistance = dirToTarget.sqrMagnitude;

            switch (currentType)
            {
                case PowerUpType.Shoot:
                    // Проверяем, что цель СПРЕДИ в пределах угла и не слишком далеко
                    if (sqrDistance <= maxShootDistSqr)
                    {
                        float angle = Vector2.Angle(transform.up, dirToTarget);
                        if (angle <= minForwardAngleForShoot)
                        {
                            shouldUse = true;
                        }
                    }
                    break;

                case PowerUpType.Mine:
                    // Проверяем, что цель СЗАДИ непосредственного бота
                    if (sqrDistance <= mineDistSqr)
                    {
                        float behindAngle = Vector2.Angle(-transform.up, dirToTarget);
                        if (behindAngle <= minBehindAngleForMine)
                        {
                            shouldUse = true;
                        }
                    }
                    break;

                case PowerUpType.ShockWave:
                    // Активируется, если цель в радиусе действия волны
                    float waveRadius = inventory.currentPowerUp.value;
                    if (sqrDistance <= waveRadius * waveRadius)
                    {
                        shouldUse = true;
                    }
                    break;

                case PowerUpType.Shield:
                    // Щит прожимаем при опасном сближении с любой целью (< 5 м)
                    if (sqrDistance <= 25f)
                    {
                        shouldUse = true;
                    }
                    break;
            }

            if (shouldUse) break;
        }

        if (shouldUse)
        {
            inventory.ActivatePowerUp();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 1. Общий радиус сканирования
        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 2. Сектор для Shoot (Зелёный)
        Gizmos.color = Color.green;
        Vector3 forwardLeft = Quaternion.Euler(0, 0, minForwardAngleForShoot) * transform.up;
        Vector3 forwardRight = Quaternion.Euler(0, 0, -minForwardAngleForShoot) * transform.up;
        Gizmos.DrawRay(transform.position, forwardLeft * maxShootDistance);
        Gizmos.DrawRay(transform.position, forwardRight * maxShootDistance);

        // 3. Сектор для Mine (Красный)
        Gizmos.color = Color.red;
        Vector3 backLeft = Quaternion.Euler(0, 0, minBehindAngleForMine) * -transform.up;
        Vector3 backRight = Quaternion.Euler(0, 0, -minBehindAngleForMine) * -transform.up;
        Gizmos.DrawRay(transform.position, backLeft * minBehindDistanceForMine);
        Gizmos.DrawRay(transform.position, backRight * minBehindDistanceForMine);

        // 4. Луч проверки нитро (Голубой)
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.up * boostCheckDistance);
    }
#endif
}