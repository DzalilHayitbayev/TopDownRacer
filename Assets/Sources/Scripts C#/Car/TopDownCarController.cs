using System.Collections;
using UnityEngine;

public class TopDownCarController : MonoBehaviour
{
    [Header("Car settings")]
    public float driftFactor = 0.95f;
    public float accelerationFactor = 30.0f;
    public float turnFactor = 3.5f;
    public float maxSpeed = 20f;

    [Header("Sprites")]
    public SpriteRenderer carSpriteRenderer;
    public SpriteRenderer carShadowRenderer;

    [Header("Jumping")]
    public AnimationCurve jumpCurve;
    public ParticleSystem landingParticleSystem;

    float accelerationInput = 0;
    float steeringInput = 0;

    float rotationAngle = 0;

    float velocityVsUp = 0;

    bool isJumping = false;

    Rigidbody2D carRigidbody2D;
    Collider2D carCollider2D;
    CarSFXHandler carSFXHandler;

    private void Awake()
    {
        carRigidbody2D = GetComponent<Rigidbody2D>();
        carCollider2D = GetComponentInChildren<Collider2D>();
        carSFXHandler = GetComponent<CarSFXHandler>();
    }

    private void FixedUpdate()
    {
        if(GameManager.Instance.GetGameState() == GameStates.countDown)
            return;

        ApplyEngineForce();

        KillOrthogonalVelocity();

        ApplySteering();
    }

    private void ApplyEngineForce()
    {
        if (isJumping && accelerationInput < 0)
            accelerationInput = 0;

        if (accelerationInput == 0)
            carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, 3.0f, Time.deltaTime * 3);
        else
            carRigidbody2D.linearDamping = 0;


        velocityVsUp = Vector2.Dot(transform.up, carRigidbody2D.linearVelocity);

        if (velocityVsUp > maxSpeed && accelerationInput > 0)
            return;

        if (velocityVsUp < -maxSpeed * 0.5f && accelerationInput < 0)
            return;

        if (carRigidbody2D.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput > 0 && !isJumping)
            return;

        Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;

        carRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);
    }

    private void ApplySteering()
    {
        float minSpeedBeforeAllowTurningFactor = (carRigidbody2D.linearVelocity.magnitude / 8);
        minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);


        rotationAngle -= steeringInput * turnFactor * minSpeedBeforeAllowTurningFactor;

        carRigidbody2D.MoveRotation(rotationAngle);
    }
    void KillOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidbody2D.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidbody2D.linearVelocity, transform.right);

        carRigidbody2D.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }

    float GetLateralVelocity()
    {
        return Vector2.Dot(transform.right, carRigidbody2D.linearVelocity);
    }

    public bool IsTireScreeching(out float lateralVelocity, out bool isBraking)
    {
        lateralVelocity = GetLateralVelocity();
        isBraking = false;

        if(isJumping) 
            return false;

        if (accelerationInput < 0 && velocityVsUp > 0)
        {
            isBraking = true;
            return true;
        }

        if (Mathf.Abs(GetLateralVelocity()) > 4.0f)
            return true;

        return false;

    }

    public void SetInputVector(Vector2 inputVector)
    {
        steeringInput = inputVector.x;
        accelerationInput = inputVector.y;
    }
    public float GetVelocityMagnitude()
    {
        return carRigidbody2D.linearVelocity.magnitude;
    }

    public void Jump(float jumpHeightScale, float jumpPushScale)
    {
        if (!isJumping)
            StartCoroutine(JumpCO(jumpHeightScale, jumpPushScale));
    }

    private IEnumerator JumpCO(float jumpHeightScale, float jumpPushScale)
    {
        isJumping = true;

        float jumpStartTime = Time.time;
        float jumpDuration = carRigidbody2D.linearVelocity.magnitude * 0.05f;

        jumpHeightScale = jumpHeightScale * carRigidbody2D.linearVelocity.magnitude * 0.05f;
        jumpHeightScale = Mathf.Clamp(jumpHeightScale, 0.0f, 1.0f);


        carCollider2D.enabled = false;

        carSFXHandler.PlayJumpSFX();

        carSpriteRenderer.sortingLayerName = "Flying";
        carShadowRenderer.sortingLayerName = "Flying";

        carRigidbody2D.AddForce(carRigidbody2D.linearVelocity.normalized * jumpPushScale * 10, ForceMode2D.Impulse);

        while (isJumping)
        {
            float jumpCompletedPercentage = (Time.time - jumpStartTime) / jumpDuration;
            jumpCompletedPercentage = Mathf.Clamp01(jumpCompletedPercentage);

            carSpriteRenderer.transform.localScale = Vector3.one + Vector3.one * jumpCurve.Evaluate(jumpCompletedPercentage) * jumpHeightScale;

            carShadowRenderer.transform.localScale = carSpriteRenderer.transform.localScale * 0.75f;

            carShadowRenderer.transform.localPosition = new Vector3(1, -1, 0.0f) * 3 * jumpCurve.Evaluate(jumpCompletedPercentage) * jumpHeightScale;

            if (jumpCompletedPercentage == 1.0f)
                break;

            yield return null;
        }

        if (Physics2D.OverlapCircle(transform.position, 1.5f))
        {
            
            isJumping = false;

            Jump(0.2f, 0.6f);
        }
        else
        {
            carSpriteRenderer.transform.localScale = Vector3.one;

            carShadowRenderer.transform.localPosition = Vector3.zero;
            carShadowRenderer.transform.localScale = Vector3.one;

            carCollider2D.enabled = true;

            carSpriteRenderer.sortingLayerName = "Default";
            carShadowRenderer.sortingLayerName = "Default";

            if(jumpHeightScale > 0.2f)
            {
                landingParticleSystem.Play();

                carSFXHandler.PlayLandingSFX();
            }

            isJumping = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.CompareTag("Jump"))
        {
            JumpData jumpData = collider2D.GetComponent<JumpData>();
            Jump(jumpData.jumpHeightScale, jumpData.jumpPushScale);
        }
    }

    
}
