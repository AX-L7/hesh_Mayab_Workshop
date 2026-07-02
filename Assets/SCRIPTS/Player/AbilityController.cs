using UnityEngine;

public class AbilityController : MonoBehaviour
{
    [Header("Grenade")]
    [SerializeField] GameObject grenadePrefab;
    [SerializeField] float grenadeThrowForce = 15f;
    [SerializeField] float grenadeArc = 30f;
    [SerializeField] float grenadeCooldown = 3f;
    [SerializeField] int maxGrenades = 3;

    [Header("Melee")]
    [SerializeField] float meleeDamage = 20f;
    [SerializeField] float meleeRange = 2.5f;
    [SerializeField] float meleeKnockback = 12f;
    [SerializeField] float meleeCooldown = 1f;
    [SerializeField] LayerMask meleeTargetLayers = ~0;
    [SerializeField] GameObject meleeEffectPrefab;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string grenadeTrigger = "Grenade";
    [SerializeField] string meleeTrigger = "Melee";

    [Header("Audio")]
    [SerializeField] AudioClip grenadeThrowSound;
    [SerializeField] AudioClip meleeSwingSound;

    [Header("References")]
    [SerializeField] InputReader input;
    [SerializeField] PlayerController playerController;

    float grenadeCooldownTimer;
    float meleeCooldownTimer;
    int currentGrenades;

    public int CurrentGrenades => currentGrenades;
    public int MaxGrenades => maxGrenades;
    public float GrenadeCooldownPercent => Mathf.Clamp01(grenadeCooldownTimer / grenadeCooldown);
    public float MeleeCooldownPercent => Mathf.Clamp01(meleeCooldownTimer / meleeCooldown);

    void Start()
    {
        currentGrenades = maxGrenades;
    }

    void Update()
    {
        grenadeCooldownTimer = Mathf.Max(0f, grenadeCooldownTimer - Time.deltaTime);
        meleeCooldownTimer = Mathf.Max(0f, meleeCooldownTimer - Time.deltaTime);

        if (input.GrenadePressed && grenadeCooldownTimer <= 0f && currentGrenades > 0)
            ThrowGrenade();

        if (input.MeleePressed && meleeCooldownTimer <= 0f)
            MeleeAttack();
    }

    void ThrowGrenade()
    {
        currentGrenades--;
        grenadeCooldownTimer = grenadeCooldown;

        Vector3 throwDir = playerController != null ? playerController.AimDirection : transform.forward;
        throwDir.y = 0f;
        throwDir.Normalize();

        Quaternion throwRotation = Quaternion.LookRotation(throwDir) * Quaternion.Euler(-grenadeArc, 0f, 0f);

        if (grenadePrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 1.2f + throwDir * 0.5f;
            GameObject grenade = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);

            Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
            if (grenadeRb != null)
                grenadeRb.linearVelocity = throwRotation * Vector3.forward * grenadeThrowForce;
        }

        if (grenadeThrowSound != null)
            AudioManager.Instance?.PlaySFX(grenadeThrowSound, transform.position);

        if (animator != null)
            animator.SetTrigger(grenadeTrigger);
    }

    void MeleeAttack()
    {
        meleeCooldownTimer = meleeCooldown;

        if (meleeSwingSound != null)
            AudioManager.Instance?.PlaySFX(meleeSwingSound, transform.position);

        if (animator != null)
            animator.SetTrigger(meleeTrigger);

        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1f, meleeRange, meleeTargetLayers);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                target.TakeDamage(meleeDamage, hit.transform.position, dir);
            }

            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                Vector3 force = (hit.transform.position - transform.position).normalized * meleeKnockback;
                force.y = 2f;
                hitRb.AddForce(force, ForceMode.Impulse);
            }
        }

        if (meleeEffectPrefab != null)
        {
            Vector3 effectPos = transform.position + transform.forward * 1.2f + Vector3.up;
            GameObject fx = Instantiate(meleeEffectPrefab, effectPos, transform.rotation);
            Destroy(fx, 2f);
        }

        CameraShake.Instance?.Shake(0.4f);
    }

    public void AddGrenades(int amount)
    {
        currentGrenades = Mathf.Min(currentGrenades + amount, maxGrenades);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1f, meleeRange);
    }
}
