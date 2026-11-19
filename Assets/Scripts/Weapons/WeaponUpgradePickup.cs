
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PooledObject))]
public class WeaponUpgradePickup : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField, Min(0.1f)]
    private float fallSpeed = 1.5f;

    [SerializeField, Min(1f)]
    private float maxLifetime = 12f;


    private Rigidbody2D rb;
    private PooledObject pooledObject;

    private float lifetime;

    private bool collected;
    private bool launched;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pooledObject = GetComponent<PooledObject>();

        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError(
                "[WeaponUpgradePickup] Collider2D is missing!",
                this
            );
        }
    }


    private void OnEnable()
    {
        lifetime = 0f;
        collected = false;
        launched = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }


    public void Launch(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.down;

        rb.linearVelocity =
            direction.normalized * Mathf.Max(0.1f, speed);

        launched = true;
    }


    private void Update()
    {
        if (!launched || collected)
            return;

        lifetime += Time.deltaTime;

        if (lifetime >= maxLifetime)
        {
            ReturnToPool();
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        WeaponManager manager =
            other.GetComponentInParent<WeaponManager>();

        if (manager == null)
            return;

        collected = true;

        manager.UpgradeWeapon();

        Debug.Log(
            "[WeaponUpgradePickup] Weapon upgrade collected!",
            this
        );

        ReturnToPool();
    }


    private void ReturnToPool()
    {
        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }


    private void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        launched = false;
    }


    private void OnValidate()
    {
        fallSpeed = Mathf.Max(0.1f, fallSpeed);
        maxLifetime = Mathf.Max(1f, maxLifetime);
    }
}
