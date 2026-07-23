using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;

    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    public event Action OnDamaged;
    public event Action OnDied;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    // Метод для спавнера (сброс здоровья при повторном использовании из пула)
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive || damage <= 0) return;

        CurrentHealth -= damage;
        OnDamaged?.Invoke();

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnDied?.Invoke();
        }
    }
}