
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerWeaponController))]
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Progression")]

    [SerializeField]
    private WeaponDataSO[] weaponLevels =
        new WeaponDataSO[3];

    [SerializeField, Min(1)]
    private int startingWeaponLevel = 1;

    private PlayerWeaponController weaponController;

    private int currentWeaponIndex = -1;

    public int CurrentWeaponLevel =>
        currentWeaponIndex + 1;

    public WeaponDataSO CurrentWeaponData =>
        currentWeaponIndex >= 0 &&
        weaponLevels != null &&
        currentWeaponIndex < weaponLevels.Length
            ? weaponLevels[currentWeaponIndex]
            : null;

    private void Awake()
    {
        weaponController =
            GetComponent<PlayerWeaponController>();
    }

    private void Start()
    {
        int levelToEquip = GameManager.Instance != null
            ? GameManager.Instance.WeaponPowerLevel
            : startingWeaponLevel;

        if (weaponLevels == null || weaponLevels.Length == 0)
        {
            Debug.LogError("[WeaponManager] No weapon levels configured!", this);
            return;
        }

        levelToEquip = Mathf.Clamp(levelToEquip, 1, weaponLevels.Length);

        if (!EquipWeapon(levelToEquip))
        {
            int fallbackLevel = Mathf.Clamp(startingWeaponLevel, 1, weaponLevels.Length);
            if (fallbackLevel != levelToEquip)
                EquipWeapon(fallbackLevel);
        }
    }

    public void UpgradeWeapon()
    {
        if (weaponLevels == null ||
            currentWeaponIndex + 1 >= weaponLevels.Length)
        {
            Debug.Log("[WeaponManager] Maximum weapon level reached!");
            return;
        }

        EquipWeapon(currentWeaponIndex + 2);
    }


    public bool DowngradeWeapon()
    {
        if (currentWeaponIndex <= 0)
        {
            Debug.Log(
                "[WeaponManager] Minimum weapon level reached!"
            );

            return false;
        }

        int previousLevel = CurrentWeaponLevel - 1;

        return EquipWeapon(previousLevel);
    }


    public bool EquipWeapon(int level)
    {
        if (weaponLevels == null ||
            level < 1 ||
            level > weaponLevels.Length)
        {
            Debug.LogError("[WeaponManager] Invalid weapon level!");
            return false;
        }

        int index = level - 1;

        WeaponDataSO selectedWeapon = weaponLevels[index];

        if (selectedWeapon == null)
        {
            Debug.LogError(
                $"[WeaponManager] Weapon Level {level} is missing!"
            );

            return false;
        }

        weaponController.SetWeaponData(selectedWeapon);

        currentWeaponIndex = index;

        GameEvents.RaiseWeaponLevelChanged(level);

        Debug.Log(
            $"[WeaponManager] Weapon Level: {level} - {selectedWeapon.WeaponName}"
        );

        return true;
    }

    [ContextMenu("Debug/Upgrade Weapon")]
    private void DebugUpgradeWeapon()
    {
        UpgradeWeapon();
    }

    [ContextMenu("Debug/Equip Level 1")]
    private void DebugEquipLevel1()
    {
        EquipWeapon(1);
    }

    [ContextMenu("Debug/Equip Level 2")]
    private void DebugEquipLevel2()
    {
        EquipWeapon(2);
    }

    [ContextMenu("Debug/Equip Level 3")]
    private void DebugEquipLevel3()
    {
        EquipWeapon(3);
    }
}
