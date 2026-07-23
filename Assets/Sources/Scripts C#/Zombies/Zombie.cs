using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public class Zombie : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackRange = 1.2f;          // Дистанция атаки
    [SerializeField] private float detectionRadius = 50f;       // Радиус агро (увеличили для всей карты)
    [SerializeField] private float targetSearchInterval = 0.5f;

    [Header("Targeting")]
    [SerializeField] private LayerMask vehicleLayer; // Слой "Vehicle" (машины и игрок)

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
        health.ResetHealth();
        health.OnDied += HandleDeath;
        health.OnDamaged += HandleHit;

        FindNearestVehicle();
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDeath;
        health.OnDamaged -= HandleHit;
    }

    private void Update()
    {
        if (!health.IsAlive) return;

        // Периодический поиск ближайшей машины/игрока
        if (Time.time >= lastSearchTime + targetSearchInterval)
        {
            lastSearchTime = Time.time;
            FindNearestVehicle();
        }

        if (currentTargetVehicle == null)
        {
            isMoving = false;
            //SetMovingAnimation(false);
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentTargetVehicle.position);

        if (distanceToTarget > attackRange)
        {
            isMoving = true;
            //SetMovingAnimation(true);
        }
        else
        {
            isMoving = false;
            //// SetMovingAnimation(false);

            // Атака по таймеру
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackCurrentTarget();
            }
        }
    }
    private void FixedUpdate()
    {
        if (!health.IsAlive || currentTargetVehicle == null) return;

        if (isMoving)
        {
            MoveAndRotateTowardsTarget();
        }
    }

    private void MoveAndRotateTowardsTarget()
    {
        Vector2 direction = ((Vector2)currentTargetVehicle.position - rb2d.position).normalized;

        // 1. Поворот
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb2d.rotation = angle;

        // 2. Движение для Kinematic Rigidbody2D
        Vector2 nextPosition = rb2d.position + direction * (moveSpeed * Time.fixedDeltaTime);
        rb2d.MovePosition(nextPosition);
    }
    private void FindNearestVehicle()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, vehicleLayer);

        float minDistance = float.MaxValue;
        Transform nearest = null;
        IDamageable nearestDamageable = null;

        foreach (var col in hitColliders)
        {
            IDamageable damTarget = col.GetComponentInParent<IDamageable>();

            if (damTarget != null && damTarget.IsAlive)
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = col.transform;
                    nearestDamageable = damTarget;
                }
            }
        }

        currentTargetVehicle = nearest;
        targetDamageable = nearestDamageable;
    }

    private void AttackCurrentTarget()
    {
        lastAttackTime = Time.time;

        if (animator != null) animator.SetTrigger(AttackHash);

        if (targetDamageable != null && targetDamageable.IsAlive)
        {
            targetDamageable.TakeDamage(attackDamage);
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
        rb2d.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetTrigger(DieHash);

        StartCoroutine(DisableAfterDelay(1.5f));
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false); // Для Object Pooling
    }

    //private void SetMovingAnimation(bool moving)
    //{
    //    if (animator != null) animator.SetBool(IsMovingHash, moving);
    //}
}