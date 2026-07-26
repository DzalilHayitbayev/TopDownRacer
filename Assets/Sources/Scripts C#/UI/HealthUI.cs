using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Target Target Reference")]
    [SerializeField] private Health targetHealth;

    [Header("UI Components (Optional)")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private void OnEnable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += UpdateHealthUI;

            UpdateHealthUI(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }

    private void OnDisable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= UpdateHealthUI;
        }
    }
    public void SetTarget(Health newHealth)
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= UpdateHealthUI;
        }

        targetHealth = newHealth;

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += UpdateHealthUI;
            UpdateHealthUI(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }

    private void UpdateHealthUI(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            healthText.text = $"{current} / {max}";
        }
    }
}