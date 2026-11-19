using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class BossHealth : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    private float currentHealth;
    private PooledObject pooledObject;
    private bool defeated;
    public float NormalizedHealth => maxHealth <= 0f ? 0f : currentHealth / maxHealth;

    public float MaxHealth => maxHealth;

    public float CurrentHealth => currentHealth;

    private void Awake() => pooledObject = GetComponent<PooledObject>();

    private void OnEnable()
    {
        currentHealth = maxHealth;
        defeated = false;
    }

    private void Start() => UpdateHealthBar();

    public void Initialize(float health)
    {
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void SetHealthBar(UnityEngine.UI.Slider slider)
    {
        healthBar = slider;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || defeated || currentHealth <= 0f)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        UpdateHealthBar();
        if (currentHealth <= 0f)
        {
            defeated = true;
            GameManager.Instance?.AddScore(1000);
            GameEvents.RaiseEnemyDefeated();
            pooledObject.ReturnToPool();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.value = NormalizedHealth;
    }
}
