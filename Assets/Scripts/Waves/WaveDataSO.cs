
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWaveData",
    menuName = "Chicken Shooter/Data/Wave Data"
)]
public class WaveDataSO : ScriptableObject
{
    [Header("Wave Information")]

    [SerializeField]
    private string waveName = "Round 01";

    [SerializeField]
    private WaveType waveType =
        WaveType.ChickenFormation;



    [Header("Chicken Configuration")]

    [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField, Min(1)]
    private int enemyCount = 5;

    [SerializeField, Min(1f)]
    private float enemyMaxHealth = 8f;

    [SerializeField, Min(0.1f)]
    private float moveSpeed = 2f;

    [SerializeField, Min(0.1f)]
    private float eggDropInterval = 3f;

    [SerializeField, Range(0f, 1f)]
    private float eggDropChance = 0.35f;


   
    [Header("Formation Settings")]

    [SerializeField]
    private EnemyFormation formation =
        EnemyFormation.Grid;

    [SerializeField, Min(0.1f)]
    private float formationSpacing = 0.8f;

    [SerializeField, Min(0f)]
    private float formationTravel = 1.5f;

    [Tooltip(
        "Custom formation offsets relative to the final entry waypoint. " +
        "The array size must equal Enemy Count when using CustomPath."
    )]
    [SerializeField]
    private Vector2[] customFormationOffsets =
        new Vector2[0];


  

    [Header("Dive Attack Settings")]

    [SerializeField, Min(0)]
    private int diveEveryNthChicken = 3;

    [SerializeField, Min(0.1f)]
    private float diveInterval = 5f;

    [SerializeField, Min(0.1f)]
    private float diveSpeed = 7f;




    [Header("Entry Path")]

    [Tooltip(
        "Normalized camera viewport coordinates. " +
        "Each chicken follows this path with its formation offset."
    )]
    [SerializeField]
    private Vector2[] entryWaypointsViewport =
    {
        new Vector2(0.5f, 1.1f),
        new Vector2(0.5f, 0.8f)
    };


    

    [Header("Asteroid Configuration")]

    [SerializeField]
    private GameObject asteroidPrefab;

    [SerializeField, Min(1)]
    private int asteroidCount = 5;

    [SerializeField, Min(1)]
    private int asteroidHealth = 3;

    [SerializeField, Min(0.1f)]
    private float asteroidSpawnInterval = 0.8f;

    [SerializeField, Min(0.1f)]
    private float asteroidMoveSpeed = 3f;


   

    [Header("Boss Encounter")]

    [SerializeField]
    private GameObject bossPrefab;

    [SerializeField, Min(1f)]
    private float bossHealth = 100f;

    [SerializeField, Min(0.1f)]
    private float bossEntrySpeed = 2f;


  

    public string WaveName => waveName;

    public WaveType Type => waveType;


    // Chicken

    public GameObject EnemyPrefab => enemyPrefab;

    public int EnemyCount => enemyCount;

    public float EnemyMaxHealth => enemyMaxHealth;

    public float MoveSpeed => moveSpeed;

    public float EggDropInterval => eggDropInterval;

    public float EggDropChance => eggDropChance;


    // Formation

    public EnemyFormation Formation => formation;

    public float FormationSpacing => formationSpacing;

    public float FormationTravel => formationTravel;

    public Vector2[] CustomFormationOffsets =>
        customFormationOffsets;


  

    public int DiveEveryNthChicken =>
        diveEveryNthChicken;

    public float DiveInterval => diveInterval;

    public float DiveSpeed => diveSpeed;


  

    public Vector2[] EntryWaypointsViewport =>
        entryWaypointsViewport;


 

    public GameObject AsteroidPrefab => asteroidPrefab;

    public int AsteroidCount => asteroidCount;

    public int AsteroidHealth => asteroidHealth;

    public float AsteroidSpawnInterval =>
        asteroidSpawnInterval;

    public float AsteroidMoveSpeed =>
        asteroidMoveSpeed;


    // Boss

    public GameObject BossPrefab => bossPrefab;

    public float BossHealth => bossHealth;

    public float BossEntrySpeed => bossEntrySpeed;


    

    public bool ValidateConfiguration(out string error)
    {
        if (string.IsNullOrWhiteSpace(waveName))
        {
            return Fail(
                "Wave name is missing.",
                out error
            );
        }


        // Boss Wave

        if (bossPrefab != null)
        {
            if (bossHealth < 1f)
            {
                return Fail(
                    "Boss health must be greater than zero.",
                    out error
                );
            }

            if (bossPrefab.GetComponent<Rigidbody2D>() == null)
            {
                return Fail(
                    "Boss Rigidbody2D is missing.",
                    out error
                );
            }

            error = null;
            return true;
        }


        // Asteroid Wave

        if (waveType == WaveType.AsteroidField)
        {
            if (asteroidPrefab == null)
            {
                return Fail(
                    "Asteroid Prefab is missing.",
                    out error
                );
            }

            if (asteroidPrefab.GetComponent<AsteroidHealth>() == null)
            {
                return Fail(
                    "AsteroidHealth component is missing.",
                    out error
                );
            }

            if (asteroidPrefab.GetComponent<AsteroidMovement>() == null)
            {
                return Fail(
                    "AsteroidMovement component is missing.",
                    out error
                );
            }

            error = null;
            return true;
        }


        // Chicken Wave

        if (waveType != WaveType.ChickenFormation)
        {
            return Fail(
                "Unsupported Wave Type.",
                out error
            );
        }


        if (enemyPrefab == null)
        {
            return Fail(
                "Chicken Prefab is missing.",
                out error
            );
        }


        if (enemyPrefab.GetComponent<ChickenAI>() == null)
        {
            return Fail(
                "ChickenAI component is missing.",
                out error
            );
        }


        if (enemyPrefab.GetComponent<EnemyHealth>() == null)
        {
            return Fail(
                "EnemyHealth component is missing.",
                out error
            );
        }


        if (enemyCount < 1)
        {
            return Fail(
                "Enemy Count must be at least 1.",
                out error
            );
        }


        if (enemyMaxHealth < 1f)
        {
            return Fail(
                "Enemy Max Health must be at least 1.",
                out error
            );
        }


        if (moveSpeed <= 0f)
        {
            return Fail(
                "Move Speed must be greater than zero.",
                out error
            );
        }


        if (entryWaypointsViewport == null ||
            entryWaypointsViewport.Length < 2)
        {
            return Fail(
                "At least 2 entry waypoints are required.",
                out error
            );
        }


        if (eggDropChance > 0f)
        {
            if (enemyPrefab.GetComponent<ChickenEggDropper>() == null)
            {
                return Fail(
                    "ChickenEggDropper component is missing.",
                    out error
                );
            }
        }


        if (diveEveryNthChicken > 0 &&
            enemyCount > 1)
        {
            if (enemyPrefab.GetComponent<ChickenDiveAttack>() == null)
            {
                return Fail(
                    "ChickenDiveAttack component is missing.",
                    out error
                );
            }
        }



        if (formation == EnemyFormation.CustomPath)
        {
            if (customFormationOffsets == null ||
                customFormationOffsets.Length != enemyCount)
            {
                return Fail(
                    "Custom Formation Offsets size must equal Enemy Count.",
                    out error
                );
            }

            foreach (Vector2 offset in customFormationOffsets)
            {
                if (float.IsNaN(offset.x) ||
                    float.IsNaN(offset.y) ||
                    float.IsInfinity(offset.x) ||
                    float.IsInfinity(offset.y))
                {
                    return Fail(
                        "Custom Formation contains invalid coordinates.",
                        out error
                    );
                }
            }
        }


        error = null;
        return true;
    }


    private static bool Fail(
        string message,
        out string error)
    {
        error = message;
        return false;
    }


  

    [ContextMenu("Validate Wave Data")]
    private void ValidateWaveData()
    {
        if (ValidateConfiguration(out string error))
        {
            Debug.Log(
                $"[WaveDataSO] {waveName} validated successfully.",
                this
            );
        }
        else
        {
            Debug.LogError(
                $"[WaveDataSO] {waveName} invalid: {error}",
                this
            );
        }
    }


   

    private void OnValidate()
    {
        enemyCount = Mathf.Max(1, enemyCount);

        enemyMaxHealth = Mathf.Max(1f, enemyMaxHealth);

        moveSpeed = Mathf.Max(0.1f, moveSpeed);

        eggDropInterval = Mathf.Max(
            0.1f,
            eggDropInterval
        );

        eggDropChance = Mathf.Clamp01(
            eggDropChance
        );

        formationSpacing = Mathf.Max(
            0.1f,
            formationSpacing
        );

        formationTravel = Mathf.Max(
            0f,
            formationTravel
        );

        diveEveryNthChicken = Mathf.Max(
            0,
            diveEveryNthChicken
        );

        diveInterval = Mathf.Max(
            0.1f,
            diveInterval
        );

        diveSpeed = Mathf.Max(
            0.1f,
            diveSpeed
        );

        asteroidCount = Mathf.Max(1, asteroidCount);

        asteroidHealth = Mathf.Max(1, asteroidHealth);

        asteroidSpawnInterval = Mathf.Max(
            0.1f,
            asteroidSpawnInterval
        );

        asteroidMoveSpeed = Mathf.Max(
            0.1f,
            asteroidMoveSpeed
        );

        bossHealth = Mathf.Max(1f, bossHealth);

        bossEntrySpeed = Mathf.Max(
            0.1f,
            bossEntrySpeed
        );

        if (entryWaypointsViewport == null)
        {
            entryWaypointsViewport = new Vector2[0];
        }

        if (customFormationOffsets == null)
        {
            customFormationOffsets = new Vector2[0];
        }
    }
}
