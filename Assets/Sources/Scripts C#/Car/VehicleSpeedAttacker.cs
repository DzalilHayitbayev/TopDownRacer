using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicleSpeedAttacker : MonoBehaviour, ISpeedAttacker
{
    [Header("Damage Settings")]
    [SerializeField] private float speedThreshold = 5f;  // Мин. скорость для нанесения урона
    [SerializeField] private int baseRamDamage = 50;     // Базовый урон от тарана
    [SerializeField] private bool scaleDamageWithSpeed = true;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb2d;
    //[SerializeField] private Animator vehicleAnimator;

    private static readonly int RamImpactHash = Animator.StringToHash("RamImpact");

    // В 2D скорость берем из rb2d.velocity.magnitude
    public float CurrentSpeed => rb2d != null ? rb2d.linearVelocity.magnitude : 0f;
    public bool CanDealSpeedDamage => CurrentSpeed >= speedThreshold;

    private void Awake()
    {
        if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();
    }

    // ВАЖНО: В 2D используем OnCollisionEnter2D и Collision2D
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!CanDealSpeedDamage) return;

        // Ищем IDamageable на объекте ИЛИ его родителе
        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();

        if (damageable != null && damageable.IsAlive)
        {
            int calculatedDamage = baseRamDamage;

            if (scaleDamageWithSpeed)
            {
                calculatedDamage = Mathf.RoundToInt(baseRamDamage * (CurrentSpeed / speedThreshold));
            }

            damageable.TakeDamage(calculatedDamage);

           /* if (vehicleAnimator != null)
            {
                vehicleAnimator.SetTrigger(RamImpactHash);
            }
           */
        }
    }
}
