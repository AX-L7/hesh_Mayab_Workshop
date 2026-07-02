using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] Image healthBarFill;
    [SerializeField] TMP_Text healthText;
    [SerializeField] Color healthFullColor = Color.green;
    [SerializeField] Color healthLowColor = Color.red;
    [SerializeField] float lowHealthThreshold = 0.3f;

    [Header("Ammo")]
    [SerializeField] TMP_Text ammoText;
    [SerializeField] Image ammoIcon;
    [SerializeField] Color ammoNormalColor = Color.white;
    [SerializeField] Color ammoLowColor = Color.red;
    [SerializeField] int lowAmmoThreshold = 3;

    [Header("Weapon")]
    [SerializeField] TMP_Text weaponNameText;
    [SerializeField] Image weaponIcon;

    [Header("Wave Info")]
    [SerializeField] TMP_Text waveText;
    [SerializeField] TMP_Text enemyCountText;

    [Header("Score")]
    [SerializeField] TMP_Text scoreText;

    [Header("Grenades")]
    [SerializeField] TMP_Text grenadeText;

    [Header("Crosshair")]
    [SerializeField] RectTransform crosshair;
    [SerializeField] Canvas parentCanvas;

    [Header("Dash Indicator")]
    [SerializeField] Image[] dashChargeIcons;
    [SerializeField] Color dashReadyColor = Color.white;
    [SerializeField] Color dashUsedColor = new Color(1f, 1f, 1f, 0.3f);

    public void UpdateHealth(float current, float max)
    {
        float percent = current / max;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = percent;
            healthBarFill.color = Color.Lerp(healthLowColor, healthFullColor, percent);
        }

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{current} / {max}";
            ammoText.color = current <= lowAmmoThreshold ? ammoLowColor : ammoNormalColor;
        }
    }

    public void UpdateWeapon(WeaponData weapon)
    {
        if (weapon == null) return;

        if (weaponNameText != null)
            weaponNameText.text = weapon.weaponName;

        if (weaponIcon != null && weapon.icon != null)
        {
            weaponIcon.sprite = weapon.icon;
            weaponIcon.enabled = true;
        }
    }

    public void UpdateWave(int current, int total)
    {
        if (waveText != null)
            waveText.text = $"Wave {current}/{total}";
    }

    public void UpdateEnemyCount(int count)
    {
        if (enemyCountText != null)
            enemyCountText.text = $"Enemies: {count}";
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    public void UpdateGrenades(int current, int max)
    {
        if (grenadeText != null)
            grenadeText.text = $"{current}/{max}";
    }

    public void UpdateDashCharges(int current, int max)
    {
        if (dashChargeIcons == null) return;
        for (int i = 0; i < dashChargeIcons.Length; i++)
        {
            if (dashChargeIcons[i] == null) continue;
            dashChargeIcons[i].color = i < current ? dashReadyColor : dashUsedColor;
        }
    }

    void Update()
    {
        UpdateCrosshair();
    }

    void UpdateCrosshair()
    {
        if (crosshair == null || parentCanvas == null) return;

        Vector2 mousePos = Input.mousePosition;

        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            crosshair.position = mousePos;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.GetComponent<RectTransform>(), mousePos, parentCanvas.worldCamera, out Vector2 localPos);
            crosshair.localPosition = localPos;
        }
    }
}
