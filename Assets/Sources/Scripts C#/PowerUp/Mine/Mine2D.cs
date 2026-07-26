using UnityEngine;

public class Mine2D : MonoBehaviour
{
    private GameObject owner;
    private int damage;

    public void Setup(GameObject owner, float damage)
    {
        this.owner = owner;
        this.damage = (int)damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;

        // Наносим урон через IDamageable с передачей attacker
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, owner);
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