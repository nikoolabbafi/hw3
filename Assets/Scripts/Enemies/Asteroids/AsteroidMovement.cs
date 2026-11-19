
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PooledObject))]
public class AsteroidMovement : MonoBehaviour
{
    [Header("Movement Settings")]

    [SerializeField, Min(0.1f)]
    private float moveSpeed = 3f;

    [SerializeField, Min(0f)]
    private float rotationSpeed = 60f;


    [Header("Visual Reference")]

    [SerializeField]
    private Transform visual;


    private Rigidbody2D rb;

    private PooledObject pooledObject;

    private Camera targetCamera;

    private Quaternion originalRotation;

    private float rotationDirection;

    private bool initialized;

    private bool returnRequested;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        pooledObject = GetComponent<PooledObject>();

        if (visual != null)
        {
            originalRotation = visual.localRotation;
        }
    }


    private void OnEnable()
    {
        initialized = false;

        returnRequested = false;

        rotationDirection =
            Random.value < 0.5f ? -1f : 1f;

        if (visual != null)
        {
            visual.localRotation = originalRotation;
        }
    }


    public void Initialize(
        Camera camera,
        float speed)
    {
        targetCamera = camera;

        moveSpeed = Mathf.Max(
            0.1f,
            speed
        );

        rb.linearVelocity =
            Vector2.down * moveSpeed;

        initialized = true;
    }


    private void Update()
    {
        if (!initialized || returnRequested)
            return;

        RotateVisual();

        CheckScreenBounds();
    }


    private void RotateVisual()
    {
        if (visual == null)
            return;

        visual.Rotate(
            0f,
            0f,
            rotationSpeed *
            rotationDirection *
            Time.deltaTime
        );
    }


    private void CheckScreenBounds()
    {
        if (targetCamera == null)
        {
            returnRequested = true;

            return;
        }

        Vector3 viewportPosition =
            targetCamera.WorldToViewportPoint(
                transform.position
            );

        if (viewportPosition.y < -0.2f ||
            viewportPosition.x < -0.2f ||
            viewportPosition.x > 1.2f)
        {
            returnRequested = true;
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || returnRequested)
            return;

        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        player.TakeDamage(1);
        returnRequested = true;

        Debug.Log(
            "[Asteroid] Player collision!"
        );
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
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;

            rb.angularVelocity = 0f;
        }

        if (visual != null)
        {
            visual.localRotation = originalRotation;
        }

        initialized = false;

        returnRequested = false;

        targetCamera = null;
    }
}
