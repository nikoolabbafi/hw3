
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]

    [SerializeField, Min(0f)]
    private float moveSpeed = 6f;


    [Header("Camera Boundaries")]

    [SerializeField]
    private bool clampToCamera = true;

    [SerializeField, Min(0f)]
    private float screenPadding = 0f;


    private Rigidbody2D rb;

    private PlayerInput playerInput;

    private PlayerHealth playerHealth;

    private Collider2D playerCollider;

    private SpriteRenderer playerSprite;

    private Camera mainCamera;

    private InputAction moveAction;

    private Vector2 moveInput;

    private bool boundsWarningShown;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        playerInput = GetComponent<PlayerInput>();

        playerHealth = GetComponent<PlayerHealth>();

        playerCollider = GetComponent<Collider2D>();

        playerSprite =
            GetComponentInChildren<SpriteRenderer>();

        mainCamera = Camera.main;


        if (mainCamera == null)
        {
            Debug.LogError(
                "Main Camera not found! Check its Tag.",
                this
            );

            enabled = false;
            return;
        }


        if (playerInput.actions == null)
        {
            Debug.LogError(
                "PlayerControls is not assigned!",
                this
            );

            enabled = false;
            return;
        }


        InputActionMap gameplayMap =
            playerInput.actions.FindActionMap(
                "Gameplay",
                true
            );

        moveAction = gameplayMap.FindAction(
            "Move",
            true
        );
    }


    private void Update()
    {
        if (!CanMove())
        {
            moveInput = Vector2.zero;
            return;
        }

        if (moveAction == null)
            return;

        moveInput = Vector2.ClampMagnitude(
            moveAction.ReadValue<Vector2>(),
            1f
        );
    }


    private void FixedUpdate()
    {
        if (!CanMove())
        {
            moveInput = Vector2.zero;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            return;
        }

        if (!clampToCamera)
        {
            rb.linearVelocity = moveInput * moveSpeed;
            return;
        }


        if (!TryGetCameraBounds(
            out float minX,
            out float maxX,
            out float minY,
            out float maxY))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }


       
        Vector2 currentPosition = rb.position;

        Vector2 clampedCurrent = ClampPosition(
            currentPosition,
            minX, maxX, minY, maxY
        );

        if ((clampedCurrent - currentPosition).sqrMagnitude
            > 0.000001f)
        {
            rb.position = clampedCurrent;
            currentPosition = clampedCurrent;
        }


        
        Vector2 desiredVelocity = moveInput * moveSpeed;

        Vector2 desiredPosition =
            currentPosition +
            desiredVelocity * Time.fixedDeltaTime;


        
        Vector2 clampedTarget = ClampPosition(
            desiredPosition,
            minX, maxX, minY, maxY
        );


        
        Vector2 allowedVelocity =
            (clampedTarget - currentPosition) /
            Time.fixedDeltaTime;

        rb.linearVelocity = allowedVelocity;
    }


    private bool CanMove()
    {
        if (GameManager.State != GameFlowState.Playing)
            return false;

        return playerHealth == null || !playerHealth.IsDead;
    }


    private bool TryGetCameraBounds(
        out float minX,
        out float maxX,
        out float minY,
        out float maxY)
    {
        float depth = Mathf.Abs(
            transform.position.z -
            mainCamera.transform.position.z
        );

        Rect cameraRect = mainCamera.pixelRect;


        Vector3 bottomLeft =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    cameraRect.xMin,
                    cameraRect.yMin,
                    depth
                )
            );

        Vector3 topRight =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    cameraRect.xMax,
                    cameraRect.yMax,
                    depth
                )
            );


        
        Bounds playerBounds = playerCollider.bounds;

        if (playerSprite != null)
        {
            playerBounds.Encapsulate(
                playerSprite.bounds
            );
        }


        Vector2 boundsOffset =
            (Vector2)playerBounds.center - rb.position;

        Vector2 extents = playerBounds.extents;


        minX = bottomLeft.x +
               extents.x +
               screenPadding -
               boundsOffset.x;

        maxX = topRight.x -
               extents.x -
               screenPadding -
               boundsOffset.x;

        minY = bottomLeft.y +
               extents.y +
               screenPadding -
               boundsOffset.y;

        maxY = topRight.y -
               extents.y -
               screenPadding -
               boundsOffset.y;


        if (minX > maxX || minY > maxY)
        {
            if (!boundsWarningShown)
            {
                Debug.LogError(
                    "Player is too large for the camera view. " +
                    "Reduce its scale or screen padding.",
                    this
                );

                boundsWarningShown = true;
            }

            return false;
        }

        return true;
    }


    private Vector2 ClampPosition(
        Vector2 position,
        float minX,
        float maxX,
        float minY,
        float maxY)
    {
        return new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY)
        );
    }


    private void OnDisable()
    {
        moveInput = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
