
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ChickenAI))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PooledObject))]
public class ChickenDiveAttack : MonoBehaviour
{
    [Header("Dive Settings")]

    [SerializeField, Min(0.1f)]
    private float diveSpeed = 7f;

    [SerializeField, Min(1f)]
    private float maxDiveDuration = 5f;

    private Rigidbody2D rb;
    private ChickenAI chickenAI;
    private PooledObject pooledObject;

    private Camera targetCamera;

    private Vector2 diveDirection;

    private float diveTimer;

    private bool assignedAttacker;
    private bool isDiving;
    private bool returnRequested;

    public bool IsDiving => isDiving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        chickenAI = GetComponent<ChickenAI>();

        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        targetCamera = Camera.main;

        assignedAttacker = false;
        isDiving = false;
        returnRequested = false;

        diveTimer = 0f;
        diveDirection = Vector2.zero;
    }

    public void Configure(
        bool canDive,
        float speed)
    {
        assignedAttacker = canDive;

        diveSpeed = Mathf.Max(0.1f, speed);
    }

    public bool TryStartDive(Transform target)
    {
        if (!assignedAttacker ||
            isDiving ||
            chickenAI.IsEntering ||
            target == null)
        {
            return false;
        }

        Vector2 direction =
            (Vector2)target.position - rb.position;

        if (direction.y >= -0.1f)
            return false;

        diveDirection = direction.normalized;

        diveTimer = 0f;

        isDiving = true;

        ChickenEggDropper eggDropper =
            GetComponent<ChickenEggDropper>();

        if (eggDropper != null)
        {
            eggDropper.SetEggDropping(false);
        }

        Debug.Log(
            $"[DiveAttack] {name} started diving!"
        );

        return true;
    }

    private void FixedUpdate()
    {
        if (!isDiving || returnRequested)
            return;

        diveTimer += Time.fixedDeltaTime;

        Vector2 nextPosition = rb.position +
            diveDirection *
            diveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);

        if (diveTimer >= maxDiveDuration ||
            IsOutsideScreen())
        {
            returnRequested = true;
        }
    }

    private bool IsOutsideScreen()
    {
        if (targetCamera == null)
            return false;

        Vector3 viewport =
            targetCamera.WorldToViewportPoint(
                transform.position
            );

        return viewport.y < -0.15f ||
               viewport.x < -0.15f ||
               viewport.x > 1.15f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDiving || returnRequested)
            return;

        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        player.TakeDamage(1);

        returnRequested = true;

        Debug.Log("[DiveAttack] Player collision!");
    }

    private void LateUpdate()
    {
        if (returnRequested)
        {
            pooledObject.ReturnToPool();
        }
    }

    private void OnDisable()
    {
        assignedAttacker = false;
        isDiving = false;
        returnRequested = false;

        diveTimer = 0f;
        diveDirection = Vector2.zero;
    }
}
