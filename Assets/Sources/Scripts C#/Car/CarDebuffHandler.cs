using System.Collections;
using UnityEngine;

public class CarDebuffHandler : MonoBehaviour
{
    private Rigidbody2D rb;
    private Coroutine slowCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Замедляет объект на duration секунд, временно увеличивая сопротивление
    /// </summary>
    /// <param name="slowMultiplier">Множитель силы замедления (например, 0.5f = срезать 50% текущей скорости и добавить drag)</param>
    /// <param name="duration">Длительность эффекта в секундах</param>
    public void ApplySlow(float slowMultiplier, float duration)
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        slowCoroutine = StartCoroutine(SlowCO(slowMultiplier, duration));
    }

    private IEnumerator SlowCO(float slowMultiplier, float duration)
    {
        // 1. Ограничиваем множитель в диапазоне [0.0, 1.0], чтобы случайные значения value не ломали физику
        float clampedSlow = Mathf.Clamp01(slowMultiplier);

        // 2. Разово притормаживаем текущую скорость
        rb.linearVelocity *= (1f - clampedSlow);

        // 3. Сохраняем базовое сопротивление и временно повышаем его
        float originalDamping = rb.linearDamping;
        rb.linearDamping += 3f;

        yield return new WaitForSeconds(duration);

        // 4. Возвращаем сопротивление в исходное состояние
        rb.linearDamping = originalDamping;
        slowCoroutine = null;
    }

    public void SpinOut(float duration)
    {
        StartCoroutine(SpinCO(duration));
    }

    private IEnumerator SpinCO(float duration)
    {
        float timer = 0f;
        float totalRotation = 360f;
        float speed = totalRotation / duration;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.Rotate(0, 0, speed * Time.deltaTime);
            yield return null;
        }
    }
}