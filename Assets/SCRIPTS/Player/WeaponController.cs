using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class WeaponController : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] WeaponData[] availableWeapons;
    [SerializeField] int startingWeaponIndex;
    [SerializeField] Transform muzzlePoint;

    [Header("Bullet Pool")]
    [SerializeField] ObjectPool bulletPool;

    [Header("References")]
    [SerializeField] InputReader input;
    [SerializeField] PlayerController playerController;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string fireTrigger = "Fire";
    [SerializeField] string reloadTrigger = "Reload";
    [SerializeField] string weaponIndexParam = "WeaponIndex";

    [Header("Events")]
    public UnityEvent<WeaponData> OnWeaponChanged;
    public UnityEvent<int, int> OnAmmoChanged;
    public UnityEvent OnFired;
    public UnityEvent OnReloadStart;
    public UnityEvent OnReloadEnd;

    int currentWeaponIndex;
    int currentAmmo;
    float fireTimer;
    bool isReloading;

    public WeaponData CurrentWeapon => availableWeapons != null && availableWeapons.Length > 0
        ? availableWeapons[currentWeaponIndex] : null;
    public int CurrentAmmo => currentAmmo;
    public int MagazineSize => CurrentWeapon != null ? CurrentWeapon.magazineSize : 0;
    public bool IsFiring { get; private set; }
    public bool IsReloading => isReloading;

    void Start()
    {
        if (availableWeapons == null || availableWeapons.Length == 0) return;
        currentWeaponIndex = Mathf.Clamp(startingWeaponIndex, 0, availableWeapons.Length - 1);
        EquipWeapon(currentWeaponIndex);
    }

    void Update()
    {
        if (CurrentWeapon == null) return;

        fireTimer -= Time.deltaTime;

        HandleFiring();
        HandleReload();
        HandleWeaponSwitch();
    }

    void HandleFiring()
    {
        bool wantsFire = CurrentWeapon.isAutomatic ? input.Fire : input.FireDown;

        if (wantsFire && fireTimer <= 0f && !isReloading && currentAmmo > 0)
        {
            Fire();
        }
        else if (wantsFire && currentAmmo <= 0 && !isReloading)
        {
            if (CurrentWeapon.emptyClickSound != null)
                AudioManager.Instance?.PlaySFX(CurrentWeapon.emptyClickSound, transform.position);
            StartCoroutine(ReloadRoutine());
        }

        IsFiring = wantsFire && fireTimer > -0.1f && !isReloading;
    }

    void Fire()
    {
        float interval = 1f / CurrentWeapon.fireRate;
        fireTimer = interval;
        currentAmmo--;

        Vector3 direction = muzzlePoint.forward;
        if (CurrentWeapon.spread > 0f)
        {
            float spreadAngle = CurrentWeapon.spread * 0.5f;
            direction = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0f
            ) * direction;
        }

        Quaternion rotation = Quaternion.LookRotation(direction);
        GameObject bulletObj = null;

        if (bulletPool != null)
            bulletObj = bulletPool.Get(muzzlePoint.position, rotation);

        if (bulletObj == null && CurrentWeapon.projectilePrefab != null)
            bulletObj = Instantiate(CurrentWeapon.projectilePrefab, muzzlePoint.position, rotation);

        if (bulletObj != null)
        {
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
                bullet.Initialize(
                    CurrentWeapon.damage,
                    CurrentWeapon.projectileSpeed,
                    CurrentWeapon.projectileLifetime,
                    CurrentWeapon.knockbackForce,
                    bulletPool,
                    CurrentWeapon.impactEffectPrefab
                );
        }

        if (CurrentWeapon.muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(CurrentWeapon.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            Destroy(flash, 1f);
        }

        if (CurrentWeapon.fireSound != null)
            AudioManager.Instance?.PlaySFX(CurrentWeapon.fireSound, transform.position);

        if (CurrentWeapon.cameraShakeIntensity > 0f)
            CameraShake.Instance?.Shake(CurrentWeapon.cameraShakeIntensity);

        if (animator != null)
            animator.SetTrigger(fireTrigger);

        OnAmmoChanged?.Invoke(currentAmmo, CurrentWeapon.magazineSize);
        OnFired?.Invoke();
    }

    void HandleReload()
    {
        if (input.ReloadPressed && !isReloading && currentAmmo < CurrentWeapon.magazineSize)
            StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        OnReloadStart?.Invoke();

        if (CurrentWeapon.reloadSound != null)
            AudioManager.Instance?.PlaySFX(CurrentWeapon.reloadSound, transform.position);

        if (animator != null)
            animator.SetTrigger(reloadTrigger);

        yield return new WaitForSeconds(CurrentWeapon.reloadTime);

        currentAmmo = CurrentWeapon.magazineSize;
        isReloading = false;
        OnReloadEnd?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo, CurrentWeapon.magazineSize);
    }

    void HandleWeaponSwitch()
    {
        if (isReloading) return;

        float scroll = input.WeaponScroll;
        if (scroll > 0.1f)
            CycleWeapon(1);
        else if (scroll < -0.1f)
            CycleWeapon(-1);

        if (input.WeaponNextPressed)
            CycleWeapon(1);
    }

    void CycleWeapon(int direction)
    {
        int newIndex = (currentWeaponIndex + direction + availableWeapons.Length) % availableWeapons.Length;
        EquipWeapon(newIndex);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= availableWeapons.Length) return;
        currentWeaponIndex = index;
        currentAmmo = CurrentWeapon.magazineSize;
        fireTimer = 0f;
        isReloading = false;

        if (animator != null)
            animator.SetInteger(weaponIndexParam, index);

        OnWeaponChanged?.Invoke(CurrentWeapon);
        OnAmmoChanged?.Invoke(currentAmmo, CurrentWeapon.magazineSize);
    }

    public void AddWeapon(WeaponData weapon)
    {
        var list = new System.Collections.Generic.List<WeaponData>(availableWeapons);
        if (!list.Contains(weapon))
        {
            list.Add(weapon);
            availableWeapons = list.ToArray();
        }
        EquipWeapon(availableWeapons.Length - 1);
    }
}
