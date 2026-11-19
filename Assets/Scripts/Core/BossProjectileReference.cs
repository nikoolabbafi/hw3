using UnityEngine;

public static class BossProjectileReference
{
    private static GameObject prefab;

    public static GameObject Prefab
    {
        get
        {
            if (prefab == null)
                prefab = Resources.Load<GameObject>("Projectiles/Egg_Projectile");

            return prefab;
        }
    }
}
