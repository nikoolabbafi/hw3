
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PooledObject))]
public class EggProjectile : MonoBehaviour, IDamageable
{
    [Header("Egg Settings")]

    [SerializeField, Min(0.1f)]
    private float moveSpeed = 4f;

    [SerializeField, Min(1f)]
    private float maxHealth = 1f;

    [SerializeField, Min(0.1f)]
    private float maxLifetime = 6f;

    private Rigidbody2D rb;
    private PooledObject pooledObject;

    private float currentHealth;
    private float lifetime;

    private bool returnRequested;
    private bool launched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        currentHealth = maxHealth;

        lifetime = 0f;

        returnRequested = false;
        launched = false;
    }

    public void Launch(Vector2 direction)
    {
        Launch(direction, moveSpeed);
    }

    public void Launch(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.down;
        }

        rb.linearVelocity =
            direction.normalized * Mathf.Max(0.1f, speed);

        launched = true;
    }

    private void Update()
    {
        if (!launched || returnRequested)
            return;

        lifetime += Time.deltaTime;

        if (lifetime >= maxLifetime)
        {
            returnRequested = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!launched || returnRequested)
            return;

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        playerHealth.TakeDamage(1);
        returnRequested = true;
    }

    public void TakeDamage(float damage)
    {
        if (returnRequested || damage <= 0f)
            return;

        currentHealth = Mathf.Max(
            0f,
            currentHealth - damage
        );

        if (currentHealth <= 0f)
        {
            Debug.Log("[Egg] Destroyed!");

            returnRequested = true;
        }
    }

    private void LateUpdate()
    {
        if (returnRequested)
        {
            pooledObject.ReturnToPool();
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
}
