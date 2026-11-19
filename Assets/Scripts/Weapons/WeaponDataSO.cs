
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWeaponData",
    menuName = "Chicken Shooter/Data/Weapon Data"
)]
public class WeaponDataSO : ScriptableObject
{
    [Header("Weapon Information")]

    [SerializeField]
    private string weaponName = "Single Laser";

    [Tooltip("Weapon icon or visual representation.")]
    [SerializeField]
    private Sprite weaponSprite;

    [SerializeField]
    private GameObject projectilePrefab;


    [Header("Combat Stats")]

    [SerializeField, Min(0.1f)]
    private float damage = 1f;

    [SerializeField, Min(0.1f)]
    private float projectileSpeed = 12f;

    [Tooltip("Number of shots per second.")]
    [SerializeField, Min(0.1f)]
    private float fireRate = 4f;


    [Header("Firing Pattern")]

    [SerializeField]
    private WeaponFirePattern firePattern =
        WeaponFirePattern.Single;

    [SerializeField, Min(1)]
    private int projectileCount = 1;

    [SerializeField, Range(0f, 90f)]
    private float spreadAngleDegrees = 0f;

    [SerializeField]
    private Vector2[] muzzleOffsets =
    {
        Vector2.zero
    };



    public string WeaponName => weaponName;

    public Sprite WeaponSprite => weaponSprite;

    public GameObject ProjectilePrefab => projectilePrefab;

    public float Damage => damage;

    public float ProjectileSpeed => projectileSpeed;

    public float FireRate => fireRate;

    public WeaponFirePattern FirePattern => firePattern;

    public int ProjectileCount => projectileCount;

    public float SpreadAngleDegrees => spreadAngleDegrees;

    public Vector2[] MuzzleOffsets =>
        muzzleOffsets ?? System.Array.Empty<Vector2>();


    

    public bool ValidateConfiguration(out string error)
    {
        if (string.IsNullOrWhiteSpace(weaponName))
        {
            return Fail("Weapon name is missing.", out error);
        }

        if (weaponSprite == null)
        {
            return Fail("Weapon sprite is missing.", out error);
        }

        if (projectilePrefab == null)
        {
            return Fail("Projectile prefab is missing.", out error);
        }

        if (damage <= 0f)
        {
            return Fail("Damage must be greater than zero.", out error);
        }

        if (projectileSpeed <= 0f)
        {
            return Fail("Projectile speed must be greater than zero.", out error);
        }

        if (fireRate <= 0f)
        {
            return Fail("Fire rate must be greater than zero.", out error);
        }



        if (!projectilePrefab.TryGetComponent<Projectile>(out _))
        {
            return Fail(
                "Projectile component is missing from Projectile Prefab.",
                out error
            );
        }

        if (!projectilePrefab.TryGetComponent<Rigidbody2D>(out _))
        {
            return Fail(
                "Rigidbody2D is missing from Projectile Prefab.",
                out error
            );
        }

        if (!projectilePrefab.TryGetComponent<PooledObject>(out _))
        {
            return Fail(
                "PooledObject is missing from Projectile Prefab.",
                out error
            );
        }

        SpriteRenderer renderer =
            projectilePrefab.GetComponent<SpriteRenderer>();

        if (renderer == null || renderer.sprite == null)
        {
            return Fail(
                "Projectile Prefab has no valid SpriteRenderer sprite.",
                out error
            );
        }



        switch (firePattern)
        {
            case WeaponFirePattern.Single:

                if (projectileCount != 1)
                {
                    return Fail(
                        "Single pattern must have exactly 1 projectile.",
                        out error
                    );
                }

                break;


            case WeaponFirePattern.Twin:

                if (projectileCount != 2)
                {
                    return Fail(
                        "Twin pattern must have exactly 2 projectiles.",
                        out error
                    );
                }

                if (muzzleOffsets == null ||
                    muzzleOffsets.Length < 2)
                {
                    return Fail(
                        "Twin pattern requires 2 muzzle offsets.",
                        out error
                    );
                }

                break;


            case WeaponFirePattern.Spread:

                if (projectileCount != 3)
                {
                    return Fail(
                        "Spread pattern must have exactly 3 projectiles.",
                        out error
                    );
                }

                if (spreadAngleDegrees <= 0f)
                {
                    return Fail(
                        "Spread angle must be greater than zero.",
                        out error
                    );
                }

                break;


            default:

                return Fail(
                    "Unknown weapon firing pattern.",
                    out error
                );
        }



        if (muzzleOffsets == null ||
            muzzleOffsets.Length == 0)
        {
            return Fail(
                "Muzzle offsets are not configured.",
                out error
            );
        }

        foreach (Vector2 offset in muzzleOffsets)
        {
            if (float.IsNaN(offset.x) ||
                float.IsNaN(offset.y) ||
                float.IsInfinity(offset.x) ||
                float.IsInfinity(offset.y))
            {
                return Fail(
                    "Muzzle offset contains an invalid value.",
                    out error
                );
            }
        }

        error = null;
        return true;
    }


    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }



    [ContextMenu("Validate Weapon Data")]
    private void ValidateWeaponData()
    {
        if (ValidateConfiguration(out string error))
        {
            Debug.Log(
                $"[WeaponDataSO] {weaponName} validated successfully. " +
                $"Pattern: {firePattern}, Damage: {damage}, " +
                $"Fire Rate: {fireRate}",
                this
            );
        }
        else
        {
            Debug.LogError(
                $"[WeaponDataSO] {weaponName} is invalid: {error}",
                this
            );
        }
    }




    private void OnValidate()
    {
        damage = Mathf.Max(0.1f, damage);

        projectileSpeed = Mathf.Max(
            0.1f,
            projectileSpeed
        );

        fireRate = Mathf.Max(
            0.1f,
            fireRate
        );

        projectileCount = Mathf.Max(
            1,
            projectileCount
        );

        spreadAngleDegrees = Mathf.Clamp(
            spreadAngleDegrees,
            0f,
            90f
        );

        if (muzzleOffsets == null)
        {
            muzzleOffsets = new Vector2[]
            {
                Vector2.zero
            };
        }
    }
}



















































