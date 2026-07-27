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

    [Header("Attack Point (AoE)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackAreaRadius = 1.0f;

    [Header("Impact Settings (Crushing)")]
    [SerializeField] private bool isHeavyZombie = false;
    [SerializeField] private float minSpeedToCrush = 2.0f;
    [SerializeField] private float deathImpulseForce = 8f;
    [SerializeField] private int heavyRamDamage = 25;

    [Header("Separation (Anti-Crowding)")]
    [SerializeField] private LayerMask zombieLayer;
    [SerializeField] private float separationRadius = 1.2f;
    [SerializeField] private float separationWeight = 1.5f;

    [Header("Targeting")]
    [SerializeField] private LayerMask vehicleLayer;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private Animator animator;
    [SerializeField] private Health health;
    [SerializeField] private Collider2D zombieCollider;

    private Transform currentTargetVehicle;
    private IDamageable targetDamageable;
    private float lastAttackTime;
    private float lastSearchTime;
    private bool isMoving;
    private bool isDying = false;

    // Static Buffers for Physics Queries (Zero Garbage Allocation)
    private static readonly Collider2D[] SeparationBuffer = new Collider2D[16];
    private static readonly Collider2D[] TargetBuffer = new Collider2D[16];
    private static readonly Collider2D[] AttackBuffer = new Collider2D[16];

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int HitHash = Animator.StringToHash("Hit");

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();
        if (zombieCollider == null) zombieCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (health == null) health = GetComponent<Health>();
        if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();
        if (zombieCollider == null) zombieCollider = GetComponent<Collider2D>();

        isDying = false;

        if (zombieCollider != null)
        {
            zombieCollider.isTrigger = false;
            zombieCollider.enabled = true;
        }

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
        if (health == null || !health.IsAlive || isDying) return;

        if (Time.time >= lastSearchTime + targetSearchInterval)
        {
            lastSearchTime = Time.time;
            FindNearestVehicle();
        }

        if (currentTargetVehicle == null)
        {
            SetMovingState(false);
            return;
        }

        // Optimization: Use sqrMagnitude to avoid expensive square root calculation
        float sqrDistanceToTarget = (currentTargetVehicle.position - transform.position).sqrMagnitude;
        float sqrAttackRange = attackRange * attackRange;

        if (sqrDistanceToTarget > sqrAttackRange)
        {
            SetMovingState(true);
        }
        else
        {
            SetMovingState(false);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackCurrentTarget();
            }
        }
    }

    private void SetMovingState(bool moving)
    {
        isMoving = moving;
        if (animator != null)
        {
            animator.SetBool(IsMovingHash, isMoving);
        }
    }

    private void FixedUpdate()
    {
        if (health == null || !health.IsAlive || isDying || currentTargetVehicle == null) return;

        if (isMoving)
        {
            MoveAndRotateTowardsTarget();
        }
    }

    private void MoveAndRotateTowardsTarget()
    {
        Vector2 directionToTarget = ((Vector2)currentTargetVehicle.position - rb2d.position).normalized;
        Vector2 separationVector = ComputeSeparationForce();
        Vector2 finalDirection = (directionToTarget + separationVector * separationWeight).normalized;

        float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
        rb2d.rotation = angle;

        Vector2 nextPosition = rb2d.position + finalDirection * (moveSpeed * Time.fixedDeltaTime);
        rb2d.MovePosition(nextPosition);
    }

    private Vector2 ComputeSeparationForce()
    {
        LayerMask maskToUse = zombieLayer.value != 0 ? zombieLayer : (LayerMask)(1 << gameObject.layer);

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(maskToUse);
        filter.useLayerMask = true;

        int hitCount = Physics2D.OverlapCircle(rb2d.position, separationRadius, filter, SeparationBuffer);
        Vector2 separationForce = Vector2.zero;
        int neighborCount = 0;

        for (int i = 0; i < hitCount; i++)
        {
            var col = SeparationBuffer[i];
            if (col == null || col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;

            Vector2 pushAwayVector = rb2d.position - (Vector2)col.transform.position;
            float sqrDistance = pushAwayVector.sqrMagnitude;

            if (sqrDistance > 0.000001f)
            {
                float distance = Mathf.Sqrt(sqrDistance);
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
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(vehicleLayer);
        filter.useLayerMask = true;

        int hitCount = Physics2D.OverlapCircle(transform.position, detectionRadius, filter, TargetBuffer);

        if (hitCount == 0)
        {
            currentTargetVehicle = null;
            targetDamageable = null;
            return;
        }

        float minSqrDistance = float.MaxValue;
        Transform nearestTransform = null;
        IDamageable nearestDamageable = null;

        for (int i = 0; i < hitCount; i++)
        {
            var col = TargetBuffer[i];
            if (col == null) continue;

            IDamageable damTarget = col.GetComponentInParent<IDamageable>();

            if (damTarget != null && damTarget.IsAlive)
            {
                Transform targetTransform = col.transform.root;
                float sqrDist = (transform.position - targetTransform.position).sqrMagnitude;

                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
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

        Vector2 pointToCheck = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(vehicleLayer);
        filter.useLayerMask = true;

        int hitCount = Physics2D.OverlapCircle(pointToCheck, attackAreaRadius, filter, AttackBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            var col = AttackBuffer[i];
            if (col == null) continue;

            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(attackDamage, gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying || health == null || !health.IsAlive) return;

        if (((1 << collision.gameObject.layer) & vehicleLayer) != 0 || collision.transform.root.CompareTag("Player"))
        {
            Rigidbody2D vehicleRb = collision.rigidbody;
            float speed = vehicleRb != null ? vehicleRb.linearVelocity.magnitude : 0f;

            if (speed >= minSpeedToCrush)
            {
                if (!isHeavyZombie)
                {
                    if (zombieCollider != null)
                    {
                        zombieCollider.isTrigger = true;
                    }

                    health.TakeDamage(health.CurrentHealth, collision.gameObject);
                }
                else
                {
                    if (collision.gameObject.TryGetComponent<IDamageable>(out var vehicleDamageable))
                    {
                        vehicleDamageable.TakeDamage(heavyRamDamage, gameObject);
                    }
                    else if (collision.transform.root.TryGetComponent<IDamageable>(out var rootDamageable))
                    {
                        rootDamageable.TakeDamage(heavyRamDamage, gameObject);
                    }

                    int crushDamage = Mathf.RoundToInt(speed * 10f);
                    health.TakeDamage(crushDamage, collision.gameObject);
                }
            }
        }
    }

    private void HandleHit()
    {
        if (animator != null && health.IsAlive && !isDying)
        {
            animator.SetTrigger(HitHash);
        }
    }

    private void HandleDeath()
    {
        if (isDying) return;
        isDying = true;
        SetMovingState(false);

        if (animator != null) animator.SetTrigger(DieHash);

        if (zombieCollider != null)
        {
            zombieCollider.isTrigger = true;
        }

        Vector2 pushDirection = Vector2.zero;

        if (health != null && health.LastAttacker != null)
        {
            if (health.LastAttacker.TryGetComponent<Rigidbody2D>(out var attackerRb))
            {
                pushDirection = attackerRb.linearVelocity.normalized;
                rb2d.AddForce(pushDirection * deathImpulseForce, ForceMode2D.Impulse);
            }

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

        if (attackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackPoint.position, attackAreaRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}