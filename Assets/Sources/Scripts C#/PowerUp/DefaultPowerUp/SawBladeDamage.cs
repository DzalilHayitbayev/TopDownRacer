using UnityEngine;

public class SawBladeDamage : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int damagePerHit = 20;
    [SerializeField] private float damageInterval = 0.2f;
    [SerializeField] private LayerMask zombieLayer;
    [SerializeField] private float selfRotationSpeed = 1080f;

    private float _nextDamageTime;

    private void Update()
    {
        transform.Rotate(0f, 0f, selfRotationSpeed * Time.deltaTime);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Time.time < _nextDamageTime) return;

        if (((1 << collision.gameObject.layer) & zombieLayer) == 0) return;

        if (collision.TryGetComponent<Health>(out var health) || collision.transform.root.TryGetComponent<Health>(out health))
        {
            if (health.IsAlive)
            {
                health.TakeDamage(damagePerHit, transform.root.gameObject);
                _nextDamageTime = Time.time + damageInterval;
            }
        }
    }
}