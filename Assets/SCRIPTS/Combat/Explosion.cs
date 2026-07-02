using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] float damage = 30f;
    [SerializeField] float radius = 5f;
    [SerializeField] float knockbackForce = 15f;
    [SerializeField] LayerMask damageLayers = ~0;

    [Header("Visuals")]
    [SerializeField] GameObject explosionEffectPrefab;

    [Header("Timing")]
    [SerializeField] float delay;
    [SerializeField] bool explodeOnStart = true;
    [SerializeField] bool destroyAfter = true;

    bool hasExploded;

    void Start()
    {
        if (explodeOnStart)
        {
            if (delay > 0f)
                Invoke(nameof(Explode), delay);
            else
                Explode();
        }
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 5f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, damageLayers);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float falloff = 1f - (distance / radius);
                target.TakeDamage(damage * falloff, hit.transform.position, direction);
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(knockbackForce, transform.position, radius, 0.5f, ForceMode.Impulse);
        }

        if (destroyAfter)
            Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
