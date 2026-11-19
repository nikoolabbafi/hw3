
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Level Setup")]

    [SerializeField]
    private LevelDataSO levelData;

    [SerializeField]
    private Camera targetCamera;

    [SerializeField]
    private bool startOnPlay = true;

    [SerializeField, Min(0f)]
    private float timeBetweenWaves = 2f;


    [Header("Spawn Timing")]

    [SerializeField, Min(0f)]
    private float spawnDelay = 0.2f;




    [Header("Testing")]

    [SerializeField, Min(1)]
    private int startFromRound = 3;

    [SerializeField]
    private bool stopAfterFirstWave;


    [Header("Debug")]

    [SerializeField]
    private bool logProgress = true;


    private Coroutine levelRoutine;

    private bool isRunning;

    private bool gameOver;

    public bool IsRunning => isRunning;


    private void OnEnable()
    {
        GameEvents.PlayerDied += HandlePlayerDied;
        gameOver = GameManager.State == GameFlowState.GameOver;
    }


    private void OnDisable()
    {
        GameEvents.PlayerDied -= HandlePlayerDied;

        StopAllCoroutines();

        levelRoutine = null;
        isRunning = false;
    }


    private void HandlePlayerDied()
    {
        gameOver = true;

        StopAllCoroutines();

        levelRoutine = null;
        isRunning = false;

        if (logProgress)
        {
            Debug.LogWarning(
                "[WaveManager] Player died. All wave routines stopped.",
                this
            );
        }
    }


    private void Start()
    {
        if (startOnPlay)
        {
            StartLevel();
        }
    }


    public void StartLevel()
    {
        if (gameOver ||
            GameManager.State == GameFlowState.GameOver)
        {
            return;
        }

        if (isRunning)
            return;

        if (levelData == null)
        {
            Debug.LogError(
                "[WaveManager] LevelDataSO is missing!"
            );
            return;
        }

        if (PoolManager.Instance == null)
        {
            Debug.LogError(
                "[WaveManager] PoolManager not found!"
            );
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError(
                "[WaveManager] Main Camera not found!"
            );
            return;
        }

        levelRoutine = StartCoroutine(RunLevel());
    }



    private IEnumerator RunLevel()
    {
        isRunning = true;

        if (gameOver ||
            GameManager.State == GameFlowState.GameOver)
        {
            FinishRun();
            yield break;
        }

        WaveDataSO[] rounds = levelData.Rounds;

     

        if (!ValidateRoundList(rounds))
        {
            FinishRun();
            yield break;
        }

        int firstRoundIndex = Mathf.Clamp(
    startFromRound - 1, 0, rounds.Length - 1
);


        for (int i = firstRoundIndex; i < rounds.Length; i++)
        {
            WaveDataSO wave = rounds[i];

            int waveNumber = i + 1;


            if (logProgress)
            {
                Debug.Log(
                    $"[WaveManager] Starting: {wave.WaveName}"
                );
            }


         

            GameEvents.RaiseWaveStarted(waveNumber);

            if (logProgress)
            {
                Debug.Log(
                    $"[WaveManager] Event: WaveStarted({waveNumber})"
                );
            }


            bool isFinalBossRound =
                i == rounds.Length - 1 &&
                wave.BossPrefab != null;


         

            if (isFinalBossRound)
            {
                yield return BossEncounter.Run(
                    wave,
                    wave.BossHealth,
                    wave.BossEntrySpeed,
                    targetCamera
                );
            }



            else if (wave.Type == WaveType.AsteroidField)
            {
                yield return AsteroidWaveSpawner.Run(
                    wave,
                    targetCamera
                );
            }


            

            else if (wave.Type == WaveType.ChickenFormation)
            {
                if (!TryBuildWorldPath(
                    wave,
                    out Vector3[] worldPath))
                {
                    Debug.LogError(
                        "[WaveManager] Could not build chicken path!"
                    );

                    FinishRun();
                    yield break;
                }


                yield return StartCoroutine(
                    SpawnChickenWave(
                        wave,
                        worldPath
                    )
                );
            }


            else
            {
                Debug.LogError(
                    "[WaveManager] Unsupported wave type!"
                );

                FinishRun();
                yield break;
            }


            if (gameOver ||
                GameManager.State == GameFlowState.GameOver)
            {
                yield break;
            }



            GameEvents.RaiseWaveCompleted(waveNumber);

            if (logProgress)
            {
                Debug.Log(
                    $"[WaveManager] Event: WaveCompleted({waveNumber})"
                );
            }


        

            if (stopAfterFirstWave)
            {
                Debug.Log(
                    "[WaveManager] First wave test completed."
                );

                break;
            }


            

            if (i < rounds.Length - 1)
            {
                yield return new WaitForSeconds(
                    timeBetweenWaves
                );
            }
        }


        if (gameOver ||
            GameManager.State == GameFlowState.GameOver)
        {
            yield break;
        }

        if (!stopAfterFirstWave)
        {
            GameEvents.RaiseAllWavesCompleted();

            if (logProgress)
            {
                Debug.Log(
                    "[WaveManager] AllWavesCompleted event raised."
                );
            }

            LevelTransitionManager.CompleteLevel(levelData);
        }


        if (logProgress)
        {
            Debug.Log(
                "[WaveManager] Wave sequence finished."
            );
        }

        FinishRun();
    }


    private bool ValidateRoundList(WaveDataSO[] rounds)
    {
        if (rounds == null || rounds.Length == 0)
        {
            Debug.LogError(
                "[WaveManager] No rounds assigned to LevelData!"
            );

            return false;
        }


        for (int i = 0; i < rounds.Length; i++)
        {
            WaveDataSO wave = rounds[i];

            if (wave == null)
            {
                Debug.LogError(
                    $"[WaveManager] Round {i + 1} is missing!"
                );

                return false;
            }

            if (!wave.ValidateConfiguration(out string error))
            {
                Debug.LogError(
                    $"[WaveManager] Invalid round {i + 1}: {error}",
                    wave
                );

                return false;
            }



            if (wave.BossPrefab != null)
            {
                if (i != rounds.Length - 1)
                {
                    Debug.LogError(
                        "[WaveManager] Boss must be in the final round!"
                    );

                    return false;
                }

                if (wave.BossPrefab.GetComponent<Rigidbody2D>() == null)
                {
                    Debug.LogError(
                        "[WaveManager] Boss Rigidbody2D is missing!"
                    );

                    return false;
                }

                continue;
            }



            if (wave.Type == WaveType.ChickenFormation)
            {
                GameObject prefab = wave.EnemyPrefab;

                if (prefab == null)
                {
                    Debug.LogError(
                        $"[WaveManager] Enemy Prefab missing in {wave.WaveName}"
                    );

                    return false;
                }


                if (prefab.GetComponent<ChickenAI>() == null)
                {
                    Debug.LogError(
                        $"[WaveManager] ChickenAI missing in {wave.WaveName}"
                    );

                    return false;
                }


                if (prefab.GetComponent<EnemyHealth>() == null)
                {
                    Debug.LogError(
                        $"[WaveManager] EnemyHealth missing in {wave.WaveName}"
                    );

                    return false;
                }


                if (wave.EntryWaypointsViewport == null ||
                    wave.EntryWaypointsViewport.Length < 2)
                {
                    Debug.LogError(
                        $"[WaveManager] Invalid Entry Path in {wave.WaveName}"
                    );

                    return false;
                }


                if (wave.EggDropChance > 0f &&
                    prefab.GetComponent<ChickenEggDropper>() == null)
                {
                    Debug.LogError(
                        $"[WaveManager] ChickenEggDropper missing in {wave.WaveName}"
                    );

                    return false;
                }


                if (wave.DiveEveryNthChicken > 0 &&
                    prefab.GetComponent<ChickenDiveAttack>() == null)
                {
                    Debug.LogError(
                        $"[WaveManager] ChickenDiveAttack missing in {wave.WaveName}"
                    );

                    return false;
                }
            }



            else if (wave.Type == WaveType.AsteroidField)
            {
                if (!AsteroidWaveSpawner.Validate(
                    wave,
                    targetCamera))
                {
                    Debug.LogError(
                        $"[WaveManager] Invalid asteroid wave: {wave.WaveName}"
                    );

                    return false;
                }
            }


            else
            {
                Debug.LogError(
                    $"[WaveManager] Unsupported wave type in {wave.WaveName}"
                );

                return false;
            }
        }


        Debug.Log(
            $"[WaveManager] Validation successful! {rounds.Length} rounds found."
        );

        return true;
    }


    private bool TryBuildWorldPath(
        WaveDataSO wave,
        out Vector3[] worldWaypoints)
    {
        worldWaypoints = null;

        if (wave.EnemyPrefab == null)
        {
            Debug.LogError(
                "[WaveManager] Enemy Prefab is missing!"
            );

            return false;
        }

        if (wave.EnemyPrefab.GetComponent<ChickenAI>() == null)
        {
            Debug.LogError(
                "[WaveManager] ChickenAI is missing on prefab!"
            );

            return false;
        }

        if (wave.EnemyPrefab.GetComponent<EnemyHealth>() == null)
        {
            Debug.LogError(
                "[WaveManager] EnemyHealth is missing on prefab!"
            );

            return false;
        }


        Vector2[] viewportPoints =
            wave.EntryWaypointsViewport;

        if (viewportPoints == null ||
            viewportPoints.Length < 2)
        {
            Debug.LogError(
                "[WaveManager] Invalid Entry Waypoints!"
            );

            return false;
        }


        float targetZ =
            wave.EnemyPrefab.transform.position.z;

        float distanceFromCamera = Mathf.Abs(
            targetZ - targetCamera.transform.position.z
        );

        distanceFromCamera = Mathf.Max(
            distanceFromCamera,
            targetCamera.nearClipPlane + 0.01f
        );


        worldWaypoints =
            new Vector3[viewportPoints.Length];


        for (int i = 0; i < viewportPoints.Length; i++)
        {
            Vector3 worldPoint =
                targetCamera.ViewportToWorldPoint(
                    new Vector3(
                        viewportPoints[i].x,
                        viewportPoints[i].y,
                        distanceFromCamera
                    )
                );

            worldPoint.z = targetZ;

            worldWaypoints[i] = worldPoint;
        }

        return true;
    }


    private IEnumerator SpawnChickenWave(
        WaveDataSO wave,
        Vector3[] basePath)
    {
        int count = Mathf.Max(1, wave.EnemyCount);

        float spacing = Mathf.Max(
            0.1f,
            wave.FormationSpacing
        );


        if (wave.Formation == EnemyFormation.CustomPath)
        {
            if (wave.CustomFormationOffsets == null ||
                wave.CustomFormationOffsets.Length != count)
            {
                Debug.LogError(
                    "[WaveManager] Invalid Custom Formation Offsets!"
                );

                yield break;
            }
        }


        List<GameObject> spawnedEnemies =
            new List<GameObject>(count);

        ChickenFormationController formationController =
            new ChickenFormationController(
                targetCamera,
                wave.MoveSpeed,
                wave.FormationTravel,
                wave.DiveInterval
            );


        for (int i = 0; i < count; i++)
        {
            Vector3 formationOffset;

            if (wave.Formation == EnemyFormation.CustomPath)
            {
                Vector2 customOffset =
                    wave.CustomFormationOffsets[i];

                formationOffset = new Vector3(
                    customOffset.x,
                    customOffset.y,
                    0f
                );
            }
            else
            {
                formationOffset = GetFormationOffset(
                    i,
                    count,
                    wave.Formation,
                    spacing
                );
            }


            Vector3[] individualPath =
                new Vector3[basePath.Length];


            for (int j = 0; j < basePath.Length; j++)
            {
                individualPath[j] =
                    basePath[j] + formationOffset;
            }


            GameObject chicken =
                PoolManager.Instance.Spawn(
                    wave.EnemyPrefab,
                    individualPath[0],
                    Quaternion.identity
                );


            if (chicken == null)
                continue;


            ChickenAI chickenAI =
                chicken.GetComponent<ChickenAI>();


            if (chickenAI == null)
            {
                Debug.LogError(
                    "[WaveManager] Spawned chicken has no ChickenAI!"
                );

                PooledObject pooledObject =
                    chicken.GetComponent<PooledObject>();

                if (pooledObject != null)
                {
                    pooledObject.ReturnToPool();
                }

                continue;
            }


            EnemyHealth enemyHealth =
    chicken.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
            {
                Debug.LogError(
                    "[WaveManager] EnemyHealth missing on spawned chicken!"
                );

                chicken.GetComponent<PooledObject>()
                    ?.ReturnToPool();

                continue;
            }

            enemyHealth.Initialize(wave.EnemyMaxHealth);












            chickenAI.BeginEntryPath(
                individualPath,
                wave.MoveSpeed
            );

            Vector2 finalSlot =
                individualPath[individualPath.Length - 1];

            chickenAI.SetFormation(
                formationController,
                finalSlot
            );

            formationController.Register(chickenAI);


            int attackFrequency =
                wave.DiveEveryNthChicken;

            bool isDiveAttacker =
                attackFrequency > 0 &&
                count > 1 &&
                (
                    (i + 1) % attackFrequency == 0 ||
                    i == count - 1
                );


            ChickenEggDropper eggDropper =
                chicken.GetComponent<ChickenEggDropper>();

            if (eggDropper != null)
            {
                eggDropper.Configure(
                    wave.EggDropInterval,
                    wave.EggDropChance
                );
            }


            ChickenDiveAttack diveAttack =
                chicken.GetComponent<ChickenDiveAttack>();

            if (diveAttack != null)
            {
                diveAttack.Configure(
                    isDiveAttacker,
                    wave.DiveSpeed
                );

                if (isDiveAttacker && eggDropper != null)
                {
                    eggDropper.SetEggDropping(false);
                }
            }

            spawnedEnemies.Add(chicken);


          
            if (spawnDelay > 0f && i < count - 1)
            {
                yield return new WaitForSeconds(spawnDelay);
            }
        }


        if (logProgress)
        {
            Debug.Log(
                $"[WaveManager] Spawned {spawnedEnemies.Count}/{count} chickens."
            );
        }


        while (true)
        {
            formationController.Tick();

            if (!HasActiveEnemies(spawnedEnemies))
            {
                break;
            }

            yield return null;
        }


        if (logProgress)
        {
            Debug.Log(
                $"[WaveManager] Wave completed: {wave.WaveName}"
            );
        }
    }


    private Vector3 GetFormationOffset(
        int index,
        int count,
        EnemyFormation formation,
        float spacing)
    {
        switch (formation)
        {
            case EnemyFormation.Line:

                float lineX =
                    (index - (count - 1) * 0.5f) * spacing;

                return new Vector3(lineX, 0f, 0f);


            case EnemyFormation.VFormation:

                if (index == 0)
                    return Vector3.zero;

                int pair = (index + 1) / 2;

                float side =
                    index % 2 == 1 ? -1f : 1f;

                return new Vector3(
                    side * pair * spacing,
                    -pair * spacing * 0.65f,
                    0f
                );


            case EnemyFormation.CustomPath:

                Debug.LogError(
                    "[WaveManager] CustomPath must use its custom offsets."
                );

                return Vector3.zero;







            case EnemyFormation.Grid:
            default:

                int columns =
                    Mathf.CeilToInt(Mathf.Sqrt(count));

                int rows =
                    Mathf.CeilToInt((float)count / columns);

                int column = index % columns;
                int row = index / columns;

                float x =
                    (column - (columns - 1) * 0.5f) * spacing;

                float y =
                    ((rows - 1) * 0.5f - row) * spacing;

                return new Vector3(x, y, 0f);
        }
    }


    private bool HasActiveEnemies(
        List<GameObject> enemies)
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }


    private void FinishRun()
    {
        isRunning = false;
        levelRoutine = null;
    }


    private void OnValidate()
    {
        timeBetweenWaves = Mathf.Max(
            0f,
            timeBetweenWaves
        );

        spawnDelay = Mathf.Max(
            0f,
            spawnDelay
        );
    }
}
