using UnityEngine;
using UnityEngine.Audio;


public class CarSFXHandler : MonoBehaviour
{
    [Header("Mixers")]
    public AudioMixer audioMixer;

    [Header("Audio sources")]
    public AudioSource tiresScreeachingAudioSource;
    public AudioSource engineAudioSource;
    public AudioSource carHitAudioSource;
    public AudioSource carJumpAudioSource;
    public AudioSource carLandingAudioSource;

    float desiredEnginePitch = 0.5f;
    float tireScreechPitch = 0.5f;

    TopDownCarController topDownCarController;

    private void Awake()
    {
        topDownCarController = GetComponentInParent<TopDownCarController>();
    }

    private void Update()
    {
        UpdateEngineSFX();
        UpdateTireScreechSFX();
    }

    private void UpdateEngineSFX()
    {
        float velocityMagnitude = topDownCarController.GetVelocityMagnitude();
        float desiredEngineVolume = velocityMagnitude * 0.05f;

        desiredEngineVolume = Mathf.Clamp(desiredEngineVolume, 0.2f, 1.0f);

        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, desiredEngineVolume, Time.deltaTime * 10);

        desiredEnginePitch = velocityMagnitude * 0.2f;
        desiredEnginePitch = Mathf.Clamp(desiredEnginePitch, 0.5f, 2.0f);
        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, desiredEnginePitch, Time.deltaTime * 1.5f);
    }

    private void UpdateTireScreechSFX()
    {
        if (topDownCarController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            if (isBraking)
            {
                tiresScreeachingAudioSource.volume = Mathf.Lerp(tiresScreeachingAudioSource.volume, 1.0f, Time.deltaTime * 10);
                tireScreechPitch = Mathf.Lerp(tireScreechPitch, 0.5f, Time.deltaTime);
            }
            else
            {
                tiresScreeachingAudioSource.volume = Mathf.Abs(lateralVelocity) * 0.05f;
                tireScreechPitch = Mathf.Abs(lateralVelocity) * 0.1f;
            }
        }

        else tiresScreeachingAudioSource.volume = Mathf.Lerp(tiresScreeachingAudioSource.volume, 0, Time.deltaTime * 10);
    }

    public void PlayJumpSFX()
    {
        carJumpAudioSource.Play();
    }

    public void PlayLandingSFX()
    {
        carLandingAudioSource.Play();
    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        float relativeVelocity = collision2D.relativeVelocity.magnitude;

        float volume = relativeVelocity * 0.1f;

        carHitAudioSource.pitch = Random.Range(0.95f, 1.0f);
        carHitAudioSource.volume = volume;

        if (!carHitAudioSource.isPlaying)
            carHitAudioSource.Play();
    }
}
