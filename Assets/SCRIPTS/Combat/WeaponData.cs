using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "Pistol";
    public Sprite icon;
    public GameObject weaponModel;

    [Header("Firing")]
    public float damage = 10f;
    public float fireRate = 5f;
    [Range(0f, 15f)] public float spread = 1f;
    public float range = 50f;
    public bool isAutomatic;

    [Header("Projectile")]
    public float projectileSpeed = 40f;
    public float projectileLifetime = 2f;
    public GameObject projectilePrefab;
    public GameObject muzzleFlashPrefab;
    public GameObject impactEffectPrefab;

    [Header("Ammo")]
    public int magazineSize = 12;
    public float reloadTime = 1.5f;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptyClickSound;

    [Header("Feel")]
    [Range(0f, 2f)] public float cameraShakeIntensity = 0.3f;
    [Range(0f, 1f)] public float movementPenalty = 0.6f;
    public float knockbackForce = 5f;
}
