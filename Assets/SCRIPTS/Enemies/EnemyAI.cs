using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Chase, Attack, Dead }

    [Header("Data")]
    [SerializeField] EnemyData data;

    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] HealthComponent health;
    [SerializeField] HitFlash hitFlash;

    [Header("Animation Parameters")]
    [SerializeField] string speedParam = "Speed";
    [SerializeField] string attackTrigger = "Attack";
    [SerializeField] string deathTrigger = "Death";
    [SerializeField] string hitTrigger = "Hit";

    [Header("Detection")]
    [SerializeField] LayerMask playerLayer;

    NavMeshAgent agent;
    Transform target;
    State currentState = State.Idle;
    float attackTimer;
    float idleSoundTimer;
    ObjectPool ownerPool;

    public State CurrentState => currentState;
    public EnemyData Data => data;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (health == null) health = GetComponent<HealthComponent>();
        if (hitFlash == null) hitFlash = GetComponent<HitFlash>();
    }

    void OnEnable()
    {
        currentState = State.Idle;
        attackTimer = 0f;

        if (health != null)
        {
            health.OnDamaged.AddListener(OnDamaged);
            health.OnDeath.AddListener(OnDeath);
            health.ResetHealth();
        }

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }
    }

    void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged.RemoveListener(OnDamaged);
            health.OnDeath.RemoveListener(OnDeath);
        }
    }

    public void Initialize(EnemyData enemyData, Transform playerTarget, ObjectPool pool = null)
    {
        data = enemyData;
        target = playerTarget;
        ownerPool = pool;
        ApplyData();
    }

    void ApplyData()
    {
        if (data == null) return;
        agent.speed = data.moveSpeed;
        agent.stoppingDistance = data.stoppingDistance;
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        if (target == null)
            FindPlayer();

        switch (currentState)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: UpdateAttack(); break;
        }

        UpdateAnimation();
        UpdateIdleSound();
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    void UpdateIdle()
    {
        if (target == null) return;
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= data.detectionRange)
            currentState = State.Chase;
    }

    void UpdateChase()
    {
        if (target == null) { currentState = State.Idle; return; }

        agent.isStopped = false;
        agent.SetDestination(target.position);

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= data.attackRange)
            currentState = State.Attack;
        else if (dist > data.detectionRange * 1.5f)
            currentState = State.Idle;
    }

    void UpdateAttack()
    {
        if (target == null) { currentState = State.Idle; return; }

        agent.isStopped = true;

        Vector3 lookDir = target.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 10f * Time.deltaTime);

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = data.attackCooldown;
        }

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > data.attackRange * 1.3f)
            currentState = State.Chase;
    }

    void PerformAttack()
    {
        if (animator != null)
            animator.SetTrigger(attackTrigger);

        if (data.attackSounds != null && data.attackSounds.Length > 0)
            AudioManager.Instance?.PlaySFX(data.attackSounds[Random.Range(0, data.attackSounds.Length)], transform.position);

        if (target == null) return;
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= data.attackRange * 1.2f)
        {
            IDamageable playerHealth = target.GetComponent<IDamageable>();
            playerHealth?.TakeDamage(data.damage, transform.position, transform.forward);
        }
    }

    void OnDamaged(Vector3 hitPoint, Vector3 hitDir)
    {
        if (hitFlash != null)
            hitFlash.Flash();

        if (animator != null)
            animator.SetTrigger(hitTrigger);

        if (currentState == State.Idle)
            currentState = State.Chase;
    }

    void OnDeath()
    {
        currentState = State.Dead;
        agent.isStopped = true;
        agent.enabled = false;

        if (animator != null)
            animator.SetTrigger(deathTrigger);

        if (data.deathSounds != null && data.deathSounds.Length > 0)
            AudioManager.Instance?.PlaySFX(data.deathSounds[Random.Range(0, data.deathSounds.Length)], transform.position);

        if (data.deathEffectPrefab != null)
            Instantiate(data.deathEffectPrefab, transform.position, Quaternion.identity);

        if (data.explodesOnDeath)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, data.explosionRadius);
            foreach (Collider hit in hits)
            {
                if (hit.gameObject == gameObject) continue;
                IDamageable damageable = hit.GetComponent<IDamageable>();
                damageable?.TakeDamage(data.explosionDamage, transform.position, (hit.transform.position - transform.position).normalized);

                Rigidbody rb = hit.GetComponent<Rigidbody>();
                rb?.AddExplosionForce(data.explosionDamage, transform.position, data.explosionRadius, 1f, ForceMode.Impulse);
            }
        }

        GameManager.Instance?.AddScore(data.scoreValue);
        WaveSpawner spawner = FindAnyObjectByType<WaveSpawner>();
        spawner?.OnEnemyDied();

        if (ownerPool != null)
            Invoke(nameof(ReturnToPool), 3f);
        else
            Destroy(gameObject, 3f);
    }

    void ReturnToPool()
    {
        ownerPool.Return(gameObject);
    }

    void UpdateAnimation()
    {
        if (animator == null) return;
        animator.SetFloat(speedParam, agent.velocity.magnitude / Mathf.Max(data.moveSpeed, 0.01f));
    }

    void UpdateIdleSound()
    {
        if (data.idleSounds == null || data.idleSounds.Length == 0) return;
        if (Random.value < data.idleSoundChance * Time.deltaTime)
            AudioManager.Instance?.PlaySFX(data.idleSounds[Random.Range(0, data.idleSounds.Length)], transform.position, 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
}
