
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AsteroidWaveSpawner
{
    public static bool Validate(
        WaveDataSO wave,
        Camera targetCamera)
    {
        if (wave == null ||
            targetCamera == null ||
            PoolManager.Instance == null)
        {
            Debug.LogError(
                "[AsteroidWave] Missing required references!"
            );

            return false;
        }

        if (wave.AsteroidPrefab == null)
        {
            Debug.LogError(
                "[AsteroidWave] Asteroid Prefab is missing!"
            );

            return false;
        }

        if (wave.AsteroidPrefab
                .GetComponent<AsteroidHealth>() == null ||
            wave.AsteroidPrefab
                .GetComponent<AsteroidMovement>() == null)
        {
            Debug.LogError(
                "[AsteroidWave] Missing asteroid components!"
            );

            return false;
        }

        return true;
    }


    public static IEnumerator Run(
        WaveDataSO wave,
        Camera targetCamera)
    {
        int count = Mathf.Max(
            1,
            wave.AsteroidCount
        );

        float spawnInterval = Mathf.Max(
            0.1f,
            wave.AsteroidSpawnInterval
        );

        HashSet<GameObject> activeAsteroids =
            new HashSet<GameObject>();

        Debug.Log(
            $"[AsteroidWave] Starting: {wave.WaveName}"
        );

        for (int i = 0; i < count; i++)
        {
            GameObject asteroid = SpawnAsteroid(
                wave,
                targetCamera
            );

            if (asteroid != null)
            {
                activeAsteroids.Add(asteroid);
            }
            else
            {
                Debug.LogError(
                    "[AsteroidWave] Asteroid spawn failed!"
                );
            }

            if (i < count - 1)
            {
                yield return new WaitForSeconds(
                    spawnInterval
                );
            }
        }

        while (activeAsteroids.Count > 0)
        {
            activeAsteroids.RemoveWhere(
                asteroid =>
                    asteroid == null ||
                    !asteroid.activeInHierarchy
            );

            if (activeAsteroids.Count > 0)
            {
                yield return null;
            }
        }

        Debug.Log(
            "[AsteroidWave] Wave completed!"
        );
    }


    private static GameObject SpawnAsteroid(
        WaveDataSO wave,
        Camera targetCamera)
    {
        Vector3 spawnPosition = GetSpawnPosition(
            wave,
            targetCamera
        );

        GameObject asteroid =
            PoolManager.Instance.Spawn(
                wave.AsteroidPrefab,
                spawnPosition,
                Quaternion.identity
            );

        if (asteroid == null)
            return null;

        AsteroidHealth health =
            asteroid.GetComponent<AsteroidHealth>();

        AsteroidMovement movement =
            asteroid.GetComponent<AsteroidMovement>();

        health.Initialize(
            wave.AsteroidHealth
        );

        movement.Initialize(
            targetCamera,
            wave.AsteroidMoveSpeed
        );

        return asteroid;
    }


    private static Vector3 GetSpawnPosition(
        WaveDataSO wave,
        Camera targetCamera)
    {
        float randomX = Random.Range(
            0.12f,
            0.88f
        );

        float targetZ =
            wave.AsteroidPrefab.transform.position.z;

        float cameraDistance = Mathf.Abs(
            targetCamera.transform.position.z -
            targetZ
        );

        Vector3 position =
            targetCamera.ViewportToWorldPoint(
                new Vector3(
                    randomX,
                    1.15f,
                    cameraDistance
                )
            );

        position.z = targetZ;

        return position;
    }
}
