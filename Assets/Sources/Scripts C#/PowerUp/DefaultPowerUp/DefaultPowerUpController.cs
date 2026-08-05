using DG.Tweening;
using UnityEngine;

public class DefaultPowerUpController : MonoBehaviour
{
    [Header("Stamina Config")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float regenDelay = 1.5f;

    [Header("Saw PowerUp Config")]
    [SerializeField] private float staminaDrainRate = 25f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private GameObject sawRingGroup;

    [Header("UI Reference")]
    [SerializeField] private StaminaUI staminaUI;

    // Clean C# logic models
    public StaminaModel Stamina { get; private set; }
    public SawPowerUpModel SawPowerUp { get; private set; }

    private bool _wasActiveLastFrame = false;
    private Tween _sawRotationTween;

    private void Awake()
    {
        // INITIALIZE MODELS
        Stamina = new StaminaModel(maxStamina, staminaRegenRate, regenDelay);
        SawPowerUp = new SawPowerUpModel(Stamina, staminaDrainRate);

        if (sawRingGroup != null)
        {
            sawRingGroup.SetActive(false);

            float duration = 360f / Mathf.Max(1f, rotationSpeed);
            _sawRotationTween = sawRingGroup.transform
                .DORotate(new Vector3(0, 0, -360f), duration, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear)
                .Pause();
        }
    }

    private void Start()
    {
        if (staminaUI != null)
        {
            SubscribeStaminaUI();
        }
    }

    public void SetStaminaUI(StaminaUI ui)
    {
        UnsubscribeStaminaUI();
        staminaUI = ui;
        SubscribeStaminaUI();
    }

    private void SubscribeStaminaUI()
    {
        if (Stamina == null || staminaUI == null) return;

        Stamina.OnStaminaChanged -= HandleStaminaChanged;
        Stamina.OnStaminaChanged += HandleStaminaChanged;

        staminaUI.UpdateStamina(Stamina.CurrentStamina, Stamina.MaxStamina);
    }

    private void UnsubscribeStaminaUI()
    {
        if (Stamina != null)
        {
            Stamina.OnStaminaChanged -= HandleStaminaChanged;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeStaminaUI();
        _sawRotationTween?.Kill();
    }

    private void HandleStaminaChanged(float current, float max)
    {
        if (staminaUI != null)
        {
            staminaUI.UpdateStamina(current, max);
        }
    }

    #region External Input Controls

    public void ActivatePowerUp()
    {
        // Раскомментируйте для проверки в консоли:
        // Debug.Log("Activate PowerUp Called!");
        SawPowerUp?.TryActivate();
    }

    public void DeactivatePowerUp()
    {
        // Debug.Log("Deactivate PowerUp Called!");
        SawPowerUp?.Deactivate();
    }

    #endregion

    private void Update()
    {
        float dt = Time.deltaTime;

        Stamina.Tick(dt);

        bool isActive = SawPowerUp.Tick(dt);

        if (isActive != _wasActiveLastFrame)
        {
            if (sawRingGroup != null)
            {
                sawRingGroup.SetActive(isActive);

                if (isActive)
                {
                    _sawRotationTween?.Play();
                    staminaUI?.OnPowerUpStarted();
                }
                else
                {
                    _sawRotationTween?.Pause();
                    staminaUI?.OnPowerUpEnded();
                }
            }
        }

        _wasActiveLastFrame = isActive;
    }
}