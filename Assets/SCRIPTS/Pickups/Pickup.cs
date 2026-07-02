using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType { Health, Ammo, Grenade, Weapon }

    [Header("Type & Value")]
    [SerializeField] PickupType type = PickupType.Health;
    [SerializeField] float amount = 25f;
    [SerializeField] WeaponData weaponToGive;

    [Header("Visual")]
    [SerializeField] float bobHeight = 0.3f;
    [SerializeField] float bobSpeed = 2f;
    [SerializeField] float rotateSpeed = 90f;
    [SerializeField] GameObject pickupEffectPrefab;

    [Header("Audio")]
    [SerializeField] AudioClip pickupSound;

    [Header("Settings")]
    [SerializeField] bool destroyOnPickup = true;
    [SerializeField] string playerTag = "Player";

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + Vector3.up * yOffset;
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        bool used = false;

        switch (type)
        {
            case PickupType.Health:
                HealthComponent hp = other.GetComponent<HealthComponent>();
                if (hp != null && hp.CurrentHealth < hp.MaxHealth)
                {
                    hp.Heal(amount);
                    used = true;
                }
                break;

            case PickupType.Ammo:
                WeaponController wc = other.GetComponent<WeaponController>();
                if (wc != null)
                {
                    wc.EquipWeapon(0);
                    used = true;
                }
                break;

            case PickupType.Grenade:
                AbilityController ac = other.GetComponent<AbilityController>();
                if (ac != null)
                {
                    ac.AddGrenades((int)amount);
                    used = true;
                }
                break;

            case PickupType.Weapon:
                WeaponController weaponCtrl = other.GetComponent<WeaponController>();
                if (weaponCtrl != null && weaponToGive != null)
                {
                    weaponCtrl.AddWeapon(weaponToGive);
                    used = true;
                }
                break;
        }

        if (!used) return;

        if (pickupSound != null)
            AudioManager.Instance?.PlaySFX(pickupSound, transform.position);

        if (pickupEffectPrefab != null)
        {
            GameObject fx = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        if (destroyOnPickup)
            Destroy(gameObject);
    }
}
