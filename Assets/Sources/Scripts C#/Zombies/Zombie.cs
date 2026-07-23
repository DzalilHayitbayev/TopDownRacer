using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public class Zombie : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float detectionRadius = 50f;
    [SerializeField] private float targetSearchInterval = 0.5f;

    [Header("Separation (Anti-Crowding)")]
    [SerializeField] private LayerMask zombieLayer;           // Слой с зомби
    [SerializeField] private float separationRadius = 1.2f;    // Радиус личного пространства
    [SerializeField] private float separationWeight = 1.5f;    // Сила расталкивания

    [Header("Targeting")]
    [SerializeField] private LayerMask vehicleLayer;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private Animator animator;
    [SerializeField] private Health health;

    private Transform currentTargetVehicle;
    private IDamageable targetDamageable;
    private float lastAttackTime;
    private float lastSearchTime;
    private bool isMoving;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int HitHash = Animator.StringToHash("Hit");

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (health == null) health = GetComponent<Health>();
        if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();

        if (health != null)
        {
            health.ResetHealth();
            health.OnDied += HandleDeath;
            health.OnDamaged += HandleHit;
        }

        lastSearchTime = 0f;
        isMoving = false;

        FindNearestVehicle();
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= HandleDeath;
            health.OnDamaged -= HandleHit;
        }
    }

    private void Update()
    {
        if (health == null || !health.IsAlive) return;

        if (Time.time >= lastSearchTime + targetSearchInterval)
        {
            lastSearchTime = Time.time;
            FindNearestVehicle();
        }

        if (currentTargetVehicle == null)
        {
            isMoving = false;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentTargetVehicle.position);

        if (distanceToTarget > attackRange)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackCurrentTarget();
            }
        }
    }

    private void FixedUpdate()
    {
        if (health == null || !health.IsAlive || currentTargetVehicle == null) return;

        if (isMoving)
        {
            MoveAndRotateTowardsTarget();
        }
    }

    private void MoveAndRotateTowardsTarget()
    {
        // 1. Основной вектор направления к машине
        Vector2 directionToTarget = ((Vector2)currentTargetVehicle.position - rb2d.position).normalized;

        // 2. Вектор расталкивания от других зомби
        Vector2 separationVector = ComputeSeparationForce();

        // 3. Итоговое направление движения (сочетание движения к цели + отталкивание)
        Vector2 finalDirection = (directionToTarget + separationVector * separationWeight).normalized;

        // Поворачиваем зомби по итоговому направлению
        float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
        rb2d.rotation = angle;

        // Перемещаем
        Vector2 nextPosition = rb2d.position + finalDirection * (moveSpeed * Time.fixedDeltaTime);
        rb2d.MovePosition(nextPosition);
    }

    /// <summary>
    /// Вычисляет вектор отталкивания от соседних зомби
    /// </summary>
    private Vector2 ComputeSeparationForce()
    {
        // Если слой зомби не задан, ищем среди слоя своего коллайдера
        LayerMask maskToUse = zombieLayer.value != 0 ? zombieLayer : (LayerMask)(1 << gameObject.layer);

        Collider2D[] nearbyZombies = Physics2D.OverlapCircleAll(rb2d.position, separationRadius, maskToUse);
        Vector2 separationForce = Vector2.zero;
        int neighborCount = 0;

        foreach (var col in nearbyZombies)
        {
            // Пропускаем самого себя
            if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;

            Vector2 pushAwayVector = rb2d.position - (Vector2)col.transform.position;
            float distance = pushAwayVector.magnitude;

            if (distance > 0.001f)
            {
                // Чем ближе сосед, тем сильнее отталкиваемся
                separationForce += pushAwayVector.normalized / distance;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            separationForce /= neighborCount;
        }

        return separationForce;
    }

    private void FindNearestVehicle()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, vehicleLayer);

        if (hitColliders.Length == 0)
        {
            currentTargetVehicle = null;
            targetDamageable = null;
            return;
        }

        float minDistance = float.MaxValue;
        Transform nearestTransform = null;
        IDamageable nearestDamageable = null;

        foreach (var col in hitColliders)
        {
            IDamageable damTarget = col.GetComponentInParent<IDamageable>();

            if (damTarget != null && damTarget.IsAlive)
            {
                Transform targetTransform = col.transform.root;
                float dist = Vector2.Distance(transform.position, targetTransform.position);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestTransform = targetTransform;
                    nearestDamageable = damTarget;
                }
            }
        }

        currentTargetVehicle = nearestTransform;
        targetDamageable = nearestDamageable;
    }

    private void AttackCurrentTarget()
    {
        lastAttackTime = Time.time;

        if (animator != null) animator.SetTrigger(AttackHash);

        if (targetDamageable != null && targetDamageable.IsAlive)
        {
            targetDamageable.TakeDamage(attackDamage, gameObject);
        }
    }

    private void HandleHit()
    {
        if (animator != null && health.IsAlive)
        {
            animator.SetTrigger(HitHash);
        }
    }

    private void HandleDeath()
    {
        isMoving = false;
        if (animator != null) animator.SetTrigger(DieHash);

        if (health != null && health.LastAttacker != null)
        {
            if (health.LastAttacker.CompareTag("Player") || health.LastAttacker.transform.root.CompareTag("Player"))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddZombieKillReward();
                }
            }
        }

        StartCoroutine(DisableAfterDelay(1.5f));
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Зелёная окружность — зона расталкивания с сородичами
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}