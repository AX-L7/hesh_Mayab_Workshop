using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] float dashSpeed = 25f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float dashCooldown = 1f;
    [SerializeField] int dashCharges = 1;
    [SerializeField] float chargeRegenTime = 2f;

    [Header("Invincibility")]
    [SerializeField] bool grantIFrames = true;
    [SerializeField] HealthComponent healthComponent;

    [Header("Visual Feedback")]
    [SerializeField] GameObject dashTrailEffect;
    [SerializeField] Renderer[] dashGhostRenderers;
    [SerializeField] Material dashGhostMaterial;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string dashTrigger = "isDashing";

    [Header("References")]
    [SerializeField] InputReader input;
    [SerializeField] PlayerController playerController;

    Rigidbody rb;
    int currentCharges;
    float cooldownTimer;
    Material[] originalMaterials;

    public bool IsDashing { get; private set; }
    public bool IsInvulnerable { get; private set; }
    public int CurrentCharges => currentCharges;
    public int MaxCharges => dashCharges;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentCharges = dashCharges;
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f && currentCharges < dashCharges)
            {
                currentCharges++;
                if (currentCharges < dashCharges)
                    cooldownTimer = chargeRegenTime;
            }
        }

        if (input.DashPressed && currentCharges > 0 && !IsDashing)
            StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        currentCharges--;
        if (currentCharges < dashCharges && cooldownTimer <= 0f)
            cooldownTimer = chargeRegenTime;

        IsDashing = true;

        if (grantIFrames && healthComponent != null)
        {
            IsInvulnerable = true;
            healthComponent.IsInvulnerable = true;
        }

        if (animator != null)
            animator.SetBool(dashTrigger, true);

        if (dashTrailEffect != null)
            dashTrailEffect.SetActive(true);

        ApplyGhostMaterial(true);

        Vector3 dashDir = playerController != null && playerController.MoveDirection.sqrMagnitude > 0.01f
            ? playerController.MoveDirection.normalized
            : transform.forward;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        IsDashing = false;

        if (animator != null)
            animator.SetBool(dashTrigger, false);

        if (grantIFrames && healthComponent != null)
        {
            IsInvulnerable = false;
            healthComponent.IsInvulnerable = false;
        }

        if (dashTrailEffect != null)
            dashTrailEffect.SetActive(false);

        ApplyGhostMaterial(false);
    }

    void ApplyGhostMaterial(bool active)
    {
        if (dashGhostMaterial == null || dashGhostRenderers == null) return;

        if (active)
        {
            originalMaterials = new Material[dashGhostRenderers.Length];
            for (int i = 0; i < dashGhostRenderers.Length; i++)
            {
                if (dashGhostRenderers[i] == null) continue;
                originalMaterials[i] = dashGhostRenderers[i].material;
                dashGhostRenderers[i].material = dashGhostMaterial;
            }
        }
        else
        {
            for (int i = 0; i < dashGhostRenderers.Length; i++)
            {
                if (dashGhostRenderers[i] == null || originalMaterials[i] == null) continue;
                dashGhostRenderers[i].material = originalMaterials[i];
            }
        }
    }
}
