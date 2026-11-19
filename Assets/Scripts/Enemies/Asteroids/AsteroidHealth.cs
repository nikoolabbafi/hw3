
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PooledObject))]
public class AsteroidHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]

    [SerializeField, Min(1f)]
    private float maxHealth = 3f;

    [SerializeField]
    private float currentHealth;

    private PooledObject pooledObject;

    private bool isDestroyed;

    public float MaxHealth => maxHealth;

    public float CurrentHealth => currentHealth;


    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }


    private void OnEnable()
    {
        currentHealth = maxHealth;

        isDestroyed = false;
    }


    public void Initialize(float health)
    {
        maxHealth = Mathf.Max(1f, health);

        currentHealth = maxHealth;

        isDestroyed = false;
    }


    public void TakeDamage(float damage)
    {
        if (isDestroyed || damage <= 0f)
            return;

        currentHealth = Mathf.Max(
            0f,
            currentHealth - damage
        );

        Debug.Log(
            $"[AsteroidHealth] {currentHealth}/{maxHealth}"
        );

        if (currentHealth <= 0f)
        {
            DestroyAsteroid();
        }
    }


    private void DestroyAsteroid()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        Debug.Log(
            "[AsteroidHealth] Asteroid destroyed!"
        );

        GameEvents.RaiseAsteroidDestroyed();

        pooledObject.ReturnToPool();
    }


    private void OnValidate()
    {
        maxHealth = Mathf.Max(
            1f,
            maxHealth
        );

        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );
    }
}
