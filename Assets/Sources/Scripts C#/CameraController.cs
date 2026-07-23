using System.Collections;
using UnityEngine;
// using Cinemachine; // You can remove the old namespace entirely
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _cameraShakeTime;
    [SerializeField] private float _cameraShakeIntensity;

    private CinemachineCamera _cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin _cinemachineBasicMultiChannelPerlin;
    public static CameraController Instance;

    private bool _isCameraShake;

    void Start()
    {
        _cinemachineCamera = GetComponent<CinemachineCamera>();

        // In v3, extensions/components are grabbed directly via GetComponent
        _cinemachineBasicMultiChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();

        Instance = this;
    }

    public void StartCameraShake()
    {
        if (_isCameraShake || _cinemachineBasicMultiChannelPerlin == null) return;
        StartCoroutine(CameraShake());
    }

    private IEnumerator CameraShake()
    {
        _isCameraShake = true;

        // "m_AmplitudeGain" is now simplified to "AmplitudeGain"
        _cinemachineBasicMultiChannelPerlin.AmplitudeGain = _cameraShakeIntensity;

        yield return new WaitForSeconds(_cameraShakeTime);

        _cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0;
        _isCameraShake = false;
    }
}