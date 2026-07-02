using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] float gizmoRadius = 1f;
    [SerializeField] Color gizmoColor = new Color(1f, 0.3f, 0.3f, 0.8f);

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}
