using TMPro;
using UnityEngine;
using DG.Tweening;

public class PendingMoneyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text moneyText;

    [Header("Animation Settings")]
    [SerializeField] private float pulseScale = 1.25f;    
    [SerializeField] private float pulseDuration = 0.2f;   
    [SerializeField] private string moneyFormat = "+{0}$"; 

    private Vector3 originalScale;
    private Tween pulseTween;

    private void Awake()
    {
        if (moneyText == null)
            moneyText = GetComponent<TMP_Text>();

        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPendingMoneyChanged += OnMoneyChanged;

            UpdateMoneyText(GameManager.Instance.PendingMoney, animate: false);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPendingMoneyChanged -= OnMoneyChanged;
        }

        pulseTween?.Kill();
        transform.localScale = originalScale;
    }

    private void OnMoneyChanged(int newAmount)
    {
        UpdateMoneyText(newAmount, animate: true);
    }

    private void UpdateMoneyText(int amount, bool animate)
    {
        if (moneyText != null)
        {
            moneyText.text = string.Format(moneyFormat, amount);
        }

        if (animate)
        {
            PlayPulseAnimation();
        }
    }

    private void PlayPulseAnimation()
    {
        pulseTween?.Kill();
        transform.localScale = originalScale;

        pulseTween = transform.DOScale(originalScale * pulseScale, pulseDuration * 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                pulseTween = transform.DOScale(originalScale, pulseDuration * 0.5f)
                    .SetEase(Ease.InQuad);
            });
    }
}