using System.Collections;
using UnityEngine;

public static class BossEncounter
{
    public static IEnumerator Run(
        WaveDataSO wave,
        float health,
        float entrySpeed,
        Camera camera)
    {
        if (wave == null ||
            camera == null ||
            PoolManager.Instance == null)
            yield break;

        GameObject prefab = wave.BossPrefab;
        if (!IsValidPrefab(prefab))
            prefab = wave.EnemyPrefab;

        if (!IsValidPrefab(prefab))
        {
            Debug.LogError($"[Boss] No boss prefab assigned for {wave.WaveName}.");
            yield break;
        }

        const float bossDepth = 0f;
        Vector3 bossScale = Vector3.one * 2f;
        float cameraDistance = Mathf.Abs(
            camera.transform.position.z - bossDepth);
        Vector3 position = camera.ViewportToWorldPoint(
            new Vector3(0.5f, 1.1f, cameraDistance));
        position.z = bossDepth;

        GameObject boss = PoolManager.Instance.Spawn(
            prefab,
            position,
            Quaternion.identity);

        if (!IsActive(boss))
            yield break;

        boss.transform.localScale = bossScale;
        BossHealth bossHealth = boss.GetComponent<BossHealth>();
        if (bossHealth == null)
        {
            boss.AddComponent<BossHealth>();
            bossHealth = boss.GetComponent<BossHealth>();
        }

        bossHealth.Initialize(health);
        bossHealth.SetHealthBar(FindHealthBar());
        EnemyHealth regularHealth = boss.GetComponent<EnemyHealth>();
        if (regularHealth != null)
            regularHealth.enabled = false;

        ChickenDiveAttack dive = boss.GetComponent<ChickenDiveAttack>();
        if (dive != null)
            dive.enabled = false;

        ChickenEggDropper eggDropper = boss.GetComponent<ChickenEggDropper>();
        if (eggDropper != null)
            eggDropper.enabled = false;

        ChickenAI chickenAI = boss.GetComponent<ChickenAI>();
        if (chickenAI != null)
            chickenAI.enabled = false;

        ChickenVisualAnimation animation = boss.GetComponent<ChickenVisualAnimation>();
        if (animation != null)
            animation.enabled = false;

        BossAttackPattern bossAttacks = boss.GetComponent<BossAttackPattern>();
        if (bossAttacks == null)
            bossAttacks = boss.AddComponent<BossAttackPattern>();
        bossAttacks.Configure(BossProjectileReference.Prefab);
        bossAttacks.enabled = false;
        Rigidbody2D body = boss.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            float targetY = camera.ViewportToWorldPoint(
                new Vector3(0.5f, 0.78f, cameraDistance)).y;

            while (IsActive(boss) &&
                   body.position.y > targetY)
            {
                body.MovePosition(
                    body.position +
                    Vector2.down *
                    Mathf.Max(0.1f, entrySpeed) *
                    Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            if (IsActive(boss))
            {
                body.position = new Vector2(body.position.x, targetY);
                body.linearVelocity = Vector2.zero;
            }
        }

        if (!IsActive(boss))
            yield break;

        BossAttackPattern attacks = boss.GetComponent<BossAttackPattern>();
        if (attacks != null)
        {
            attacks.enabled = true;
            attacks.Begin();
        }

        while (IsActive(boss))
            yield return null;
    }

    private static bool IsActive(GameObject instance)
    {
        return instance != null && instance.activeInHierarchy;
    }

    private static bool IsValidPrefab(GameObject prefab)
    {
        
        return prefab != null;
    }

    private static UnityEngine.UI.Slider FindHealthBar()
    {
        UnityEngine.UI.Slider[] sliders =
            Object.FindObjectsByType<UnityEngine.UI.Slider>(
                FindObjectsSortMode.None);

        return sliders.Length > 0 ? sliders[0] : null;
    }
}
