using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StaminaUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image fillImage; // Опционально: если вы используете Image with Filled type вместо Slider

    [Header("Animation Settings (DOTween)")]
    [SerializeField] private RectTransform uiBarContainer; // Контейнер всей полоски стамины, который будем масштабировать и трясти
    [SerializeField] private float scaleMultiplier = 1.2f;  // Во сколько раз увеличивать шкалу
    [SerializeField] private float scaleDuration = 0.2f;    // Длительность изменения масштаба

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.1f;    // Длительность одного импульса тряски
    [SerializeField] private float shakeStrength = 4f;      // Сила подрагивания (пиксели)
    [SerializeField] private int shakeVibrato = 10;         // Частота колебаний

    private Vector3 _originalScale;
    private Vector3 _originalAnchoredPosition;

    private Tween _scaleTween;
    private Tween _shakeTween;

    private bool _isEnlarged = false;

    private void Awake()
    {
        if (uiBarContainer == null)
        {
            uiBarContainer = GetComponent<RectTransform>();
        }

        _originalScale = uiBarContainer.localScale;
        _originalAnchoredPosition = uiBarContainer.anchoredPosition;
    }

    public void UpdateStamina(float current, float max)
    {
        float fillAmount = Mathf.Clamp01(current / max);

        if (staminaSlider != null)
        {
            staminaSlider.value = fillAmount;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = fillAmount;
        }
    }

    public void OnPowerUpStarted()
    {
        if (_isEnlarged) return;
        _isEnlarged = true;

        _scaleTween?.Kill();

        _scaleTween = uiBarContainer.DOScale(_originalScale * scaleMultiplier, scaleDuration)
            .SetEase(Ease.OutBack);

        StartShake();
    }

    public void OnPowerUpEnded()
    {
        if (!_isEnlarged) return;
        _isEnlarged = false;

        _shakeTween?.Kill();
        _scaleTween?.Kill();

        uiBarContainer.anchoredPosition = _originalAnchoredPosition;
        _scaleTween = uiBarContainer.DOScale(_originalScale, scaleDuration)
            .SetEase(Ease.InBack);
    }

    private void StartShake()
    {
        _shakeTween?.Kill();

        _shakeTween = uiBarContainer.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato)
            .SetLoops(-1, LoopType.Restart)
            .OnKill(() =>
            {
                uiBarContainer.anchoredPosition = _originalAnchoredPosition;
            });
    }

    private void OnDisable()
    {
        _scaleTween?.Kill();
        _shakeTween?.Kill();
    }
}