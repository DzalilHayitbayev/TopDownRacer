using TMPro;
using UnityEngine;
using DG.Tweening;

public class MoneyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text moneyText;

    [Header("Formatting Settings")]
    [SerializeField] private string moneyFormat = "{0}$";

    [Header("Animation Settings")]
    [SerializeField] private bool enablePulseAnimation = true;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.2f;

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
        if (GameManager.Instance != null && GameManager.Instance.Wallet != null)
        {
            // Подписываемся на события кошелька
            GameManager.Instance.Wallet.OnMoneyChanged += OnMoneyChanged;

            // Сразу берем итоговый баланс из кошелька
            UpdateText(GameManager.Instance.Wallet.Balance, animate: false);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.Wallet != null)
        {
            GameManager.Instance.Wallet.OnMoneyChanged -= OnMoneyChanged;
        }

        pulseTween?.Kill();
        transform.localScale = originalScale;
    }

    private void OnMoneyChanged(int newAmount)
    {
        UpdateText(newAmount, animate: enablePulseAnimation);
    }

    private void UpdateText(int amount, bool animate)
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