
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ChickenAI))]
public class ChickenEggDropper : MonoBehaviour
{
    [Header("Egg References")]

    [SerializeField]
    private GameObject eggPrefab;

    [SerializeField]
    private Transform eggDropPoint;

    [Header("Egg Drop Settings")]

    [SerializeField, Min(0.1f)]
    private float dropInterval = 3f;

    [SerializeField, Range(0f, 1f)]
    private float dropChance = 0.35f;

    private ChickenAI chickenAI;

    private float timer;

    private bool canDropEggs;

    private void Awake()
    {
        chickenAI = GetComponent<ChickenAI>();
    }

    private void OnEnable()
    {
        timer = 0f;

        canDropEggs = false;
    }

    public void Configure(
        float interval,
        float chance)
    {
        dropInterval = Mathf.Max(
            0.1f,
            interval
        );

        dropChance = Mathf.Clamp01(chance);

        timer = Random.Range(
            dropInterval * 0.5f,
            dropInterval
        );

        canDropEggs = true;
    }

    private void Update()
    {
        if (!canDropEggs)
            return;

        if (chickenAI.IsEntering)
            return;

        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        timer = dropInterval;

        if (Random.value < dropChance)
        {
            DropEgg();
        }
    }

    private void DropEgg()
    {
        if (eggPrefab == null ||
            eggDropPoint == null ||
            PoolManager.Instance == null)
        {
            return;
        }

        GameObject egg =
            PoolManager.Instance.Spawn(
                eggPrefab,
                eggDropPoint.position,
                Quaternion.identity
            );

        if (egg == null)
            return;

        EggProjectile projectile =
            egg.GetComponent<EggProjectile>();

        if (projectile == null)
        {
            egg.GetComponent<PooledObject>()
                ?.ReturnToPool();

            return;
        }

        projectile.Launch(Vector2.down);
    }

    public void SetEggDropping(bool value)
    {
        canDropEggs = value;
    }

    private void OnDisable()
    {
        timer = 0f;

        canDropEggs = false;
    }
}
