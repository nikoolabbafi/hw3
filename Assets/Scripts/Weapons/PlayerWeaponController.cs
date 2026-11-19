
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInput))]
public class PlayerWeaponController : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField]
    private WeaponDataSO weaponData;

    [Header("Fire Point")]
    [SerializeField]
    private Transform firePoint;

    private PlayerInput playerInput;
    private PlayerHealth playerHealth;
    private InputAction fireAction;

    private float nextFireTime;

    public WeaponDataSO CurrentWeaponData => weaponData;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerHealth = GetComponent<PlayerHealth>();

        fireAction = playerInput.currentActionMap?
            .FindAction("Fire", false);

        if (fireAction == null)
        {
            fireAction = playerInput.actions
                .FindAction("Gameplay/Fire", false);
        }

        if (fireAction == null)
        {
            Debug.LogError(
                "[Weapon] Fire action not found!"
            );
        }
    }

    public void SetWeaponData(WeaponDataSO newWeaponData)
    {
        if (newWeaponData == null)
        {
            Debug.LogError("[Weapon] Weapon Data is null!");
            return;
        }

        weaponData = newWeaponData;
        nextFireTime = 0f;

        Debug.Log(
            $"[Weapon] Equipped: {weaponData.WeaponName}"
        );
    }

    private void Update()
    {
        if (GameManager.State != GameFlowState.Playing ||
            (playerHealth != null && playerHealth.IsDead))
        {
            return;
        }

        if (weaponData == null ||
            firePoint == null ||
            fireAction == null)
            return;

        if (!fireAction.IsPressed())
            return;

        if (Time.time < nextFireTime)
            return;

        Fire();

        float interval =
            1f / Mathf.Max(0.1f, weaponData.FireRate);

        nextFireTime = Time.time + interval;
    }

    private void Fire()
    {
        if (PoolManager.Instance == null)
        {
            Debug.LogError("[Weapon] PoolManager not found!");
            return;
        }

        if (weaponData.ProjectilePrefab == null)
        {
            Debug.LogError("[Weapon] Projectile Prefab missing!");
            return;
        }

        int projectileCount = GetProjectileCount();

        Vector2[] offsets = weaponData.MuzzleOffsets;

        Vector2 baseDirection =
            ((Vector2)firePoint.up).normalized;

        for (int i = 0; i < projectileCount; i++)
        {
            Vector2 offset =
                offsets != null && i < offsets.Length
                ? offsets[i]
                : Vector2.zero;

            Vector3 spawnPosition =
                firePoint.TransformPoint(
                    new Vector3(offset.x, offset.y, 0f)
                );

            float angle = 0f;

   
            if (weaponData.FirePattern ==
                WeaponFirePattern.Spread &&
                projectileCount > 1)
            {
                float t = i / (float)(projectileCount - 1);

                angle = Mathf.Lerp(
                    -weaponData.SpreadAngleDegrees / 2f,
                    weaponData.SpreadAngleDegrees / 2f,
                    t
                );
            }

            Vector3 rotatedDirection =
                Quaternion.Euler(0f, 0f, angle) *
                new Vector3(
                    baseDirection.x,
                    baseDirection.y,
                    0f
                );

            Vector2 direction =
                new Vector2(
                    rotatedDirection.x,
                    rotatedDirection.y
                ).normalized;

            Quaternion rotation =
                Quaternion.FromToRotation(
                    Vector3.up,
                    new Vector3(direction.x, direction.y, 0f)
                );

            GameObject projectileObject =
                PoolManager.Instance.Spawn(
                    weaponData.ProjectilePrefab,
                    spawnPosition,
                    rotation
                );

            if (projectileObject == null)
                continue;

            Projectile projectile =
                projectileObject.GetComponent<Projectile>();

            if (projectile == null)
            {
                Debug.LogError(
                    "[Weapon] Projectile component missing!"
                );

                PooledObject pooled =
                    projectileObject.GetComponent<PooledObject>();

                if (pooled != null)
                    pooled.ReturnToPool();
                else
                    projectileObject.SetActive(false);

                continue;
            }

            projectile.Initialize(
                direction,
                weaponData.ProjectileSpeed,
                weaponData.Damage
            );
        }
    }

    private int GetProjectileCount()
    {
        switch (weaponData.FirePattern)
        {
            case WeaponFirePattern.Single:
                return 1;

            case WeaponFirePattern.Twin:
                return 2;

            case WeaponFirePattern.Spread:
                return Mathf.Max(
                    1,
                    weaponData.ProjectileCount
                );

            default:
                return Mathf.Max(
                    1,
                    weaponData.ProjectileCount
                );
        }
    }
}
