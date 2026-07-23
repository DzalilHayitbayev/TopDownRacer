using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    public bool IsAlive => currentHealth > 0;
    public GameObject LastAttacker { get; private set; }

    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public event Action OnDied;
    public event Action OnDamaged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        LastAttacker = null;
    }

    public void TakeDamage(int damage, GameObject attacker = null)
    {
        if (!IsAlive) return;

        if (attacker != null)
        {
            LastAttacker = attacker;
        }

        currentHealth -= damage;
        OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            OnDied?.Invoke();
        }
    }
}