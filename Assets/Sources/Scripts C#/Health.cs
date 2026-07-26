using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    public bool IsAlive => currentHealth > 0;
    public GameObject LastAttacker { get; private set; }

    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action OnDied;
    public event Action OnDamaged;

    public event Action<int, int> OnHealthChanged;

    private bool isInvulnerable = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        LastAttacker = null;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage, GameObject attacker = null)
    {
        if (!IsAlive || damage <= 0) return;

        if(isInvulnerable)
        {
            Debug.Log($"[{gameObject.name}] is invulnerable and cannot take damage.");
            return;
        }

        if (attacker != null)
        {
            LastAttacker = attacker;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

        OnDamaged?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
        {
            OnDied?.Invoke();
        }
    }

    public void IsUnderSheild(bool isUnderShield)
    {
        isInvulnerable = isUnderShield;
    }
}