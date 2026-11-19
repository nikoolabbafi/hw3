using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossHealth))]
public class BossAttackPattern : MonoBehaviour
{
    private enum EmperorPhase
    {
        None = 0,
        PhaseOne = 1,
        PhaseTwo = 2,
        Enraged = 3
    }

    [Header("Standard Boss Attack")]
    [SerializeField, Min(0.5f)]
    private float attackInterval = 2.5f;

    [SerializeField, Min(0.1f)]
    private float projectileSpeed = 4f;

    [SerializeField]
    private GameObject projectilePrefab;

    [Header("Emperor Galaxy FSM")]
    [Tooltip("Enable only on the Emperor_Galaxy_Boss prefab. Other bosses keep the standard attack pattern.")]
    [SerializeField]
    private bool emperorGalaxyMode;

    [SerializeField]
    private GameObject minionPrefab;

    [SerializeField, Range(0.1f, 0.9f)]
    private float phaseTwoHealthThreshold = 0.6f;

    [SerializeField, Range(0.05f, 0.8f)]
    private float enragedHealthThreshold = 0.3f;

    [SerializeField, Min(0.2f)]
    private float phaseOneInterval = 2.3f;

    [SerializeField, Min(0.2f)]
    private float phaseTwoInterval = 1.45f;

    [SerializeField, Min(0.1f)]
    private float enragedInterval = 0.7f;

    [SerializeField, Min(0.1f)]
    private float phaseOneProjectileSpeed = 4.5f;

    [SerializeField, Min(0.1f)]
    private float phaseTwoProjectileSpeed = 5.2f;

    [SerializeField, Min(0.1f)]
    private float enragedProjectileSpeed = 6.2f;

    [Header("Enraged Minion Summon")]
    [SerializeField, Min(1f)]
    private float minionSpawnInterval = 5f;

    [SerializeField, Range(1, 4)]
    private int minionsPerSummon = 2;

    [SerializeField, Min(1f)]
    private float minionHealth = 10f;

    [SerializeField, Min(0.1f)]
    private float minionEntrySpeed = 3.5f;

    [SerializeField, Min(0.1f)]
    private float minionDiveInterval = 3f;

    [SerializeField, Min(0.1f)]
    private float minionDiveSpeed = 8.2f;

    [SerializeField, Min(0.1f)]
    private float minionEggDropInterval = 2.2f;

    [SerializeField, Range(0f, 1f)]
    private float minionEggDropChance = 0.2f;

    private BossHealth bossHealth;
    private float attackTimer;
    private float minionTimer;
    private bool hasStarted;
    private EmperorPhase currentPhase;
    private Camera targetCamera;

    private readonly List<GameObject> summonedMinions =
        new List<GameObject>();

    private ChickenFormationController minionFormation;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
    }

    private void OnEnable()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<BossHealth>();

        attackTimer = attackInterval;
        minionTimer = minionSpawnInterval;
        hasStarted = false;
        currentPhase = EmperorPhase.None;
        targetCamera = Camera.main;
        minionFormation = null;
        summonedMinions.Clear();
    }

    private void OnDisable()
    {
        CleanupSummonedMinions();
        hasStarted = false;
    }

    public void Begin()
    {
        hasStarted = true;
        attackTimer = emperorGalaxyMode
            ? phaseOneInterval
            : attackInterval;
        minionTimer = minionSpawnInterval;
        currentPhase = EmperorPhase.None;

        if (emperorGalaxyMode)
            UpdateEmperorPhase();
    }

    public void Configure(GameObject projectile)
    {
        if (projectilePrefab == null)
            projectilePrefab = projectile;
    }

    private void Update()
    {
        if (!hasStarted ||
            projectilePrefab == null ||
            PoolManager.Instance == null)
        {
            return;
        }

        if (!emperorGalaxyMode)
        {
            UpdateStandardBoss();
            return;
        }

        UpdateEmperorGalaxy();
    }

    private void UpdateStandardBoss()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        attackTimer = attackInterval;

        FireProjectileAtAngle(-12f, projectileSpeed);
        FireProjectileAtAngle(0f, projectileSpeed);
        FireProjectileAtAngle(12f, projectileSpeed);
    }

    private void UpdateEmperorGalaxy()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<BossHealth>();

        if (bossHealth == null)
            return;

        UpdateEmperorPhase();

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            attackTimer = GetCurrentAttackInterval();
            FireCurrentPhasePattern();
        }

        if (currentPhase == EmperorPhase.Enraged)
        {
            minionTimer -= Time.deltaTime;

            if (minionTimer <= 0f)
            {
                minionTimer = minionSpawnInterval;
                SummonMinions();
            }
        }

        if (minionFormation != null)
            minionFormation.Tick();

        RemoveInactiveMinionsFromList();
    }

    private void UpdateEmperorPhase()
    {
        if (bossHealth == null)
            return;

        float health = bossHealth.NormalizedHealth;
        EmperorPhase nextPhase;

        if (health <= enragedHealthThreshold)
            nextPhase = EmperorPhase.Enraged;
        else if (health <= phaseTwoHealthThreshold)
            nextPhase = EmperorPhase.PhaseTwo;
        else
            nextPhase = EmperorPhase.PhaseOne;

        if (nextPhase == currentPhase)
            return;

        currentPhase = nextPhase;
        attackTimer = GetCurrentAttackInterval();

        Debug.Log(
            "[Emperor Galaxy] Entered " + currentPhase +
            " at " + Mathf.RoundToInt(health * 100f) + "% health.",
            this
        );
    }

    private float GetCurrentAttackInterval()
    {
        switch (currentPhase)
        {
            case EmperorPhase.PhaseTwo:
                return phaseTwoInterval;

            case EmperorPhase.Enraged:
                return enragedInterval;

            default:
                return phaseOneInterval;
        }
    }

    private void FireCurrentPhasePattern()
    {
        switch (currentPhase)
        {
            case EmperorPhase.PhaseTwo:
                FireProjectileAtAngle(-28f, phaseTwoProjectileSpeed);
                FireProjectileAtAngle(-14f, phaseTwoProjectileSpeed);
                FireProjectileAtAngle(0f, phaseTwoProjectileSpeed);
                FireProjectileAtAngle(14f, phaseTwoProjectileSpeed);
                FireProjectileAtAngle(28f, phaseTwoProjectileSpeed);
                break;

            case EmperorPhase.Enraged:
                FireProjectileAtAngle(-36f, enragedProjectileSpeed);
                FireProjectileAtAngle(-24f, enragedProjectileSpeed);
                FireProjectileAtAngle(-12f, enragedProjectileSpeed);
                FireProjectileAtAngle(0f, enragedProjectileSpeed);
                FireProjectileAtAngle(12f, enragedProjectileSpeed);
                FireProjectileAtAngle(24f, enragedProjectileSpeed);
                FireProjectileAtAngle(36f, enragedProjectileSpeed);
                break;

            default:
                FireProjectileAtAngle(-12f, phaseOneProjectileSpeed);
                FireProjectileAtAngle(0f, phaseOneProjectileSpeed);
                FireProjectileAtAngle(12f, phaseOneProjectileSpeed);
                break;
        }
    }

    private void FireProjectileAtAngle(float angle, float speed)
    {
        Vector2 direction =
            Quaternion.Euler(0f, 0f, angle) * Vector2.down;

        Quaternion rotation =
            Quaternion.FromToRotation(Vector3.up, direction);

        GameObject projectile = PoolManager.Instance.Spawn(
            projectilePrefab,
            transform.position,
            rotation
        );

        if (projectile == null)
            return;

        EggProjectile egg = projectile.GetComponent<EggProjectile>();

        if (egg != null)
        {
            egg.Launch(direction, speed);
        }
        else
        {
            PooledObject pooled =
                projectile.GetComponent<PooledObject>();

            if (pooled != null)
                pooled.ReturnToPool();
            else
                projectile.SetActive(false);
        }
    }

    private void SummonMinions()
    {
        if (minionPrefab == null)
        {
            Debug.LogWarning(
                "[Emperor Galaxy] Minion Prefab is not assigned.",
                this
            );
            return;
        }

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        if (minionFormation == null)
        {
            minionFormation = new ChickenFormationController(
                targetCamera,
                1.6f,
                0.55f,
                minionDiveInterval
            );
        }

        float targetZ = minionPrefab.transform.position.z;
        float cameraDistance = Mathf.Abs(
            targetCamera.transform.position.z - targetZ
        );

        int count = Mathf.Clamp(minionsPerSummon, 1, 4);

        for (int i = 0; i < count; i++)
        {
            float viewportX = count == 1
                ? 0.5f
                : Mathf.Lerp(0.36f, 0.64f, i / (float)(count - 1));

            Vector3 spawn = targetCamera.ViewportToWorldPoint(
                new Vector3(viewportX, 1.12f, cameraDistance)
            );

            Vector3 middle = targetCamera.ViewportToWorldPoint(
                new Vector3(viewportX, 0.96f, cameraDistance)
            );

            Vector3 slot = targetCamera.ViewportToWorldPoint(
                new Vector3(viewportX, 0.78f, cameraDistance)
            );

            spawn.z = targetZ;
            middle.z = targetZ;
            slot.z = targetZ;

            GameObject minion = PoolManager.Instance.Spawn(
                minionPrefab,
                spawn,
                Quaternion.identity
            );

            if (minion == null)
                continue;

            EnemyHealth health = minion.GetComponent<EnemyHealth>();
            if (health != null)
                health.Initialize(minionHealth);

            ChickenAI chickenAI = minion.GetComponent<ChickenAI>();
            if (chickenAI == null)
            {
                minion.GetComponent<PooledObject>()?.ReturnToPool();
                continue;
            }

            ChickenEggDropper eggDropper =
                minion.GetComponent<ChickenEggDropper>();

            if (eggDropper != null)
            {
                eggDropper.Configure(
                    minionEggDropInterval,
                    minionEggDropChance
                );
            }

            ChickenDiveAttack diveAttack =
                minion.GetComponent<ChickenDiveAttack>();

            if (diveAttack != null)
                diveAttack.Configure(true, minionDiveSpeed);

            chickenAI.BeginEntryPath(
                new Vector3[] { spawn, middle, slot },
                minionEntrySpeed
            );

            chickenAI.SetFormation(minionFormation, slot);
            minionFormation.Register(chickenAI);
            summonedMinions.Add(minion);
        }

        Debug.Log(
            "[Emperor Galaxy] Summoned " + count + " cyber chicken minions.",
            this
        );
    }

    private void RemoveInactiveMinionsFromList()
    {
        for (int i = summonedMinions.Count - 1; i >= 0; i--)
        {
            GameObject minion = summonedMinions[i];

            if (minion == null || !minion.activeInHierarchy)
                summonedMinions.RemoveAt(i);
        }
    }

    private void CleanupSummonedMinions()
    {
        for (int i = summonedMinions.Count - 1; i >= 0; i--)
        {
            GameObject minion = summonedMinions[i];

            if (minion == null || !minion.activeInHierarchy)
                continue;

            PooledObject pooled = minion.GetComponent<PooledObject>();

            if (pooled != null)
                pooled.ReturnToPool();
            else
                minion.SetActive(false);
        }

        summonedMinions.Clear();
        minionFormation = null;
    }

    private void OnValidate()
    {
        phaseTwoHealthThreshold = Mathf.Clamp(
            phaseTwoHealthThreshold,
            0.1f,
            0.9f
        );

        enragedHealthThreshold = Mathf.Clamp(
            enragedHealthThreshold,
            0.05f,
            phaseTwoHealthThreshold - 0.01f
        );

        phaseOneInterval = Mathf.Max(0.2f, phaseOneInterval);
        phaseTwoInterval = Mathf.Max(0.2f, phaseTwoInterval);
        enragedInterval = Mathf.Max(0.1f, enragedInterval);
        minionSpawnInterval = Mathf.Max(1f, minionSpawnInterval);
        minionsPerSummon = Mathf.Clamp(minionsPerSummon, 1, 4);
        minionHealth = Mathf.Max(1f, minionHealth);
        minionEntrySpeed = Mathf.Max(0.1f, minionEntrySpeed);
        minionDiveInterval = Mathf.Max(0.1f, minionDiveInterval);
        minionDiveSpeed = Mathf.Max(0.1f, minionDiveSpeed);
        minionEggDropInterval = Mathf.Max(0.1f, minionEggDropInterval);
        minionEggDropChance = Mathf.Clamp01(minionEggDropChance);
    }
}
