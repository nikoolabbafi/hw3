
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerBombController : MonoBehaviour
{
    private const float InstantKillDamage = 1000000f;
    private const float EnemyPlaneZ = 0f;

    [Header("Bomb Inventory")]

    [SerializeField, Min(0)]
    private int initialBombCount = 2;

    private int currentBombCount;

    public int CurrentBombCount => currentBombCount;


    [Header("Boss Damage")]

    [SerializeField, Range(0f, 1f)]
    private float bossDamagePercent = 0.18f;


    [Header("Explosion")]

    [SerializeField]
    private GameObject explosionPrefab;

    [SerializeField]
    private LayerMask enemyLayers = ~0;


    private PlayerInput playerInput;
    private PlayerHealth playerHealth;
    private InputAction bombAction;

    private readonly HashSet<IDamageable> processedTargets =
        new HashSet<IDamageable>();


    private void Awake()
    {
        

        currentBombCount = GameManager.Instance != null
            ? Mathf.Max(0, GameManager.Instance.BombCount)
            : Mathf.Max(0, initialBombCount);


        playerInput = GetComponent<PlayerInput>();
        playerHealth = GetComponent<PlayerHealth>();

        bombAction = playerInput.currentActionMap?
            .FindAction("Bomb", false);

        if (bombAction == null)
        {
            bombAction = playerInput.actions
                .FindAction("Gameplay/Bomb", false);
        }

        if (bombAction == null)
        {
            Debug.LogError(
                "[Bomb] Bomb action not found!",
                this
            );
        }
    }


    private void Start()
    {
        PublishBombCount();

        Debug.Log(
            $"[Bomb] Initial Inventory: {currentBombCount}",
            this
        );
    }


    private void Update()
    {
        if (!CanUseCombatActions())
            return;

        if (bombAction != null &&
            bombAction.WasPressedThisFrame())
        {
            TryUseBomb();
        }
    }


    private bool CanUseCombatActions()
    {
        if (GameManager.State != GameFlowState.Playing)
            return false;

        return playerHealth == null || !playerHealth.IsDead;
    }



    public void AddBombs(int amount)
    {
        if (amount <= 0)
            return;

        SetBombCount(currentBombCount + amount);

        Debug.Log(
            $"[Bomb] Added {amount} bomb(s). Total: {currentBombCount}",
            this
        );
    }


    public void SetBombCount(int amount)
    {
        int newCount = Mathf.Max(0, amount);

        if (newCount == currentBombCount)
            return;

        currentBombCount = newCount;

        PublishBombCount();
    }


    private void PublishBombCount()
    {
        GameEvents.RaiseBombCountChanged(
            currentBombCount
        );
    }


   

    public bool TryUseBomb()
    {
        if (!CanUseCombatActions())
            return false;

        if (currentBombCount <= 0)
        {
            Debug.Log("[Bomb] No bombs remaining!");
            return false;
        }

        if (PoolManager.Instance == null)
        {
            Debug.LogError("[Bomb] PoolManager not found!");
            return false;
        }

        if (explosionPrefab == null)
        {
            Debug.LogError("[Bomb] Explosion Prefab missing!");
            return false;
        }

        Camera targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogError("[Bomb] Main Camera not found!");
            return false;
        }


        Vector3 explosionPosition =
            ViewportToWorldPoint(
                targetCamera,
                0.5f,
                0.5f
            );


        GameObject explosion =
            PoolManager.Instance.Spawn(
                explosionPrefab,
                explosionPosition,
                Quaternion.identity
            );

        if (explosion == null)
            return false;


        BombExplosionEffect effect =
            explosion.GetComponent<BombExplosionEffect>();

        if (effect != null)
        {
            effect.ConfigureForCamera(targetCamera);
        }
        else
        {
            Debug.LogWarning(
                "[Bomb] BombExplosionEffect missing!",
                explosion
            );
        }


        

        SetBombCount(currentBombCount - 1);


        

        ApplyFullScreenDamage(targetCamera);

        Debug.Log(
            $"[Bomb] Used successfully. Remaining: {currentBombCount}",
            this
        );

        return true;
    }


    

    private void ApplyFullScreenDamage(Camera targetCamera)
    {
        Vector3 bottomLeft =
            ViewportToWorldPoint(targetCamera, 0f, 0f);

        Vector3 topRight =
            ViewportToWorldPoint(targetCamera, 1f, 1f);


        Collider2D[] hits = Physics2D.OverlapAreaAll(
            new Vector2(bottomLeft.x, bottomLeft.y),
            new Vector2(topRight.x, topRight.y),
            enemyLayers
        );


        processedTargets.Clear();

        int destroyedEnemies = 0;
        int damagedBosses = 0;


        foreach (Collider2D hit in hits)
        {
            if (hit == null || !IsEnemy(hit))
                continue;


            

            BossHealth boss =
                hit.GetComponentInParent<BossHealth>();

            if (boss != null)
            {
                if (processedTargets.Add(boss))
                {
                    float damage =
                        boss.MaxHealth * bossDamagePercent;

                    boss.TakeDamage(damage);

                    damagedBosses++;
                }

                continue;
            }


            IDamageable target =
                hit.GetComponentInParent<IDamageable>();

            if (target == null)
                continue;

            if (processedTargets.Add(target))
            {
                target.TakeDamage(InstantKillDamage);

                destroyedEnemies++;
            }
        }


        Debug.Log(
            $"[Bomb] Screen cleared. " +
            $"Destroyed: {destroyedEnemies}, " +
            $"Bosses damaged: {damagedBosses}",
            this
        );
    }


    private bool IsEnemy(Collider2D hit)
    {
        Transform current = hit.transform;

        while (current != null)
        {
            if (current.CompareTag("Enemy"))
                return true;

            current = current.parent;
        }

        return false;
    }


   

    private static Vector3 ViewportToWorldPoint(
        Camera targetCamera,
        float x,
        float y)
    {
        float distance = Mathf.Abs(
            targetCamera.transform.position.z - EnemyPlaneZ
        );

        Vector3 position =
            targetCamera.ViewportToWorldPoint(
                new Vector3(x, y, distance)
            );

        position.z = EnemyPlaneZ;

        return position;
    }


   

    [ContextMenu("Debug/Add One Bomb")]
    private void DebugAddOneBomb()
    {
        AddBombs(1);
    }


    private void OnValidate()
    {
        initialBombCount = Mathf.Max(
            0,
            initialBombCount
        );

        bossDamagePercent = Mathf.Clamp01(
            bossDamagePercent
        );
    }
}
