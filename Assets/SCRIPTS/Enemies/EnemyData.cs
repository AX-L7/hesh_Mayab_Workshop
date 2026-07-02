using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Zombie";

    [Header("Stats")]
    public float maxHealth = 50f;
    public float moveSpeed = 3.5f;
    public float damage = 10f;

    [Header("AI Behavior")]
    public float detectionRange = 20f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float stoppingDistance = 1.5f;
    [Range(0f, 2f)] public float attackWindup = 0.3f;

    [Header("Loot")]
    [Range(0f, 1f)] public float lootDropChance = 0.2f;
    public int scoreValue = 10;

    [Header("On Death")]
    public bool explodesOnDeath;
    public float explosionRadius = 3f;
    public float explosionDamage = 25f;
    public GameObject deathEffectPrefab;

    [Header("Audio")]
    public AudioClip[] idleSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] deathSounds;
    [Range(0f, 1f)] public float idleSoundChance = 0.01f;
}
