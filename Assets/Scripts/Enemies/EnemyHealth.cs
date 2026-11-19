
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PooledObject))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]

    [SerializeField, Min(1f)]
    private float maxHealth = 8f;

    [SerializeField]
    private float currentHealth;


    [Header("Score Reward")]

    [SerializeField, Min(0)]
    private int scoreValue = 100;


    private float prefabMaxHealth;

    private PooledObject pooledObject;

    private bool isDead;


    public float CurrentHealth => currentHealth;

    public float MaxHealth => maxHealth;

    public int ScoreValue => scoreValue;


    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();

        prefabMaxHealth = maxHealth;
    }


    private void OnEnable()
    {
        maxHealth = prefabMaxHealth;

        currentHealth = maxHealth;

        isDead = false;
    }



    public void Initialize(float health)
    {
        maxHealth = Mathf.Max(1f, health);

        currentHealth = maxHealth;

        isDead = false;
    }


    // TAKE DAMAGE

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        currentHealth = Mathf.Max(
            0f,
            currentHealth - damage
        );

        Debug.Log(
            $"[EnemyHealth] {name} Health: " +
            $"{currentHealth}/{maxHealth}"
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }


    // DIE

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;


        Debug.Log(
            $"[EnemyHealth] {name} Defeated!"
        );



        EnemyDropTable dropTable =
            GetComponent<EnemyDropTable>();

        if (dropTable != null)
        {
            dropTable.DropWeaponUpgrade();
        }



        GameEvents.RaiseScoreAwarded(scoreValue);



        GameEvents.RaiseEnemyDefeated();



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

        scoreValue = Mathf.Max(0, scoreValue);
    }
}
