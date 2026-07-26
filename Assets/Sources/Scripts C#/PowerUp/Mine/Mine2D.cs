using UnityEngine;

public class Mine2D : MonoBehaviour
{
    private int damage;

    public void Setup(float damage)
    {
        this.damage = (int)damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        // Наносим урон через IDamageable с передачей attacker
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }

        // Вращение + замедление
        if (other.TryGetComponent<CarDebuffHandler>(out var debuff))
        {
            debuff.SpinOut(0.8f);
            debuff.ApplySlow(0.3f, 2f);
        }

        Destroy(gameObject);
    }
}