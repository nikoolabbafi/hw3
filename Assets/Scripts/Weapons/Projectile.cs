
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PooledObject))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]

    [SerializeField, Min(0.1f)]
    private float maxLifetime = 5f;

    private Rigidbody2D rb;
    private PooledObject pooledObject;

    private float lifetime;
    private bool initialized;

    public float Damage { get; private set; }


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pooledObject = GetComponent<PooledObject>();
    }


   
    public void Initialize(
        Vector2 direction,
        float speed,
        float damage)
    {
        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.up;

        direction.Normalize();

        Damage = Mathf.Max(0f, damage);

        lifetime = 0f;
        initialized = true;

        rb.linearVelocity = direction * speed;
        rb.angularVelocity = 0f;
    }


    private void Update()
    {
        if (!initialized)
            return;

        lifetime += Time.deltaTime;

        if (lifetime >= maxLifetime)
        {
            ReturnToPool();
        }
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        Transform current = other.transform;

        bool isEnemy = false;

        while (current != null)
        {
            if (current.CompareTag("Enemy"))
            {
                isEnemy = true;
                break;
            }

            current = current.parent;
        }

        if (!isEnemy)
            return;

        IDamageable target = null;

        current = other.transform;

        while (current != null && target == null)
        {
            target = current.GetComponent<IDamageable>();
            current = current.parent;
        }

        if (target != null)
        {
            target.TakeDamage(Damage);
        }

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

        lifetime = 0f;
        initialized = false;
    }


    private void OnValidate()
    {
        maxLifetime = Mathf.Max(0.1f, maxLifetime);
    }
}
