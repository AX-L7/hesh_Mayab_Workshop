using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] bool destroyOnDeath;
    [SerializeField] float destroyDelay;

    [Header("Invulnerability")]
    [SerializeField] float invulnerabilityDuration;

    [Header("Events")]
    public UnityEvent<float, float> OnHealthChanged;
    public UnityEvent<Vector3, Vector3> OnDamaged;
    public UnityEvent OnDeath;
    public UnityEvent OnHealed;

    float currentHealth;
    bool isDead;
    bool isInvulnerable;
    float invulnerabilityTimer;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => currentHealth / maxHealth;
    public bool IsDead => isDead;
    public bool IsInvulnerable { get => isInvulnerable; set => isInvulnerable = value; }

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
                isInvulnerable = false;
        }
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, transform.position, Vector3.zero);
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (isDead || isInvulnerable) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke(hitPoint, hitDirection);

        if (invulnerabilityDuration > 0f)
        {
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
        }

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHealed?.Invoke();
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        isInvulnerable = false;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }
}
