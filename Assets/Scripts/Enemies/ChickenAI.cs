
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ChickenAI : MonoBehaviour
{
    [Header("Horizontal Movement")]

    [SerializeField]
    private float moveSpeed = 2f;

    [SerializeField]
    private float horizontalPadding = 0.8f;


    [Header("Wave Movement")]

    [SerializeField]
    private float waveAmplitude = 0.35f;

    [SerializeField]
    private float waveFrequency = 1.5f;


    [Header("Direction")]

    [SerializeField]
    private bool startMovingRight = true;


    private Rigidbody2D rb;
    private Camera mainCamera;

    private float startY;
    private float timer;
    private float phaseOffset;

    private int direction;


    // Entry Path

    private Vector2[] entryPath;
    private int entrySegment;
    private float entryProgress;
    private float entrySpeed;
    private bool isEntering;


    // Formation

    private ChickenFormationController formationController;

    private Vector2 formationSlot;


    // Dive Attack

    private ChickenDiveAttack diveAttack;


    // Public Properties

    public bool IsEntering => isEntering;

    public Vector2 FormationSlot => formationSlot;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        mainCamera = Camera.main;

        diveAttack = GetComponent<ChickenDiveAttack>();
    }


    private void OnEnable()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        mainCamera = Camera.main;

        startY = rb.position.y;
        timer = 0f;

        phaseOffset = Random.Range(
            0f,
            Mathf.PI * 2f
        );

        direction = startMovingRight ? 1 : -1;


        // Reset Entry Path

        entryPath = null;
        entrySegment = 0;
        entryProgress = 0f;
        isEntering = false;


       

        formationController = null;
        formationSlot = Vector2.zero;
    }


    public void BeginEntryPath(
        Vector3[] worldWaypoints,
        float speed)
    {
        if (worldWaypoints == null ||
            worldWaypoints.Length < 2)
        {
            Debug.LogWarning(
                "[ChickenAI] Invalid Entry Path!"
            );

            return;
        }

        entryPath = new Vector2[worldWaypoints.Length];

        for (int i = 0; i < worldWaypoints.Length; i++)
        {
            entryPath[i] = worldWaypoints[i];
        }

        entrySegment = 0;
        entryProgress = 0f;

        entrySpeed = Mathf.Max(0.01f, speed);

        moveSpeed = entrySpeed;

        rb.position = entryPath[0];

        isEntering = true;
    }


    public void SetFormation(
        ChickenFormationController controller,
        Vector2 slot)
    {
        formationController = controller;

        formationSlot = slot;
    }


    private void FixedUpdate()
    {
        // Entry Path Movement

        if (isEntering)
        {
            MoveAlongEntryPath();
            return;
        }


        // Dive Attack Movement

        if (diveAttack != null &&
            diveAttack.IsDiving)
        {
            return;
        }



        if (formationController != null)
        {
            rb.MovePosition(
                formationController.GetPosition(
                    formationSlot
                )
            );

            return;
        }


      

        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
                return;
        }

        timer += Time.fixedDeltaTime;

        float cameraDistance = Mathf.Abs(
            mainCamera.transform.position.z -
            transform.position.z
        );

        Vector3 leftEdge =
            mainCamera.ViewportToWorldPoint(
                new Vector3(0f, 0.5f, cameraDistance)
            );

        Vector3 rightEdge =
            mainCamera.ViewportToWorldPoint(
                new Vector3(1f, 0.5f, cameraDistance)
            );

        float leftLimit =
            leftEdge.x + horizontalPadding;

        float rightLimit =
            rightEdge.x - horizontalPadding;

        if (rightLimit <= leftLimit)
            return;

        Vector2 currentPosition = rb.position;

        if (currentPosition.x <= leftLimit)
            direction = 1;

        else if (currentPosition.x >= rightLimit)
            direction = -1;

        float nextX = currentPosition.x +
            direction * moveSpeed * Time.fixedDeltaTime;

        nextX = Mathf.Clamp(
            nextX,
            leftLimit,
            rightLimit
        );

        float nextY = startY +
            Mathf.Sin(
                timer * waveFrequency *
                Mathf.PI * 2f + phaseOffset
            ) * waveAmplitude;

        rb.MovePosition(
            new Vector2(nextX, nextY)
        );
    }


    private void MoveAlongEntryPath()
    {
        if (entryPath == null ||
            entryPath.Length < 2)
        {
            FinishEntryPath();
            return;
        }

        if (entrySegment >= entryPath.Length - 1)
        {
            FinishEntryPath();
            return;
        }

        Vector2 p1 = entryPath[entrySegment];
        Vector2 p2 = entryPath[entrySegment + 1];

        float segmentLength = Vector2.Distance(p1, p2);

        if (segmentLength < 0.001f)
        {
            entrySegment++;
            entryProgress = 0f;
            return;
        }

        entryProgress +=
            entrySpeed * Time.fixedDeltaTime / segmentLength;

        float t = Mathf.Clamp01(entryProgress);

        Vector2 p0 = entryPath[
            Mathf.Max(0, entrySegment - 1)
        ];

        Vector2 p3 = entryPath[
            Mathf.Min(entrySegment + 2, entryPath.Length - 1)
        ];

        Vector2 nextPosition = CatmullRom(
            p0, p1, p2, p3, t
        );

        rb.MovePosition(nextPosition);

        if (entryProgress >= 1f)
        {
            entrySegment++;
            entryProgress = 0f;

            if (entrySegment >= entryPath.Length - 1)
            {
                FinishEntryPath();
            }
        }
    }


    private Vector2 CatmullRom(
        Vector2 p0,
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 +
             4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 -
             3f * p2 + p3) * t3
        );
    }


    private void FinishEntryPath()
    {
        if (entryPath != null &&
            entryPath.Length > 0)
        {
            rb.position = entryPath[entryPath.Length - 1];
        }

        isEntering = false;

        startY = rb.position.y;
        timer = 0f;

        direction = startMovingRight ? 1 : -1;

        entryPath = null;
    }
}
