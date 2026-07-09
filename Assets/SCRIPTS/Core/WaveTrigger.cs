using UnityEngine;

public class WaveTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _waveSpawner;

    private void Start()
    {
        _waveSpawner.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _waveSpawner.SetActive(true);
        }
    }
}
