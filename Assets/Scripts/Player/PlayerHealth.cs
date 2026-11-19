
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]

    [SerializeField, Min(1)]
    private int maxHealth = 3;


    [Header("Invincibility Settings")]

    [SerializeField, Min(0.1f)]
    private float invincibilityDuration = 2f;

    [SerializeField, Min(0.02f)]
    private float blinkInterval = 0.1f;


    private int currentHealth;

    private bool isDead;

    private bool isInvincible;

    private Rigidbody2D rb;

    private WeaponManager weaponManager;

    private SpriteRenderer[] spriteRenderers;

    private bool[] initialRendererStates;

    private Coroutine invincibilityCoroutine;


    

    public int MaxHealth => maxHealth;

    public int CurrentHealth => currentHealth;

    public bool IsDead => isDead;

    public bool IsInvincible => isInvincible;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        weaponManager = GetComponent<WeaponManager>();

        maxHealth = Mathf.Max(1, maxHealth);

        currentHealth = maxHealth;



        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        initialRendererStates =
            new bool[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            initialRendererStates[i] =
                spriteRenderers[i].enabled;
        }


        if (spriteRenderers.Length == 0)
        {
            Debug.LogWarning(
                "[PlayerHealth] No SpriteRenderer found!",
                this
            );
        }

        if (weaponManager == null)
        {
            Debug.LogWarning(
                "[PlayerHealth] WeaponManager not found!",
                this
            );
        }
    }


    private void Start()
    {
        if (GameManager.Instance != null)
            SetHealth(GameManager.Instance.PlayerLives);
        else
            GameEvents.RaisePlayerLivesChanged(currentHealth);
    }



    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") ||
            other.CompareTag("EnemyProjectile") ||
            other.CompareTag("Hazard"))
        {
            TakeDamage(1);
        }
    }



    public void TakeDamage(int damageAmount)
    {
        if (isDead ||
            isInvincible ||
            damageAmount <= 0)
        {
            return;
        }


        currentHealth = Mathf.Max(
            0,
            currentHealth - damageAmount
        );


    

        if (weaponManager != null)
        {
            weaponManager.DowngradeWeapon();
        }


        bool died = currentHealth <= 0;


        if (died)
        {
            isDead = true;

            rb.linearVelocity = Vector2.zero;

            rb.simulated = false;
        }
        else
        {
            isInvincible = true;

            invincibilityCoroutine =
                StartCoroutine(InvincibilityRoutine());
        }



        GameEvents.RaisePlayerDamaged();

        GameEvents.RaisePlayerLivesChanged(
            currentHealth
        );


        if (died)
        {
            GameEvents.RaisePlayerDied();
        }
    }


   

    private IEnumerator InvincibilityRoutine()
    {
        Debug.Log(
            "[PlayerHealth] Invincibility Started!",
            this
        );

        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            SetSpriteVisibility(false);

            float hiddenTime = Mathf.Min(
                blinkInterval,
                invincibilityDuration - elapsed
            );

            yield return new WaitForSeconds(hiddenTime);

            elapsed += hiddenTime;


            if (elapsed >= invincibilityDuration)
                break;


            SetSpriteVisibility(true);

            float visibleTime = Mathf.Min(
                blinkInterval,
                invincibilityDuration - elapsed
            );

            yield return new WaitForSeconds(visibleTime);

            elapsed += visibleTime;
        }


        RestoreSpriteVisibility();

        isInvincible = false;

        invincibilityCoroutine = null;


        Debug.Log(
            "[PlayerHealth] Invincibility Ended!",
            this
        );
    }


 

    private void SetSpriteVisibility(bool visible)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].enabled =
                    visible && initialRendererStates[i];
            }
        }
    }


    private void RestoreSpriteVisibility()
    {
        if (spriteRenderers == null)
            return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].enabled =
                    initialRendererStates[i];
            }
        }
    }


    public void SetHealth(int value)
    {
        bool wasDead = isDead;

        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        currentHealth = Mathf.Clamp(
            value,
            0,
            maxHealth
        );

        isDead = currentHealth <= 0;
        isInvincible = false;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = !isDead;

        RestoreSpriteVisibility();

        GameEvents.RaisePlayerLivesChanged(currentHealth);

       
        if (isDead && !wasDead)
        {
            GameEvents.RaisePlayerDied();
        }
    }


    public void RestoreFullHealth()
    {
        SetHealth(maxHealth);
    }


    private void OnDisable()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);

            invincibilityCoroutine = null;
        }

        isInvincible = false;

        RestoreSpriteVisibility();
    }


    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);

        invincibilityDuration = Mathf.Max(
            0.1f,
            invincibilityDuration
        );

        blinkInterval = Mathf.Max(
            0.02f,
            blinkInterval
        );
    }
}






















































