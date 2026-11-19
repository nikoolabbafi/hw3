
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyDropTable : MonoBehaviour
{
    [Header("Weapon Upgrade Drop")]

    [SerializeField]
    private GameObject weaponUpgradePrefab;

    [SerializeField, Range(0f, 1f)]
    private float dropChance = 0.2f;

    [SerializeField]
    private Vector2 spawnOffset = new Vector2(0f, -0.2f);

    [SerializeField, Min(0.1f)]
    private float fallSpeed = 1.5f;


    public void DropWeaponUpgrade()
    {
        if (weaponUpgradePrefab == null)
            return;

        if (Random.value > dropChance)
            return;

        if (PoolManager.Instance == null)
        {
            Debug.LogWarning(
                "[EnemyDropTable] PoolManager not found!",
                this
            );

            return;
        }


        Vector3 spawnPosition =
            transform.position +
            new Vector3(
                spawnOffset.x,
                spawnOffset.y,
                0f
            );


        GameObject pickupObject =
            PoolManager.Instance.Spawn(
                weaponUpgradePrefab,
                spawnPosition,
                Quaternion.identity
            );

        if (pickupObject == null)
            return;


        WeaponUpgradePickup pickup =
            pickupObject.GetComponent<WeaponUpgradePickup>();

        if (pickup == null)
        {
            Debug.LogError(
                "[EnemyDropTable] WeaponUpgradePickup component missing!",
                pickupObject
            );

            pickupObject.GetComponent<PooledObject>()
                ?.ReturnToPool();

            return;
        }


        pickup.Launch(Vector2.down, fallSpeed);

        Debug.Log(
            "[EnemyDropTable] Weapon upgrade dropped!",
            this
        );
    }


    private void OnValidate()
    {
        dropChance = Mathf.Clamp01(dropChance);

        fallSpeed = Mathf.Max(0.1f, fallSpeed);
    }
}
