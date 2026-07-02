using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShake : MonoBehaviour
{
    [SerializeField] float defaultIntensity = 1f;

    CinemachineImpulseSource impulseSource;

    public static CameraShake Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake()
    {
        impulseSource.GenerateImpulse(defaultIntensity);
    }

    public void Shake(float intensity)
    {
        impulseSource.GenerateImpulse(intensity);
    }

    public void Shake(Vector3 velocity)
    {
        impulseSource.GenerateImpulse(velocity);
    }
}
